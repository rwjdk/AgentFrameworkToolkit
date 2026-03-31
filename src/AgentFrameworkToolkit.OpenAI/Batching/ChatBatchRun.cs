using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using OpenAI.Files;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Represents a batch run and exposes helpers for retrieving matched requests, responses, and errors.
/// </summary>
[PublicAPI]
public class ChatBatchRun
{
    private readonly OpenAIFileClient? _fileClient;
    private readonly StructuredOutputSchemaDefinition? _structuredOutput;

    internal ChatBatchRun(OpenAIFileClient? fileClient, StructuredOutputSchemaDefinition? structuredOutput = null)
    {
        _fileClient = fileClient;
        _structuredOutput = structuredOutput;
    }

    /// <summary>
    /// Gets the batch identifier.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Status of batch run
    /// </summary>
    public BatchRunStatus Status
    {
        get =>
            StatusString switch
            {
                "validating" => BatchRunStatus.Validating,
                "failed" => BatchRunStatus.Failed,
                "in_progress" => BatchRunStatus.InProgress,
                "finalizing" => BatchRunStatus.Finalizing,
                "completed" => BatchRunStatus.Completed,
                "expired" => BatchRunStatus.Expired,
                "cancelling" => BatchRunStatus.Cancelling,
                "cancelled" => BatchRunStatus.Cancelled,
                _ => BatchRunStatus.Unknown
            };
    }

    /// <summary>
    /// Status of the batch.
    /// </summary>
    [JsonPropertyName("status")]
    internal string StatusString { get; set; } = string.Empty;

    /// <summary>
    /// Gets the request counts.
    /// </summary>
    [JsonPropertyName("request_counts")]
    public BatchCounts Counts { get; set; } = new()
    {
        Total = 0,
        Completed = 0,
        Failed = 0
    };

    /// <summary>
    /// Gets the input file id.
    /// </summary>
    [JsonPropertyName("input_file_id")]
    internal string? InputFileId { get; set; }

    /// <summary>
    /// Gets the batch endpoint.
    /// </summary>
    [JsonPropertyName("endpoint")]
    internal string? Endpoint { get; set; }

    /// <summary>
    /// Gets the output file id.
    /// </summary>
    [JsonPropertyName("output_file_id")]
    internal string? OutputFileId { get; set; }

    /// <summary>
    /// Gets the error file id.
    /// </summary>
    [JsonPropertyName("error_file_id")]
    internal string? ErrorFileId { get; set; }

    /// <summary>
    /// Gets the completed batch result joined by custom id.
    /// <param name="cleanUpRemoteFilesOnSuccessfulRetrieval">If the files involved in the batch should be removed on successful retrieval</param>
    /// </summary>
    /// <returns>
    /// A collection containing the original request together with the matched response and error for each line.
    /// Returns an empty collection when the batch is not yet completed.
    /// </returns>
    public async Task<IList<ChatBatchRunResult>> GetResultAsync(bool cleanUpRemoteFilesOnSuccessfulRetrieval = false)
    {
        if (!IsCompletedStatus(StatusString) || string.IsNullOrWhiteSpace(InputFileId))
        {
            return [];
        }

        (string inputFileContent, string? outputFileContent, string? errorFileContent) = await DownloadBatchFilesAsync();
        IList<ChatBatchRunResult> results = BuildResultItems(inputFileContent, outputFileContent, errorFileContent, Endpoint);
        if (cleanUpRemoteFilesOnSuccessfulRetrieval)
        {
            OpenAIFileClient fileClient = GetRequiredFileClient();
            if (!string.IsNullOrWhiteSpace(InputFileId))
            {
                await fileClient.DeleteFileAsync(InputFileId);
            }
            if (!string.IsNullOrWhiteSpace(OutputFileId))
            {
                await fileClient.DeleteFileAsync(OutputFileId);
            }
            if (!string.IsNullOrWhiteSpace(ErrorFileId))
            {
                await fileClient.DeleteFileAsync(ErrorFileId);
            }
        }

        return results;
    }

