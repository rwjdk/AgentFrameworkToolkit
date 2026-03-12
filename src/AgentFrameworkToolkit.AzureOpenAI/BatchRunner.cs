using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentFrameworkToolkit;
using AgentFrameworkToolkit.OpenAI;
using Azure.Core;
using Microsoft.Extensions.AI;
using OpenAI.Batch;
using OpenAI.Files;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.AzureOpenAI;

/// <summary>
/// Azure OpenAI runner for batch jobs.
/// </summary>
public class BatchRunner
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false
    };

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRunner"/> class.
    /// </summary>
    /// <param name="endpoint">Your Azure OpenAI endpoint.</param>
    /// <param name="apiKey">Your Azure OpenAI API key.</param>
    public BatchRunner(string endpoint, string apiKey)
    {
        Connection = new AzureOpenAIConnection
        {
            Endpoint = endpoint,
            ApiKey = apiKey
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRunner"/> class.
    /// </summary>
    /// <param name="endpoint">Your Azure OpenAI endpoint.</param>
    /// <param name="credentials">Your RBAC credentials.</param>
    public BatchRunner(string endpoint, TokenCredential credentials)
    {
        Connection = new AzureOpenAIConnection
        {
            Endpoint = endpoint,
            Credentials = credentials
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRunner"/> class.
    /// </summary>
    /// <param name="connection">Connection details.</param>
    public BatchRunner(AzureOpenAIConnection connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Gets the Azure OpenAI connection.
    /// </summary>
    public AzureOpenAIConnection Connection { get; }

    /// <summary>
    /// Creates a new batch run.
    /// </summary>
    /// <param name="options">Options applied to every entry in the batch.</param>
    /// <param name="lines">The batch entries to submit.</param>
    /// <returns>The created batch run.</returns>
    public async Task<BatchRun> CreateBatchAsync(BatchRunOptions options, IList<BatchRunLine> lines)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(lines);

        if (lines.Count == 0)
        {
            throw new ArgumentException("At least one batch line must be provided.", nameof(lines));
        }

        ValidateBatchLines(lines);

        RawCallDetails? latestRawCall = null;
        Action<RawCallDetails>? rawCallDetails = null;
        if (options.RawHttpCallDetails != null)
        {
            rawCallDetails = details =>
            {
                latestRawCall = details;
                options.RawHttpCallDetails(details);
            };
        }

        Azure.AI.OpenAI.AzureOpenAIClient client = Connection.GetClient(rawCallDetails);
        OpenAIFileClient fileClient = client.GetOpenAIFileClient();
        BatchClient batchClient = client.GetBatchClient();

        string tempPath = Path.Combine(Path.GetTempPath(), $"aft-batch-{Guid.NewGuid():N}.jsonl");

        try
        {
            string jsonl = BuildJsonl(options, lines);
            await File.WriteAllTextAsync(tempPath, jsonl, Utf8WithoutBom);

            OpenAIFile inputFile = await fileClient.UploadFileAsync(tempPath, new FileUploadPurpose("batch"));

            JsonObject batchPayload = new()
            {
                ["input_file_id"] = inputFile.Id,
                ["endpoint"] = GetEndpoint(options.ClientType),
                ["completion_window"] = options.CompletionWindow
            };

            CreateBatchOperation batchOperation;
            try
            {
                batchOperation =
                    await batchClient.CreateBatchAsync(
                        BinaryContent.CreateJson(batchPayload),
                        waitUntilCompleted: options.WaitUntilCompleted);
            }
            catch (ClientResultException ex) when (ex.Status == 400)
            {
                string responseData = latestRawCall?.ResponseData ?? string.Empty;
                throw new AgentFrameworkToolkitException(
                    "Azure OpenAI rejected the batch request with HTTP 400. " +
                    "Common causes are using a non-batch deployment name, using the wrong batch endpoint, or uploading JSONL with invalid encoding/content. " +
                    $"Service response: {responseData}",
                    ex);
            }

            return await GetBatchAsync(batchOperation.BatchId);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    /// <summary>
    /// Gets an existing batch run.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <returns>The batch run.</returns>
    public async Task<BatchRun> GetBatchAsync(string batchId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchId);

        Azure.AI.OpenAI.AzureOpenAIClient client = Connection.GetClient();
        BatchClient batchClient = client.GetBatchClient();

        System.ClientModel.ClientResult result = await batchClient.GetBatchAsync(batchId, new RequestOptions());
        JsonObject batchObject = ParseJsonObject(result.GetRawResponse().Content.ToString());

        return BatchRun.FromJson(batchObject, Connection);
    }

    internal static BatchClientType ParseClientType(string? endpoint)
    {
        return string.Equals(endpoint, "/responses", StringComparison.OrdinalIgnoreCase)
            ? BatchClientType.ResponsesApi
            : BatchClientType.ChatClient;
    }

    internal static JsonObject ParseJsonObject(string json)
    {
        JsonNode? node = JsonNode.Parse(json);
        if (node is not JsonObject jsonObject)
        {
            throw new AgentFrameworkToolkitException("Expected a JSON object.");
        }

        return jsonObject;
    }

    internal static JsonArray ParseJsonArray(string json)
    {
        JsonNode? node = JsonNode.Parse(json);
        if (node is not JsonArray jsonArray)
        {
            throw new AgentFrameworkToolkitException("Expected a JSON array.");
        }

        return jsonArray;
    }

    internal static string BuildJsonl(BatchRunOptions options, IList<BatchRunLine> lines)
    {
        List<string> jsonLines = [];

        for (int i = 0; i < lines.Count; i++)
        {
            BatchRunLine line = lines[i];
            JsonObject payload = new()
            {
                ["custom_id"] = string.IsNullOrWhiteSpace(line.CustomId) ? $"line-{i}" : line.CustomId,
                ["method"] = "POST",
                ["url"] = GetEndpoint(options.ClientType),
                ["body"] = BuildRequestBody(options, line)
            };

            jsonLines.Add(payload.ToJsonString(JsonSerializerOptions));
        }

        return string.Join(Environment.NewLine, jsonLines);
    }

    internal static JsonObject BuildRequestBody(BatchRunOptions options, BatchRunLine line)
    {
        IList<ChatMessage> messages = GetMessages(options, line);
        JsonObject body = new()
        {
            ["model"] = options.Model
        };

        switch (options.ClientType)
        {
            case BatchClientType.ChatClient:
                body["messages"] = BuildChatCompletionMessages(messages);
                ApplySharedChatCompletionOptions(body, options);
                break;
            case BatchClientType.ResponsesApi:
                body["input"] = BuildResponsesInput(messages);
                ApplySharedResponsesOptions(body, options);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(options.ClientType), options.ClientType, null);
        }

        return body;
    }

    private static IList<ChatMessage> GetMessages(BatchRunOptions options, BatchRunLine line)
    {
        if (string.IsNullOrWhiteSpace(options.Instructions))
        {
            return line.Messages;
        }

        List<ChatMessage> messages =
        [
            new(ChatRole.System, options.Instructions)
        ];

        foreach (ChatMessage message in line.Messages)
        {
            messages.Add(message);
        }

        return messages;
    }

    private static JsonArray BuildChatCompletionMessages(IList<ChatMessage> messages)
    {
        JsonArray jsonMessages = [];

        foreach (ChatMessage message in messages)
        {
            JsonObject jsonMessage = new()
            {
                ["role"] = message.Role.Value
            };

            JsonArray contentParts = [];
            JsonArray toolCalls = [];

            foreach (AIContent content in message.Contents)
            {
                switch (content)
                {
                    case TextContent textContent:
                        contentParts.Add(new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = textContent.Text
                        });
                        break;
                    case DataContent dataContent:
                        contentParts.Add(new JsonObject
                        {
                            ["type"] = "image_url",
                            ["image_url"] = new JsonObject
                            {
                                ["url"] = GetDataContentUri(dataContent)
                            }
                        });
                        break;
                    case UriContent uriContent:
                        contentParts.Add(new JsonObject
                        {
                            ["type"] = "image_url",
                            ["image_url"] = new JsonObject
                            {
                                ["url"] = uriContent.Uri.ToString()
                            }
                        });
                        break;
                    case FunctionCallContent functionCallContent:
                        toolCalls.Add(new JsonObject
                        {
                            ["id"] = functionCallContent.CallId,
                            ["type"] = "function",
                            ["function"] = new JsonObject
                            {
                                ["name"] = functionCallContent.Name,
                                ["arguments"] = JsonSerializer.Serialize(functionCallContent.Arguments)
                            }
                        });
                        break;
                    case FunctionResultContent functionResultContent:
                        jsonMessage["tool_call_id"] = functionResultContent.CallId;
                        contentParts.Add(new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = SerializeResult(functionResultContent.Result)
                        });
                        break;
                }
            }

            if (toolCalls.Count > 0)
            {
                jsonMessage["tool_calls"] = toolCalls;
            }

            if (contentParts.Count == 1 &&
                contentParts[0] is JsonObject firstPart &&
                string.Equals(firstPart["type"]?.GetValue<string>(), "text", StringComparison.Ordinal))
            {
                jsonMessage["content"] = firstPart["text"]?.GetValue<string>();
            }
            else if (contentParts.Count > 0)
            {
                jsonMessage["content"] = contentParts;
            }
            else
            {
                jsonMessage["content"] = null;
            }

            jsonMessages.Add(jsonMessage);
        }

        return jsonMessages;
    }

    private static JsonArray BuildResponsesInput(IList<ChatMessage> messages)
    {
        JsonArray input = [];

        foreach (ChatMessage message in messages)
        {
            List<JsonObject> items = BuildResponsesItems(message);
            foreach (JsonObject item in items)
            {
                input.Add(item);
            }
        }

        return input;
    }

    private static List<JsonObject> BuildResponsesItems(ChatMessage message)
    {
        List<JsonObject> items = [];
        JsonArray contentParts = [];

        foreach (AIContent content in message.Contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    contentParts.Add(new JsonObject
                    {
                        ["type"] = "input_text",
                        ["text"] = textContent.Text
                    });
                    break;
                case DataContent dataContent:
                    contentParts.Add(new JsonObject
                    {
                        ["type"] = "input_image",
                        ["image_url"] = GetDataContentUri(dataContent)
                    });
                    break;
                case UriContent uriContent:
                    contentParts.Add(new JsonObject
                    {
                        ["type"] = "input_image",
                        ["image_url"] = uriContent.Uri.ToString()
                    });
                    break;
                case FunctionCallContent functionCallContent:
                    items.Add(new JsonObject
                    {
                        ["type"] = "function_call",
                        ["call_id"] = functionCallContent.CallId,
                        ["name"] = functionCallContent.Name,
                        ["arguments"] = JsonSerializer.Serialize(functionCallContent.Arguments)
                    });
                    break;
                case FunctionResultContent functionResultContent:
                    items.Add(new JsonObject
                    {
                        ["type"] = "function_call_output",
                        ["call_id"] = functionResultContent.CallId,
                        ["output"] = SerializeResult(functionResultContent.Result)
                    });
                    break;
            }
        }

        if (contentParts.Count > 0)
        {
            items.Insert(0, new JsonObject
            {
                ["type"] = "message",
                ["role"] = message.Role.Value,
                ["content"] = contentParts.Count == 1 &&
                              contentParts[0] is JsonObject firstPart &&
                              string.Equals(firstPart["type"]?.GetValue<string>(), "input_text", StringComparison.Ordinal)
                    ? firstPart["text"]?.GetValue<string>()
                    : contentParts
            });
        }

        return items;
    }

    private static void ApplySharedChatCompletionOptions(JsonObject body, BatchRunOptions options)
    {
        if (options.MaxOutputTokens.HasValue)
        {
            body["max_completion_tokens"] = options.MaxOutputTokens.Value;
        }

        if (options.Temperature.HasValue)
        {
            body["temperature"] = options.Temperature.Value;
        }

        if (options.ReasoningEffort.HasValue)
        {
            body["reasoning_effort"] = ToReasoningEffortString(options.ReasoningEffort.Value);
        }

        if (options.ServiceTier.HasValue)
        {
            body["service_tier"] = ToServiceTierString(options.ServiceTier.Value);
        }
    }

    private static void ApplySharedResponsesOptions(JsonObject body, BatchRunOptions options)
    {
        if (options.MaxOutputTokens.HasValue)
        {
            body["max_output_tokens"] = options.MaxOutputTokens.Value;
        }

        if (options.Temperature.HasValue)
        {
            body["temperature"] = options.Temperature.Value;
        }

        if (options.ReasoningEffort.HasValue || options.ReasoningSummaryVerbosity.HasValue)
        {
            JsonObject reasoning = new();

            if (options.ReasoningEffort.HasValue)
            {
                reasoning["effort"] = ToReasoningEffortString(options.ReasoningEffort.Value);
            }

            if (options.ReasoningSummaryVerbosity.HasValue)
            {
                reasoning["summary"] = ToReasoningSummaryVerbosityString(options.ReasoningSummaryVerbosity.Value);
            }

            body["reasoning"] = reasoning;
        }

        if (options.ServiceTier.HasValue)
        {
            body["service_tier"] = ToServiceTierString(options.ServiceTier.Value);
        }
    }

    private static string GetEndpoint(BatchClientType clientType)
    {
        return clientType switch
        {
            BatchClientType.ChatClient => "/chat/completions",
            BatchClientType.ResponsesApi => "/responses",
            _ => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
        };
    }

    private static string SerializeResult(object? result)
    {
        return result switch
        {
            null => string.Empty,
            string text => text,
            _ => JsonSerializer.Serialize(result)
        };
    }

    private static string GetDataContentUri(DataContent dataContent)
    {
        if (!string.IsNullOrWhiteSpace(dataContent.Uri))
        {
            return dataContent.Uri;
        }

        string base64 = Convert.ToBase64String(dataContent.Data.ToArray());
        return $"data:{dataContent.MediaType};base64,{base64}";
    }

    private static string ToReasoningEffortString(OpenAIReasoningEffort reasoningEffort)
    {
        return reasoningEffort switch
        {
            OpenAIReasoningEffort.None => "none",
            OpenAIReasoningEffort.Minimal => "minimal",
            OpenAIReasoningEffort.Low => "low",
            OpenAIReasoningEffort.Medium => "medium",
            OpenAIReasoningEffort.High => "high",
            OpenAIReasoningEffort.ExtraHigh => "xhigh",
            _ => throw new ArgumentOutOfRangeException(nameof(reasoningEffort), reasoningEffort, null)
        };
    }

    private static string ToReasoningSummaryVerbosityString(OpenAIReasoningSummaryVerbosity verbosity)
    {
        return verbosity switch
        {
            OpenAIReasoningSummaryVerbosity.Auto => "auto",
            OpenAIReasoningSummaryVerbosity.Concise => "concise",
            OpenAIReasoningSummaryVerbosity.Detailed => "detailed",
            _ => throw new ArgumentOutOfRangeException(nameof(verbosity), verbosity, null)
        };
    }

    private static string ToServiceTierString(OpenAIServiceTier serviceTier)
    {
        return serviceTier switch
        {
            OpenAIServiceTier.Auto => "auto",
            OpenAIServiceTier.Flex => "flex",
            OpenAIServiceTier.Default => "default",
            OpenAIServiceTier.Priority => "priority",
            _ => throw new ArgumentOutOfRangeException(nameof(serviceTier), serviceTier, null)
        };
    }

    private static void ValidateBatchLines(IList<BatchRunLine> lines)
    {
        HashSet<string> customIds = new(StringComparer.Ordinal);

        foreach (BatchRunLine line in lines)
        {
            ArgumentNullException.ThrowIfNull(line);

            if (line.Messages.Count == 0)
            {
                throw new ArgumentException("Every batch line must contain at least one chat message.", nameof(lines));
            }

            if (!string.IsNullOrWhiteSpace(line.CustomId) && !customIds.Add(line.CustomId))
            {
                throw new ArgumentException($"Duplicate batch custom id '{line.CustomId}' detected.", nameof(lines));
            }
        }
    }
}

