using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentFrameworkToolkit.OpenAI;
using Azure.Core;
using Microsoft.Extensions.AI;
using OpenAI.Batch;
using OpenAI.Files;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.AzureOpenAI.Batching;

/// <summary>
/// Azure OpenAI runner for batch jobs.
/// </summary>
public class BatchRunner
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false
    };

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
    public async Task<ChatBatchRun> RunChatBatchAsync(ChatBatchOptions options, IList<ChatBatchRequest> lines)
    {
        string batchId = await CreateBatchIdAsync(options, lines, null);
        return await GetChatBatchAsync(batchId);
    }

    /// <summary>
    /// Creates a new batch run with structured output for every line.
    /// </summary>
    /// <typeparam name="T">The structured output type returned for each line.</typeparam>
    /// <param name="options">Options applied to every entry in the batch.</param>
    /// <param name="lines">The batch entries to submit.</param>
    /// <param name="serializerOptions">Optional serializer options used for schema generation and result deserialization.</param>
    /// <returns>The created structured batch run.</returns>
    public async Task<ChatBatchRun<T>> RunChatBatchAsync<T>(ChatBatchOptions options, IList<ChatBatchRequest> lines, JsonSerializerOptions? serializerOptions = null)
    {
        StructuredOutputSchemaDefinition structuredOutput = StructuredOutputSchemaHelper.Create<T>(serializerOptions);
        ArgumentNullException.ThrowIfNull(structuredOutput);

        string batchId = await CreateBatchIdAsync(options, lines, structuredOutput);
        return await GetChatBatchAsync<T>(batchId, structuredOutput);
    }

    private async Task<string> CreateBatchIdAsync(ChatBatchOptions options, IList<ChatBatchRequest> lines, StructuredOutputSchemaDefinition? structuredOutput)
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

        string jsonl = BuildJsonl(options, lines, structuredOutput);
        byte[] jsonlBytes = new UTF8Encoding(false).GetBytes(jsonl);
        using MemoryStream jsonlStream = new(jsonlBytes);

        OpenAIFile inputFile = await fileClient.UploadFileAsync(jsonlStream, "batch.jsonl", new FileUploadPurpose("batch"));

        JsonObject batchPayload = new()
        {
            ["input_file_id"] = inputFile.Id,
            ["endpoint"] = GetEndpoint(options.ClientType),
            ["completion_window"] = "24h"
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

        return batchOperation.BatchId;
    }

    /// <summary>
    /// Gets an existing batch run.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <returns>The batch run.</returns>
    public Task<ChatBatchRun> GetChatBatchAsync(string batchId)
    {
        return GetBatchAsyncCoreAsync(batchId);
    }

    /// <summary>
    /// Gets an existing structured batch run.
    /// </summary>
    /// <typeparam name="T">The structured output type returned for each line.</typeparam>
    /// <param name="batchId">The batch identifier.</param>
    /// <param name="serializerOptions">Optional serializer options used for schema generation and result deserialization.</param>
    /// <returns>The structured batch run.</returns>
    public Task<ChatBatchRun<T>> GetChatBatchAsync<T>(string batchId, JsonSerializerOptions? serializerOptions = null)
    {
        StructuredOutputSchemaDefinition structuredOutput = StructuredOutputSchemaHelper.Create<T>(serializerOptions);
        return GetChatBatchAsync<T>(batchId, structuredOutput);
    }

    private async Task<ChatBatchRun> GetBatchAsyncCoreAsync(string batchId)
    {
        JsonObject batchObject = await GetBatchObjectAsync(batchId);
        return ChatBatchRun.FromJson(batchObject, Connection);
    }

    private async Task<ChatBatchRun<T>> GetChatBatchAsync<T>(string batchId, StructuredOutputSchemaDefinition structuredOutput)
    {
        ArgumentNullException.ThrowIfNull(structuredOutput);
        JsonObject batchObject = await GetBatchObjectAsync(batchId);

        return ChatBatchRun.FromJson<T>(batchObject, Connection, structuredOutput);
    }

    private async Task<JsonObject> GetBatchObjectAsync(string batchId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchId);

        Azure.AI.OpenAI.AzureOpenAIClient client = Connection.GetClient();
        BatchClient batchClient = client.GetBatchClient();

        ClientResult result = await batchClient.GetBatchAsync(batchId, new RequestOptions());
        return ParseJsonObject(result.GetRawResponse().Content.ToString());
    }

    internal static ChatBatchClientType ParseClientType(string? endpoint)
    {
        return string.Equals(endpoint, "/responses", StringComparison.OrdinalIgnoreCase)
            ? ChatBatchClientType.ResponsesApi
            : ChatBatchClientType.ChatClient;
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

    internal static string BuildJsonl(ChatBatchOptions options, IList<ChatBatchRequest> lines)
    {
        return BuildJsonl(options, lines, structuredOutput: null);
    }

    internal static string BuildJsonl<T>(ChatBatchOptions options, IList<ChatBatchRequest> lines, JsonSerializerOptions? serializerOptions = null)
    {
        StructuredOutputSchemaDefinition structuredOutput = StructuredOutputSchemaHelper.Create<T>(serializerOptions);
        return BuildJsonl(options, lines, structuredOutput);
    }

    internal static string BuildJsonl(ChatBatchOptions options, IList<ChatBatchRequest> lines, StructuredOutputSchemaDefinition? structuredOutput)
    {
        List<string> jsonLines = [];

        foreach (ChatBatchRequest line in lines)
        {
            JsonObject payload = new()
            {
                ["custom_id"] = string.IsNullOrWhiteSpace(line.CustomId) ? Guid.NewGuid().ToString() : line.CustomId,
                ["method"] = "POST",
                ["url"] = GetEndpoint(options.ClientType),
                ["body"] = BuildRequestBody(options, line, structuredOutput)
            };

            jsonLines.Add(payload.ToJsonString(JsonSerializerOptions));
        }

        return string.Join(Environment.NewLine, jsonLines);
    }

    internal static JsonObject BuildRequestBody(ChatBatchOptions options, ChatBatchRequest line, StructuredOutputSchemaDefinition? structuredOutput)
    {
        IList<ChatMessage> messages = GetMessages(options, line);
        JsonObject body = new()
        {
            ["model"] = options.Model
        };

        switch (options.ClientType)
        {
            case ChatBatchClientType.ChatClient:
                body["messages"] = BuildChatCompletionMessages(messages);
                ApplySharedChatCompletionOptions(body, options);
                ApplyStructuredOutput(body, options.ClientType, structuredOutput);
                break;
            case ChatBatchClientType.ResponsesApi:
                body["input"] = BuildResponsesInput(messages);
                ApplySharedResponsesOptions(body, options);
                ApplyStructuredOutput(body, options.ClientType, structuredOutput);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(options.ClientType), options.ClientType, null);
        }

        return body;
    }

    private static IList<ChatMessage> GetMessages(ChatBatchOptions options, ChatBatchRequest line)
    {
        if (!string.IsNullOrWhiteSpace(options.Instructions))
        {
            return
            [
                new ChatMessage(ChatRole.System, options.Instructions),
                ..line.Messages
            ];
        }

        return line.Messages;

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

            if (contentParts is [JsonObject firstPart] && string.Equals(firstPart["type"]?.GetValue<string>(), "text", StringComparison.Ordinal))
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
                ["content"] = contentParts is [JsonObject firstPart] &&
                              string.Equals(firstPart["type"]?.GetValue<string>(), "input_text", StringComparison.Ordinal)
                    ? firstPart["text"]?.GetValue<string>()
                    : contentParts
            });
        }

        return items;
    }

    private static void ApplySharedChatCompletionOptions(JsonObject body, ChatBatchOptions options)
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

    private static void ApplySharedResponsesOptions(JsonObject body, ChatBatchOptions options)
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

    private static void ApplyStructuredOutput(JsonObject body, ChatBatchClientType clientType, StructuredOutputSchemaDefinition? structuredOutput)
    {
        if (structuredOutput == null)
        {
            return;
        }

        if (!structuredOutput.ResponseFormat.Schema.HasValue)
        {
            throw new InvalidOperationException("Structured output requires a valid JSON schema.");
        }

        JsonElement normalizedSchema = StructuredOutputSchemaHelper.NormalizeObjectSchemas(structuredOutput.ResponseFormat.Schema.Value);
        JsonNode? schemaNode = StructuredOutputSchemaHelper.JsonElementToJsonNode(normalizedSchema);
        if (schemaNode is not JsonObject schemaObject)
        {
            throw new InvalidOperationException("Structured output schema root must be an object.");
        }

        switch (clientType)
        {
            case ChatBatchClientType.ChatClient:
                body["response_format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["json_schema"] = BuildChatJsonSchemaPayload(structuredOutput, schemaObject)
                };
                break;
            case ChatBatchClientType.ResponsesApi:
                body["text"] = new JsonObject
                {
                    ["format"] = BuildResponsesJsonSchemaPayload(structuredOutput, schemaObject)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null);
        }
    }

    private static JsonObject BuildChatJsonSchemaPayload(StructuredOutputSchemaDefinition structuredOutput, JsonObject schemaObject)
    {
        JsonObject payload = new()
        {
            ["name"] = structuredOutput.SchemaName,
            ["schema"] = schemaObject,
            ["strict"] = true
        };

        if (!string.IsNullOrWhiteSpace(structuredOutput.ResponseFormat.SchemaDescription))
        {
            payload["description"] = structuredOutput.ResponseFormat.SchemaDescription;
        }

        return payload;
    }

    private static JsonObject BuildResponsesJsonSchemaPayload(StructuredOutputSchemaDefinition structuredOutput, JsonObject schemaObject)
    {
        JsonObject payload = BuildChatJsonSchemaPayload(structuredOutput, schemaObject);
        payload["type"] = "json_schema";
        return payload;
    }

    private static string GetEndpoint(ChatBatchClientType clientType)
    {
        return clientType switch
        {
            ChatBatchClientType.ChatClient => "/chat/completions",
            ChatBatchClientType.ResponsesApi => "/responses",
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

    private static void ValidateBatchLines(IList<ChatBatchRequest> lines)
    {
        HashSet<string> customIds = new(StringComparer.Ordinal);

        foreach (ChatBatchRequest line in lines)
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