    internal static IList<ChatBatchRunResult> BuildResultItems(
        string inputFileContent,
        string? outputFileContent,
        string? errorFileContent,
        string? endpoint)
    {
        IReadOnlyList<ChatBatchRequest> requests = ParseRequestLines(inputFileContent);
        IReadOnlyDictionary<string, ChatBatchRunResponse> responses = ParseResultDictionary(outputFileContent, endpoint);
        IReadOnlyDictionary<string, ChatBatchRunError> errors = ParseErrorDictionary(errorFileContent);
        List<ChatBatchRunResult> results = [];

        foreach (ChatBatchRequest request in requests)
        {
            string customId = request.CustomId;
            responses.TryGetValue(customId, out ChatBatchRunResponse? response);
            errors.TryGetValue(customId, out ChatBatchRunError? error);

            results.Add(new ChatBatchRunResult
            {
                CustomId = customId,
                RequestMessages = request.Messages,
                ResponseMessage = response?.Message,
                Error = error
            });
        }

        return results;
    }

    internal static IList<ChatBatchRunResult<T>> BuildStructuredResultItems<T>(
        string inputFileContent,
        string? outputFileContent,
        string? errorFileContent,
        string? endpoint,
        StructuredOutputSchemaDefinition structuredOutput)
    {
        IReadOnlyList<ChatBatchRequest> requests = ParseRequestLines(inputFileContent);
        IReadOnlyDictionary<string, ChatBatchRunResponse<T>> responses = ParseStructuredResultDictionary<T>(outputFileContent, endpoint, structuredOutput);
        IReadOnlyDictionary<string, ChatBatchRunError> errors = ParseErrorDictionary(errorFileContent);
        List<ChatBatchRunResult<T>> results = [];

        foreach (ChatBatchRequest request in requests)
        {
            string customId = request.CustomId;
            responses.TryGetValue(customId, out ChatBatchRunResponse<T>? response);
            errors.TryGetValue(customId, out ChatBatchRunError? error);

            results.Add(new ChatBatchRunResult<T>
            {
                CustomId = customId,
                RequestMessages = request.Messages,
                ResponseMessage = response?.Message,
                ResponseObject = response == null ? default(T) : response.Result,
                Error = error
            });
        }

        return results;
    }

    internal static IReadOnlyList<ChatBatchRequest> ParseRequestLines(string fileContent)
    {
        List<ChatBatchRequest> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = InternalBatchRunner.ParseJsonObject(line);
            JsonObject bodyObject = lineObject["body"]?.AsObject()
                                    ?? throw new AgentFrameworkToolkitException("Batch input line was missing a request body.");
            string? endpoint = lineObject["url"]?.GetValue<string>();

            results.Add(new ChatBatchRequest
            {
                CustomId = lineObject["custom_id"]?.GetValue<string>() ?? Guid.NewGuid().ToString(),
                Messages = [.. ParseRequestMessages(bodyObject, endpoint)]
            });
        }

