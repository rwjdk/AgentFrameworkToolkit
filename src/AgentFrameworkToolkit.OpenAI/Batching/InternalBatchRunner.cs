using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using OpenAI.Batch;
using OpenAI.Files;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Runner for batch jobs backed by raw OpenAI batch and file clients.
/// </summary>
internal class InternalBatchRunner
{
    private readonly bool _azureOpenAi;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAIBatchRunner"/> class.
    /// </summary>
    /// <param name="batchClient">The batch client used for creating and retrieving batch jobs.</param>
    /// <param name="fileClient">The file client used for uploading input files and downloading results.</param>
    /// <param name="azureOpenAi"></param>
    public InternalBatchRunner(BatchClient batchClient, OpenAIFileClient fileClient, bool azureOpenAi)
    {
        _azureOpenAi = azureOpenAi;
        ArgumentNullException.ThrowIfNull(batchClient);
        ArgumentNullException.ThrowIfNull(fileClient);

        BatchClient = batchClient;
        FileClient = fileClient;
    }

    /// <summary>
    /// Gets the batch client.
    /// </summary>
    private BatchClient BatchClient { get; }

    /// <summary>
    /// Gets the file client.
    /// </summary>
    private OpenAIFileClient FileClient { get; }

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

    /// <summary>
    /// Creates a new embedding batch run.
    /// </summary>
    /// <param name="options">Options applied to every entry in the batch.</param>
    /// <param name="lines">The batch entries to submit.</param>
    /// <returns>The created embedding batch run.</returns>
    public async Task<EmbeddingBatchRun> RunEmbeddingBatchAsync(EmbeddingBatchOptions options, IList<EmbeddingBatchRequest> lines)
    {
        string batchId = await CreateBatchIdAsync(options, lines);
        return await GetEmbeddingBatchAsync(batchId);
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

        string jsonl = BuildJsonl(options, lines, structuredOutput);
        try
        {
            return await CreateBatchIdAsync(jsonl, GetEndpoint(options.ClientType), options.WaitUntilCompleted);
        }
        catch (ClientResultException ex) when (ex.Status == 400)
        {
            throw new AgentFrameworkToolkitException(
                "The batch service rejected the batch request with HTTP 400. " +
                "Common causes are using a non-batch deployment name, using the wrong batch endpoint, or uploading JSONL with invalid encoding/content.",
                ex);
        }
    }

    private async Task<string> CreateBatchIdAsync(EmbeddingBatchOptions options, IList<EmbeddingBatchRequest> lines)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(lines);

        if (lines.Count == 0)
        {
            throw new ArgumentException("At least one batch line must be provided.", nameof(lines));
        }

        ValidateBatchLines(lines);