/// <summary>
/// Options for a batch run.
/// </summary>
public class BatchRunOptions
{
    /// <summary>
    /// Gets or sets the model or deployment name used for every line in the batch.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets the endpoint style to use for the batch.
    /// </summary>
    public BatchClientType ClientType { get; set; } = BatchClientType.ChatClient;

    /// <summary>
    /// Gets or sets a value indicating whether the create call should wait for the service-side operation.
    /// </summary>
    public bool WaitUntilCompleted { get; set; }

    /// <summary>
    /// Gets or sets the batch completion window. Azure currently supports <c>24h</c>.
    /// </summary>
    public string CompletionWindow { get; set; } = "24h";

    /// <summary>
    /// Gets or sets instructions that are prepended to every batch line as a system message.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of output tokens per request.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Gets or sets the temperature used for generation.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the reasoning effort when using reasoning-capable models.
    /// </summary>
    public OpenAIReasoningEffort? ReasoningEffort { get; set; }

    /// <summary>
    /// Gets or sets the reasoning summary verbosity for the Responses API.
    /// </summary>
    public OpenAIReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }

    /// <summary>
    /// Gets or sets the service tier to use when supported by the model.
    /// </summary>
    public OpenAIServiceTier? ServiceTier { get; set; }

    /// <summary>
    /// Gets or sets an action for inspecting the raw HTTP calls made during upload and batch creation.
    /// </summary>
    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }
}

/// <summary>
/// Represents one request line in the batch input file.
/// </summary>
public class BatchRunLine
{
    /// <summary>
    /// Gets or sets the optional custom id for the line. If omitted, one is generated.
    /// </summary>
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets or set the messages sent as the request payload.
    /// </summary>
    public required IList<ChatMessage> Messages { get; set; }
}

/// <summary>
/// The batch endpoint style to use.
/// </summary>
public enum BatchClientType
{
    /// <summary>
    /// Uses the Chat Completions batch endpoint.
    /// </summary>
    ChatClient,

    /// <summary>
    /// Uses the Responses API batch endpoint.
    /// </summary>
    ResponsesApi
}