        return results;
    }

    internal static IReadOnlyList<ChatBatchRunResponse> ParseResultLines(string fileContent, string? endpoint)
    {
        List<ChatBatchRunResponse> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = InternalBatchRunner.ParseJsonObject(line);
            JsonObject responseObject = lineObject["response"]?.AsObject()
                                        ?? throw new AgentFrameworkToolkitException("Batch result line was missing a response object.");
            JsonObject bodyObject = responseObject["body"]?.AsObject()
                                    ?? throw new AgentFrameworkToolkitException("Batch result line was missing a response body.");

            results.Add(new ChatBatchRunResponse
            {
                CustomId = lineObject["custom_id"]?.GetValue<string>() ?? string.Empty,
                StatusCode = responseObject["status_code"]?.GetValue<int>() ?? 0,
                RequestId = responseObject["request_id"]?.GetValue<string>(),
                Message = ParseMessage(bodyObject, endpoint),
                RawBody = bodyObject.DeepClone() as JsonObject
            });
        }

        return results;
    }

    internal static IReadOnlyList<ChatBatchRunError> ParseErrorLines(string fileContent)
    {
        List<ChatBatchRunError> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = InternalBatchRunner.ParseJsonObject(line);
            JsonObject? errorObject = lineObject["error"] as JsonObject;
            JsonObject? responseObject = lineObject["response"] as JsonObject;
            JsonObject? bodyObject = responseObject?["body"] as JsonObject;

            results.Add(new ChatBatchRunError
            {
                CustomId = lineObject["custom_id"]?.GetValue<string>() ?? string.Empty,
                StatusCode = responseObject?["status_code"]?.GetValue<int>(),
                RequestId = responseObject?["request_id"]?.GetValue<string>(),
                ErrorCode = GetStringValue(errorObject?["code"]),
                ErrorMessage = GetStringValue(errorObject?["message"]),
                RawError = errorObject?.DeepClone() as JsonObject,
                RawBody = bodyObject?.DeepClone() as JsonObject
            });
        }

        return results;
    }

    internal static ChatBatchRun FromJson(JsonObject batchObject, OpenAIFileClient fileClient)
    {
        return PopulateFromJson(new ChatBatchRun(fileClient), batchObject);
    }

    internal static ChatBatchRun<T> FromJson<T>(JsonObject batchObject, OpenAIFileClient fileClient, StructuredOutputSchemaDefinition structuredOutput)
    {
        return PopulateFromJson(new ChatBatchRun<T>(fileClient, structuredOutput), batchObject);
    }

    internal static IReadOnlyList<ChatBatchRunResponse<T>> ParseStructuredResultLines<T>(
        string fileContent,
        string? endpoint,
        JsonSerializerOptions? serializerOptions = null)
    {
        StructuredOutputSchemaDefinition structuredOutput = StructuredOutputSchemaHelper.Create<T>(serializerOptions);
        return ParseStructuredResultLines<T>(fileContent, endpoint, structuredOutput);
    }

    internal static IReadOnlyList<ChatBatchRunResponse<T>> ParseStructuredResultLines<T>(
        string fileContent,
        string? endpoint,
        StructuredOutputSchemaDefinition structuredOutput)
    {
        List<ChatBatchRunResponse<T>> results = [];

        foreach (ChatBatchRunResponse resultLine in ParseResultLines(fileContent, endpoint))
        {
            results.Add(new ChatBatchRunResponse<T>
            {
                CustomId = resultLine.CustomId,
                StatusCode = resultLine.StatusCode,
                RequestId = resultLine.RequestId,
                Message = resultLine.Message,
                RawBody = resultLine.RawBody?.DeepClone() as JsonObject,
                Result = DeserializeStructuredResult<T>(resultLine, structuredOutput)
            });
        }

        return results;
    }

    private protected async Task<IList<ChatBatchRunResult<T>>> GetStructuredResultAsync<T>()
    {
        if (_structuredOutput == null)
        {
            throw new InvalidOperationException("This batch run was not created or retrieved with structured output metadata. Use RunChatBatchAsync<T>() or GetChatBatchAsync<T>().");
        }

        if (!IsCompletedStatus(StatusString) || string.IsNullOrWhiteSpace(InputFileId))
        {
            return [];
        }

        (string inputFileContent, string? outputFileContent, string? errorFileContent) = await DownloadBatchFilesAsync();
        return BuildStructuredResultItems<T>(inputFileContent, outputFileContent, errorFileContent, Endpoint, _structuredOutput);
    }

    private static T? DeserializeStructuredResult<T>(ChatBatchRunResponse response, StructuredOutputSchemaDefinition structuredOutput)
    {
        string json = ExtractStructuredResultJson(response);

        if (structuredOutput.IsWrappedInObject)
        {
            JsonObject wrappedObject = InternalBatchRunner.ParseJsonObject(json);
            JsonNode? dataNode = wrappedObject["data"];
            return dataNode == null
                ? default
                : dataNode.Deserialize<T>(structuredOutput.SerializerOptions);
        }

        return JsonSerializer.Deserialize<T>(json, structuredOutput.SerializerOptions);
    }

    private static string ExtractStructuredResultJson(ChatBatchRunResponse response)
    {
        if (response.RawBody == null)
        {
            throw new AgentFrameworkToolkitException($"Batch result line '{response.CustomId}' did not contain a response body.");
        }

        string? structuredJson = ExtractStructuredResultJson(response.RawBody);
        if (string.IsNullOrWhiteSpace(structuredJson))
        {
            throw new AgentFrameworkToolkitException($"Batch result line '{response.CustomId}' did not contain a structured JSON response.");
        }

        return structuredJson;
    }

    private static string? ExtractStructuredResultJson(JsonObject bodyObject)
    {
        if (bodyObject["choices"] is JsonArray choices)
        {
            JsonObject? messageObject = choices[0]?["message"] as JsonObject;
            return messageObject == null ? null : ExtractTextContent(messageObject["content"], "text");
        }

        if (bodyObject["output"] is not JsonArray output)
        {
            return null;
        }

        foreach (JsonNode? itemNode in output)
        {
            if (itemNode is not JsonObject itemObject || !string.Equals(itemObject["type"]?.GetValue<string>(), "message", StringComparison.Ordinal))
            {
                continue;
            }

            string? text = ExtractTextContent(itemObject["content"], "output_text", "input_text");
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return null;
    }

    private async Task<(string InputFileContent, string? OutputFileContent, string? ErrorFileContent)> DownloadBatchFilesAsync()
    {
        Task<string> inputTask = DownloadFileAsStringAsync(InputFileId!);
        Task<string?> outputTask = string.IsNullOrWhiteSpace(OutputFileId) ? Task.FromResult<string?>(null) : DownloadOptionalFileAsStringAsync(OutputFileId);
        Task<string?> errorTask = string.IsNullOrWhiteSpace(ErrorFileId) ? Task.FromResult<string?>(null) : DownloadOptionalFileAsStringAsync(ErrorFileId);

        await Task.WhenAll((Task)inputTask, outputTask, errorTask);
        return (await inputTask, await outputTask, await errorTask);
    }

    private async Task<string> DownloadFileAsStringAsync(string fileId)
    {
        System.ClientModel.ClientResult<BinaryData> download = await GetRequiredFileClient().DownloadFileAsync(fileId);
        return download.Value.ToString();
    }

    private async Task<string?> DownloadOptionalFileAsStringAsync(string? fileId)
    {
        return string.IsNullOrWhiteSpace(fileId) ? null : await DownloadFileAsStringAsync(fileId);
    }

    private OpenAIFileClient GetRequiredFileClient()
    {
        return _fileClient ?? throw new InvalidOperationException(
            "This batch run was not created with an OpenAIFileClient. Retrieve it through BatchRunner to enable file downloads.");
    }

    private static IReadOnlyList<ChatMessage> ParseRequestMessages(JsonObject bodyObject, string? endpoint)
    {
        if (bodyObject["messages"] is JsonArray chatMessages)
        {
            return ParseChatRequestMessages(chatMessages);
        }

        if (bodyObject["input"] is JsonArray responsesInput)
        {
            return ParseResponsesInputMessages(responsesInput);
        }

        ChatBatchClientType clientType = InternalBatchRunner.ParseClientType(endpoint);
        return clientType switch
        {
            ChatBatchClientType.ChatClient => [],
            ChatBatchClientType.ResponsesApi => [],
            _ => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
        };
    }

    private static ChatMessage? ParseMessage(JsonObject bodyObject, string? endpoint)
    {
        if (bodyObject["output"] is JsonArray)
        {
            return ParseResponsesMessages(bodyObject);
        }

        if (bodyObject["choices"] is JsonArray)
        {
            return ParseChatCompletionMessages(bodyObject);
        }

        ChatBatchClientType clientType = InternalBatchRunner.ParseClientType(endpoint);
        return clientType switch
        {
            ChatBatchClientType.ChatClient => ParseChatCompletionMessages(bodyObject),
            ChatBatchClientType.ResponsesApi => ParseResponsesMessages(bodyObject),
            _ => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
        };
    }

    private static IReadOnlyList<ChatMessage> ParseChatRequestMessages(JsonArray messagesArray)
    {
        List<ChatMessage> messages = [];

        foreach (JsonNode? messageNode in messagesArray)
        {
            if (messageNode is not JsonObject messageObject)
            {
                continue;
            }

            messages.Add(ParseChatCompletionMessage(messageObject));
        }

        return messages;
    }

    private static ChatMessage? ParseChatCompletionMessages(JsonObject bodyObject)
    {
        JsonArray? choices = bodyObject["choices"] as JsonArray;
        List<ChatMessage> messages = [];

        if (choices == null)
        {
            return null;
        }

        foreach (JsonNode? choiceNode in choices)
        {
            JsonObject? messageObject = choiceNode?["message"] as JsonObject;
            if (messageObject == null)
            {
                continue;
            }

            return ParseChatCompletionMessage(messageObject);
        }

        return null;
    }

    private static IReadOnlyList<ChatMessage> ParseResponsesInputMessages(JsonArray inputArray)
    {
        List<ChatMessage> messages = [];

        foreach (JsonNode? itemNode in inputArray)
        {
            if (itemNode is not JsonObject itemObject)
            {
                continue;
            }

            string? type = itemObject["type"]?.GetValue<string>();
            switch (type)
            {
                case "message":
                    messages.Add(ParseResponsesMessage(itemObject));
                    break;
                case "function_call":
                    messages.Add(new ChatMessage
                    {
                        Role = ChatRole.Assistant,
                        Contents =
                        [
                            new FunctionCallContent(
                                itemObject["call_id"]?.GetValue<string>() ?? string.Empty,
                                itemObject["name"]?.GetValue<string>() ?? string.Empty,
                                ParseArguments(itemObject["arguments"]))
                        ]
                    });
                    break;
                case "function_call_output":
                    messages.Add(new ChatMessage
                    {
                        Role = ChatRole.Tool,
                        Contents =
                        [
                            new TextContent(GetStringValue(itemObject["output"]) ?? string.Empty)
                        ]
                    });
                    break;
            }
        }

        return messages;
    }

    private static ChatMessage? ParseResponsesMessages(JsonObject bodyObject)
    {
        JsonArray? output = bodyObject["output"] as JsonArray;
        if (output == null)
        {
            return null;
        }

        foreach (JsonNode? itemNode in output)
        {
            if (itemNode is not JsonObject itemObject)
            {
                continue;
            }

            string? type = itemObject["type"]?.GetValue<string>();
            switch (type)
            {
                case "message":
                    return ParseResponsesMessage(itemObject);
            }
        }

        return null;
    }

    private static ChatMessage ParseChatCompletionMessage(JsonObject messageObject)
    {
        ChatMessage message = new()
        {
            Role = ParseRole(messageObject["role"]?.GetValue<string>()),
            Contents = []
        };

        JsonNode? contentNode = messageObject["content"];
        if (contentNode is JsonValue contentValue && contentValue.TryGetValue(out string? textContent) && !string.IsNullOrWhiteSpace(textContent))
        {
            message.Contents.Add(new TextContent(textContent));
        }
        else if (contentNode is JsonArray contentArray)
        {
            foreach (JsonNode? partNode in contentArray)
            {
                if (partNode is not JsonObject partObject)
                {
                    continue;
                }

                string? type = partObject["type"]?.GetValue<string>();
                if (string.Equals(type, "text", StringComparison.Ordinal))
                {
                    string? text = partObject["text"]?.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        message.Contents.Add(new TextContent(text));
                    }
                }
            }
        }

        if (messageObject["tool_calls"] is JsonArray toolCalls)
        {
            foreach (JsonNode? toolCallNode in toolCalls)
            {
                if (toolCallNode is not JsonObject toolCallObject)
                {
                    continue;
                }

                JsonObject? functionObject = toolCallObject["function"] as JsonObject;
                message.Contents.Add(new FunctionCallContent(
                    toolCallObject["id"]?.GetValue<string>() ?? string.Empty,
                    functionObject?["name"]?.GetValue<string>() ?? string.Empty,
                    ParseArguments(functionObject?["arguments"])));
            }
        }

        return message;
    }

    private static ChatMessage ParseResponsesMessage(JsonObject itemObject)
    {
        ChatMessage message = new()
        {
            Role = ParseRole(itemObject["role"]?.GetValue<string>()),
            Contents = []
        };

        JsonNode? contentNode = itemObject["content"];
        if (contentNode is JsonValue contentValue && contentValue.TryGetValue(out string? textContent) && !string.IsNullOrWhiteSpace(textContent))
        {
            message.Contents.Add(new TextContent(textContent));
            return message;
        }

        if (contentNode is not JsonArray contentArray)
        {
            return message;
        }

        foreach (JsonNode? contentItemNode in contentArray)
        {
            if (contentItemNode is not JsonObject contentItemObject)
            {
                continue;
            }

            string? type = contentItemObject["type"]?.GetValue<string>();
            switch (type)
            {
                case "output_text":
                case "input_text":
                    {
                        string? text = contentItemObject["text"]?.GetValue<string>();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            message.Contents.Add(new TextContent(text));
                        }

                        break;
                    }
                case "refusal":
                    {
                        string? refusal = contentItemObject["refusal"]?.GetValue<string>();
                        if (!string.IsNullOrWhiteSpace(refusal))
                        {
                            message.Contents.Add(new TextContent(refusal));
                        }

                        break;
                    }
            }
        }

        return message;
    }

    private static IEnumerable<string> EnumerateJsonLines(string fileContent)
    {
        return fileContent
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static line => !string.IsNullOrWhiteSpace(line));
    }

    private static IDictionary<string, object?> ParseArguments(JsonNode? argumentsNode)
    {
        if (argumentsNode == null)
        {
            return new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        if (argumentsNode is JsonValue jsonValue && jsonValue.TryGetValue(out string? argumentsText) && !string.IsNullOrWhiteSpace(argumentsText))
        {
            JsonObject argumentsObject = InternalBatchRunner.ParseJsonObject(argumentsText);
            return argumentsObject.Deserialize<Dictionary<string, object?>>() ??
                   new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        if (argumentsNode is JsonObject jsonObject)
        {
            return jsonObject.Deserialize<Dictionary<string, object?>>() ??
                   new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        return new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    private static string? ExtractTextContent(JsonNode? contentNode, params string[] allowedTypes)
    {
        if (contentNode is JsonValue contentValue && contentValue.TryGetValue(out string? textContent))
        {
            return textContent;
        }

        if (contentNode is not JsonArray contentArray)
        {
            return null;
        }

        List<string> textParts = [];
        foreach (JsonNode? partNode in contentArray)
        {
            if (partNode is not JsonObject partObject)
            {
                continue;
            }

            string? type = partObject["type"]?.GetValue<string>();
            if (type == null || !allowedTypes.Contains(type, StringComparer.Ordinal))
            {
                continue;
            }

            string? partText = partObject["text"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(partText))
            {
                textParts.Add(partText);
            }
        }

        return textParts.Count == 0 ? null : string.Concat(textParts);
    }

    private static string? GetStringValue(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        return node is JsonValue jsonValue && jsonValue.TryGetValue(out string? stringValue)
            ? stringValue
            : node.ToJsonString();
    }

    private static IReadOnlyDictionary<string, ChatBatchRunResponse> ParseResultDictionary(string? outputFileContent, string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(outputFileContent))
        {
            return new Dictionary<string, ChatBatchRunResponse>(StringComparer.Ordinal);
        }

        return ParseResultLines(outputFileContent, endpoint)
            .ToDictionary(line => line.CustomId, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, ChatBatchRunResponse<T>> ParseStructuredResultDictionary<T>(
        string? outputFileContent,
        string? endpoint,
        StructuredOutputSchemaDefinition structuredOutput)
    {
        if (string.IsNullOrWhiteSpace(outputFileContent))
        {
            return new Dictionary<string, ChatBatchRunResponse<T>>(StringComparer.Ordinal);
        }

        return ParseStructuredResultLines<T>(outputFileContent, endpoint, structuredOutput)
            .ToDictionary(line => line.CustomId, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, ChatBatchRunError> ParseErrorDictionary(string? errorFileContent)
    {
        if (string.IsNullOrWhiteSpace(errorFileContent))
        {
            return new Dictionary<string, ChatBatchRunError>(StringComparer.Ordinal);
        }

        return ParseErrorLines(errorFileContent)
            .ToDictionary(line => line.CustomId, StringComparer.Ordinal);
    }

    private static ChatRole ParseRole(string? role)
    {
        return role switch
        {
            "system" => ChatRole.System,
            "assistant" => ChatRole.Assistant,
            "tool" => ChatRole.Tool,
            _ => ChatRole.User
        };
    }

    private static bool IsCompletedStatus(string? status)
    {
        return string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase);
    }

    private static TBatchRun PopulateFromJson<TBatchRun>(TBatchRun batchRun, JsonObject batchObject)
        where TBatchRun : ChatBatchRun
    {
        batchRun.Id = batchObject["id"]?.GetValue<string>()
                           ?? throw new AgentFrameworkToolkitException("Batch response did not contain an id.");
        batchRun.StatusString = batchObject["status"]?.GetValue<string>()
                          ?? throw new AgentFrameworkToolkitException("Batch response did not contain a status.");
        batchRun.Counts = new BatchCounts
        {
            Total = batchObject["request_counts"]?["total"]?.GetValue<int>() ?? 0,
            Completed = batchObject["request_counts"]?["completed"]?.GetValue<int>() ?? 0,
            Failed = batchObject["request_counts"]?["failed"]?.GetValue<int>() ?? 0
        };
        batchRun.InputFileId = batchObject["input_file_id"]?.GetValue<string>();
        batchRun.Endpoint = batchObject["endpoint"]?.GetValue<string>();
        batchRun.OutputFileId = batchObject["output_file_id"]?.GetValue<string>();
        batchRun.ErrorFileId = batchObject["error_file_id"]?.GetValue<string>();
        return batchRun;
    }
}

/// <summary>
/// Represents a structured batch run and exposes typed helpers for retrieving matched results.
/// </summary>
/// <typeparam name="T">The structured output type returned for each line.</typeparam>
public class ChatBatchRun<T> : ChatBatchRun
{
    internal ChatBatchRun(OpenAIFileClient? fileClient, StructuredOutputSchemaDefinition structuredOutput)
        : base(fileClient, structuredOutput)
    {
    }

    /// <summary>
    /// Gets the completed structured batch result joined by custom id.
    /// </summary>
    /// <returns>
    /// A collection containing the original request together with the matched structured response and error for each line.
    /// Returns an empty collection when the batch is not yet completed.
    /// </returns>
    public Task<IList<ChatBatchRunResult<T>>> GetResultAsync()
    {
        return GetStructuredResultAsync<T>();
    }
}