        string jsonl = BuildJsonl(options, lines);
        try
        {
            return await CreateBatchIdAsync(jsonl, EmbeddingsEndpoint, options.WaitUntilCompleted);
        }
        catch (ClientResultException ex) when (ex.Status == 400)
        {
            throw new AgentFrameworkToolkitException(
                "The batch service rejected the embeddings batch request with HTTP 400. " +
                "Common causes are using a non-batch deployment name, using the wrong batch endpoint, or uploading JSONL with invalid encoding/content.",
                ex);
        }
    }

    private async Task<string> CreateBatchIdAsync(
        string jsonl,
        string endpoint,
        bool waitUntilCompleted)
    {
        byte[] jsonlBytes = new UTF8Encoding(false).GetBytes(jsonl);
        using MemoryStream jsonlStream = new(jsonlBytes);

        OpenAIFile inputFile = await FileClient.UploadFileAsync(jsonlStream, "batch.jsonl", new FileUploadPurpose("batch"));

        JsonObject batchPayload = new()
        {
            ["input_file_id"] = inputFile.Id,
            ["endpoint"] = endpoint,
            ["completion_window"] = "24h"
        };

        CreateBatchOperation batchOperation =
            await BatchClient.CreateBatchAsync(
                BinaryContent.CreateJson(batchPayload),
                waitUntilCompleted: waitUntilCompleted);

        return batchOperation.BatchId;
    }

    /// <summary>
    /// Gets an existing batch run.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <returns>The batch run.</returns>
    public async Task<ChatBatchRun> GetChatBatchAsync(string batchId)
    {
        JsonObject batchObject = await GetBatchObjectAsync(batchId);
        return ChatBatchRun.FromJson(batchObject, FileClient);
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

    /// <summary>
    /// Gets an existing embedding batch run.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <returns>The embedding batch run.</returns>
    public async Task<EmbeddingBatchRun> GetEmbeddingBatchAsync(string batchId)
    {
        JsonObject batchObject = await GetBatchObjectAsync(batchId);
        return EmbeddingBatchRun.FromJson(batchObject, FileClient);
    }

    private async Task<ChatBatchRun<T>> GetChatBatchAsync<T>(string batchId, StructuredOutputSchemaDefinition structuredOutput)
    {
        ArgumentNullException.ThrowIfNull(structuredOutput);
        JsonObject batchObject = await GetBatchObjectAsync(batchId);

        return ChatBatchRun.FromJson<T>(batchObject, FileClient, structuredOutput);
    }

    private async Task<JsonObject> GetBatchObjectAsync(string batchId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchId);

        ClientResult result = await BatchClient.GetBatchAsync(batchId, new RequestOptions());
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

    private string BuildJsonl(ChatBatchOptions options, IList<ChatBatchRequest> lines, StructuredOutputSchemaDefinition? structuredOutput)
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

    private static string BuildJsonl(EmbeddingBatchOptions options, IList<EmbeddingBatchRequest> lines)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(lines);

        List<string> jsonLines = [];

        foreach (EmbeddingBatchRequest line in lines)
        {
            JsonObject payload = new()
            {
                ["custom_id"] = string.IsNullOrWhiteSpace(line.CustomId) ? Guid.NewGuid().ToString() : line.CustomId,
                ["method"] = "POST",
                ["url"] = EmbeddingsEndpoint,
                ["body"] = BuildRequestBody(options, line)
            };

            jsonLines.Add(payload.ToJsonString(JsonSerializerOptions));
        }

        return string.Join(Environment.NewLine, jsonLines);
    }

    private static JsonObject BuildRequestBody(ChatBatchOptions options, ChatBatchRequest line, StructuredOutputSchemaDefinition? structuredOutput)
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

    private static JsonObject BuildRequestBody(EmbeddingBatchOptions options, EmbeddingBatchRequest line)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(line);

        JsonObject body = new()
        {
            ["model"] = options.Model,
            ["input"] = line.Value
        };

        ApplyEmbeddingGenerationOptions(body, options);
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
        if (options.ReasoningEffort.HasValue)
        {
            body["reasoning_effort"] = ToReasoningEffortString(options.ReasoningEffort.Value);
        }
    }

    private static void ApplySharedResponsesOptions(JsonObject body, ChatBatchOptions options)
    {
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

    private static void ApplyEmbeddingGenerationOptions(JsonObject body, EmbeddingBatchOptions options)
    {
        EmbeddingGenerationOptions? generationOptions = options.GenerationOptions;

        if (!string.IsNullOrWhiteSpace(generationOptions?.ModelId) &&
            !string.Equals(generationOptions.ModelId, options.Model, StringComparison.Ordinal))
        {
            throw new ArgumentException("GenerationOptions.ModelId must match the batch Model when both are provided.", nameof(options));
        }

        if (generationOptions?.Dimensions is { } dimensions)
        {
            body["dimensions"] = dimensions;
        }

        if (generationOptions?.AdditionalProperties == null)
        {
            return;
        }

        foreach (KeyValuePair<string, object?> property in generationOptions.AdditionalProperties)
        {
            if (body.ContainsKey(property.Key))
            {
                continue;
            }

            body[property.Key] = SerializeToJsonNode(property.Value);
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

    private string GetEndpoint(ChatBatchClientType clientType)
    {
        if (_azureOpenAi)
        {
            return clientType switch
            {
                ChatBatchClientType.ChatClient => "/chat/completions",
                ChatBatchClientType.ResponsesApi => "/responses",
                _ => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
            };
        }
        return clientType switch
        {
            ChatBatchClientType.ChatClient => "/v1/chat/completions",
            ChatBatchClientType.ResponsesApi => "/v1/responses",
            _ => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
        };

    }

    private static JsonNode? SerializeToJsonNode(object? value)
    {
        if (value is JsonNode node)
        {
            return node.DeepClone();
        }

        if (value == null)
        {
            return null;
        }

        return JsonSerializer.SerializeToNode(value, value.GetType());
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

    private static void ValidateBatchLines(IList<EmbeddingBatchRequest> lines)
    {
        HashSet<string> customIds = new(StringComparer.Ordinal);

        foreach (EmbeddingBatchRequest line in lines)
        {
            ArgumentNullException.ThrowIfNull(line);

            if (string.IsNullOrWhiteSpace(line.Value))
            {
                throw new ArgumentException("Embedding batch input values cannot be null or empty.", nameof(lines));
            }

            if (!string.IsNullOrWhiteSpace(line.CustomId) && !customIds.Add(line.CustomId))
            {
                throw new ArgumentException($"Duplicate batch custom id '{line.CustomId}' detected.", nameof(lines));
            }
        }
    }

    private const string EmbeddingsEndpoint = "/v1/embeddings";
}
