using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AgentFrameworkToolkit;
using Microsoft.Extensions.AI;
using OpenAI.Files;

namespace AgentFrameworkToolkit.AzureOpenAI;

/// <summary>
/// Represents a batch run and exposes helpers for retrieving matched requests, responses, and errors.
/// </summary>
public class BatchRun
{
    private readonly AzureOpenAIConnection _connection;
    private readonly StructuredOutputSchemaDefinition? _structuredOutput;

    internal BatchRun(AzureOpenAIConnection connection, StructuredOutputSchemaDefinition? structuredOutput = null)
    {
        _connection = connection;
        _structuredOutput = structuredOutput;
    }

    /// <summary>
    /// Gets the batch identifier.
    /// </summary>
    public string BatchId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the batch status.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the request counts.
    /// </summary>
    [JsonPropertyName("request_counts")]
    public BatchResponseCounts Counts { get; internal set; } = new()
    {
        Total = 0,
        Completed = 0,
        Failed = 0
    };

    /// <summary>
    /// Gets the input file id.
    /// </summary>
    [JsonPropertyName("input_file_id")]
    public string? InputFileId { get; internal set; }

    /// <summary>
    /// Gets the batch endpoint.
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; internal set; }

    /// <summary>
    /// Gets the output file id.
    /// </summary>
    [JsonPropertyName("output_file_id")]
    public string? OutputFileId { get; internal set; }

    /// <summary>
    /// Gets the error file id.
    /// </summary>
    [JsonPropertyName("error_file_id")]
    public string? ErrorFileId { get; internal set; }

    /// <summary>
    /// Gets the completed batch result joined by custom id.
    /// </summary>
    /// <returns>
    /// A collection containing the original request together with the matched response and error for each line.
    /// Returns an empty collection when the batch is not yet completed.
    /// </returns>
    public Task<IReadOnlyList<BatchRunItem>> GetResult()
    {
        return GetResultAsync();
    }

    /// <summary>
    /// Gets the completed batch result joined by custom id.
    /// </summary>
    /// <returns>
    /// A collection containing the original request together with the matched response and error for each line.
    /// Returns an empty collection when the batch is not yet completed.
    /// </returns>
    public async Task<IReadOnlyList<BatchRunItem>> GetResultAsync()
    {
        if (!IsCompletedStatus(Status) || string.IsNullOrWhiteSpace(InputFileId))
        {
            return [];
        }

        (string inputFileContent, string? outputFileContent, string? errorFileContent) = await DownloadBatchFilesAsync();
        return BuildResultItems(inputFileContent, outputFileContent, errorFileContent, Endpoint);
    }

    /// <summary>
    /// Downloads the successful output lines and converts them back into chat messages.
    /// </summary>
    /// <returns>The parsed output lines.</returns>
    public async Task<IReadOnlyList<BatchRunResultLine>> DownloadResultsAsync()
    {
        if (string.IsNullOrWhiteSpace(OutputFileId))
        {
            return [];
        }

        string fileContent = await DownloadFileAsStringAsync(OutputFileId);
        return ParseResultLines(fileContent, Endpoint);
    }

    /// <summary>
    /// Downloads the successful output lines and converts them into structured results.
    /// </summary>
    /// <typeparam name="T">The structured output type returned for each line.</typeparam>
    /// <returns>The parsed output lines.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the batch run was not created or retrieved with structured output metadata.</exception>
    public async Task<IReadOnlyList<BatchRunStructuredResultLine<T>>> DownloadStructuredResultsAsync<T>()
    {
        if (_structuredOutput == null)
        {
            throw new InvalidOperationException("This batch run was not created with structured output metadata. Use CreateBatchAsync<T>() or GetBatchAsync<T>().");
        }

        if (string.IsNullOrWhiteSpace(OutputFileId))
        {
            return [];
        }

        string fileContent = await DownloadFileAsStringAsync(OutputFileId);
        return ParseStructuredResultLines<T>(fileContent, Endpoint, _structuredOutput);
    }

    /// <summary>
    /// Downloads the failed output lines.
    /// </summary>
    /// <returns>The parsed error lines.</returns>
    public async Task<IReadOnlyList<BatchRunErrorLine>> DownloadErrorsAsync()
    {
        if (string.IsNullOrWhiteSpace(ErrorFileId))
        {
            return [];
        }

        string fileContent = await DownloadFileAsStringAsync(ErrorFileId);
        return ParseErrorLines(fileContent);
    }

    internal static IReadOnlyList<BatchRunItem> BuildResultItems(
        string inputFileContent,
        string? outputFileContent,
        string? errorFileContent,
        string? endpoint)
    {
        IReadOnlyList<BatchRunLine> requests = ParseRequestLines(inputFileContent);
        IReadOnlyDictionary<string, BatchRunResultLine> responses = ParseResultDictionary(outputFileContent, endpoint);
        IReadOnlyDictionary<string, BatchRunErrorLine> errors = ParseErrorDictionary(errorFileContent);
        List<BatchRunItem> results = [];

        foreach (BatchRunLine request in requests)
        {
            string customId = request.CustomId ?? string.Empty;
            responses.TryGetValue(customId, out BatchRunResultLine? response);
            errors.TryGetValue(customId, out BatchRunErrorLine? error);

            results.Add(new BatchRunItem
            {
                Request = request,
                Response = response,
                Error = error
            });
        }

        return results;
    }

    internal static IReadOnlyList<BatchRunItem<T>> BuildStructuredResultItems<T>(
        string inputFileContent,
        string? outputFileContent,
        string? errorFileContent,
        string? endpoint,
        StructuredOutputSchemaDefinition structuredOutput)
    {
        IReadOnlyList<BatchRunLine> requests = ParseRequestLines(inputFileContent);
        IReadOnlyDictionary<string, BatchRunStructuredResultLine<T>> responses = ParseStructuredResultDictionary<T>(outputFileContent, endpoint, structuredOutput);
        IReadOnlyDictionary<string, BatchRunErrorLine> errors = ParseErrorDictionary(errorFileContent);
        List<BatchRunItem<T>> results = [];

        foreach (BatchRunLine request in requests)
        {
            string customId = request.CustomId ?? string.Empty;
            responses.TryGetValue(customId, out BatchRunStructuredResultLine<T>? response);
            errors.TryGetValue(customId, out BatchRunErrorLine? error);

            results.Add(new BatchRunItem<T>
            {
                Request = request,
                Response = response,
                Error = error
            });
        }

        return results;
    }

    internal static IReadOnlyList<BatchRunLine> ParseRequestLines(string fileContent)
    {
        List<BatchRunLine> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = BatchRunner.ParseJsonObject(line);
            JsonObject bodyObject = lineObject["body"]?.AsObject()
                                    ?? throw new AgentFrameworkToolkitException("Batch input line was missing a request body.");
            string? endpoint = lineObject["url"]?.GetValue<string>();

            results.Add(new BatchRunLine
            {
                CustomId = lineObject["custom_id"]?.GetValue<string>(),
                Messages = [.. ParseRequestMessages(bodyObject, endpoint)]
            });
        }

        return results;
    }

    internal static IReadOnlyList<BatchRunResultLine> ParseResultLines(string fileContent, string? endpoint)
    {
        List<BatchRunResultLine> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = BatchRunner.ParseJsonObject(line);
            JsonObject responseObject = lineObject["response"]?.AsObject()
                                        ?? throw new AgentFrameworkToolkitException("Batch result line was missing a response object.");
            JsonObject bodyObject = responseObject["body"]?.AsObject()
                                    ?? throw new AgentFrameworkToolkitException("Batch result line was missing a response body.");

            results.Add(new BatchRunResultLine
            {
                CustomId = lineObject["custom_id"]?.GetValue<string>() ?? string.Empty,
                StatusCode = responseObject["status_code"]?.GetValue<int>() ?? 0,
                RequestId = responseObject["request_id"]?.GetValue<string>(),
                Messages = ParseMessages(bodyObject, endpoint),
                RawBody = bodyObject.DeepClone() as JsonObject
            });
        }

        return results;
    }

    internal static IReadOnlyList<BatchRunErrorLine> ParseErrorLines(string fileContent)
    {
        List<BatchRunErrorLine> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = BatchRunner.ParseJsonObject(line);
            JsonObject? errorObject = lineObject["error"] as JsonObject;
            JsonObject? responseObject = lineObject["response"] as JsonObject;
            JsonObject? bodyObject = responseObject?["body"] as JsonObject;

            results.Add(new BatchRunErrorLine
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

    internal static BatchRun FromJson(JsonObject batchObject, AzureOpenAIConnection connection)
    {
        return PopulateFromJson(new BatchRun(connection), batchObject);
    }

    internal static BatchRun<T> FromJson<T>(JsonObject batchObject, AzureOpenAIConnection connection, StructuredOutputSchemaDefinition structuredOutput)
    {
        return PopulateFromJson(new BatchRun<T>(connection, structuredOutput), batchObject);
    }

    internal static IReadOnlyList<BatchRunStructuredResultLine<T>> ParseStructuredResultLines<T>(
        string fileContent,
        string? endpoint,
        JsonSerializerOptions? serializerOptions = null)
    {
        StructuredOutputSchemaDefinition structuredOutput = StructuredOutputSchemaHelper.Create<T>(serializerOptions);
        return ParseStructuredResultLines<T>(fileContent, endpoint, structuredOutput);
    }

    internal static IReadOnlyList<BatchRunStructuredResultLine<T>> ParseStructuredResultLines<T>(
        string fileContent,
        string? endpoint,
        StructuredOutputSchemaDefinition structuredOutput)
    {
        List<BatchRunStructuredResultLine<T>> results = [];

        foreach (BatchRunResultLine resultLine in ParseResultLines(fileContent, endpoint))
        {
            results.Add(new BatchRunStructuredResultLine<T>
            {
                CustomId = resultLine.CustomId,
                StatusCode = resultLine.StatusCode,
                RequestId = resultLine.RequestId,
                Messages = resultLine.Messages,
                RawBody = resultLine.RawBody?.DeepClone() as JsonObject,
                Result = DeserializeStructuredResult<T>(resultLine, structuredOutput)
            });
        }

        return results;
    }

    private protected async Task<IReadOnlyList<BatchRunItem<T>>> GetStructuredResultAsync<T>()
    {
        if (_structuredOutput == null)
        {
            throw new InvalidOperationException("This batch run was not created or retrieved with structured output metadata. Use CreateBatchAsync<T>() or GetBatchAsync<T>().");
        }

        if (!IsCompletedStatus(Status) || string.IsNullOrWhiteSpace(InputFileId))
        {
            return [];
        }

        (string inputFileContent, string? outputFileContent, string? errorFileContent) = await DownloadBatchFilesAsync();
        return BuildStructuredResultItems<T>(inputFileContent, outputFileContent, errorFileContent, Endpoint, _structuredOutput);
    }

    private static T? DeserializeStructuredResult<T>(BatchRunResultLine resultLine, StructuredOutputSchemaDefinition structuredOutput)
    {
        string json = ExtractStructuredResultJson(resultLine);

        if (structuredOutput.IsWrappedInObject)
        {
            JsonObject wrappedObject = BatchRunner.ParseJsonObject(json);
            JsonNode? dataNode = wrappedObject["data"];
            return dataNode == null
                ? default
                : dataNode.Deserialize<T>(structuredOutput.SerializerOptions);
        }

        return JsonSerializer.Deserialize<T>(json, structuredOutput.SerializerOptions);
    }

    private static string ExtractStructuredResultJson(BatchRunResultLine resultLine)
    {
        if (resultLine.RawBody == null)
        {
            throw new AgentFrameworkToolkitException($"Batch result line '{resultLine.CustomId}' did not contain a response body.");
        }

        string? structuredJson = ExtractStructuredResultJson(resultLine.RawBody);
        if (string.IsNullOrWhiteSpace(structuredJson))
        {
            throw new AgentFrameworkToolkitException($"Batch result line '{resultLine.CustomId}' did not contain a structured JSON response.");
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
        Task<string?> outputTask = string.IsNullOrWhiteSpace(OutputFileId)
            ? Task.FromResult<string?>(null)
            : DownloadOptionalFileAsStringAsync(OutputFileId);
        Task<string?> errorTask = string.IsNullOrWhiteSpace(ErrorFileId)
            ? Task.FromResult<string?>(null)
            : DownloadOptionalFileAsStringAsync(ErrorFileId);

        await Task.WhenAll((Task)inputTask, outputTask, errorTask);
        return (await inputTask, await outputTask, await errorTask);
    }

    private async Task<string> DownloadFileAsStringAsync(string fileId)
    {
        Azure.AI.OpenAI.AzureOpenAIClient client = _connection.GetClient();
        OpenAIFileClient fileClient = client.GetOpenAIFileClient();
        System.ClientModel.ClientResult<BinaryData> download = await fileClient.DownloadFileAsync(fileId);
        return download.Value.ToString();
    }

    private async Task<string?> DownloadOptionalFileAsStringAsync(string? fileId)
    {
        return string.IsNullOrWhiteSpace(fileId) ? null : await DownloadFileAsStringAsync(fileId);
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

        BatchClientType clientType = BatchRunner.ParseClientType(endpoint);
        return clientType switch
        {
            BatchClientType.ChatClient => [],
            BatchClientType.ResponsesApi => [],
            _ => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
        };
    }

    private static IReadOnlyList<ChatMessage> ParseMessages(JsonObject bodyObject, string? endpoint)
    {
        if (bodyObject["output"] is JsonArray)
        {
            return ParseResponsesMessages(bodyObject);
        }

        if (bodyObject["choices"] is JsonArray)
        {
            return ParseChatCompletionMessages(bodyObject);
        }

        BatchClientType clientType = BatchRunner.ParseClientType(endpoint);
        return clientType switch
        {
            BatchClientType.ChatClient => ParseChatCompletionMessages(bodyObject),
            BatchClientType.ResponsesApi => ParseResponsesMessages(bodyObject),
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

    private static IReadOnlyList<ChatMessage> ParseChatCompletionMessages(JsonObject bodyObject)
    {
        JsonArray? choices = bodyObject["choices"] as JsonArray;
        List<ChatMessage> messages = [];

        if (choices == null)
        {
            return messages;
        }

        foreach (JsonNode? choiceNode in choices)
        {
            JsonObject? messageObject = choiceNode?["message"] as JsonObject;
            if (messageObject == null)
            {
                continue;
            }

            messages.Add(ParseChatCompletionMessage(messageObject));
        }

        return messages;
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

    private static IReadOnlyList<ChatMessage> ParseResponsesMessages(JsonObject bodyObject)
    {
        JsonArray? output = bodyObject["output"] as JsonArray;
        List<ChatMessage> messages = [];

        if (output == null)
        {
            return messages;
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
            }
        }

        return messages;
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
            JsonObject argumentsObject = BatchRunner.ParseJsonObject(argumentsText);
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

    private static IReadOnlyDictionary<string, BatchRunResultLine> ParseResultDictionary(string? outputFileContent, string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(outputFileContent))
        {
            return new Dictionary<string, BatchRunResultLine>(StringComparer.Ordinal);
        }

        return ParseResultLines(outputFileContent, endpoint)
            .ToDictionary(line => line.CustomId, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, BatchRunStructuredResultLine<T>> ParseStructuredResultDictionary<T>(
        string? outputFileContent,
        string? endpoint,
        StructuredOutputSchemaDefinition structuredOutput)
    {
        if (string.IsNullOrWhiteSpace(outputFileContent))
        {
            return new Dictionary<string, BatchRunStructuredResultLine<T>>(StringComparer.Ordinal);
        }

        return ParseStructuredResultLines<T>(outputFileContent, endpoint, structuredOutput)
            .ToDictionary(line => line.CustomId, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, BatchRunErrorLine> ParseErrorDictionary(string? errorFileContent)
    {
        if (string.IsNullOrWhiteSpace(errorFileContent))
        {
            return new Dictionary<string, BatchRunErrorLine>(StringComparer.Ordinal);
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
        where TBatchRun : BatchRun
    {
        batchRun.BatchId = batchObject["id"]?.GetValue<string>()
                           ?? throw new AgentFrameworkToolkitException("Batch response did not contain an id.");
        batchRun.Status = batchObject["status"]?.GetValue<string>()
                          ?? throw new AgentFrameworkToolkitException("Batch response did not contain a status.");
        batchRun.Counts = new BatchResponseCounts
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
public class BatchRun<T> : BatchRun
{
    internal BatchRun(AzureOpenAIConnection connection, StructuredOutputSchemaDefinition structuredOutput)
        : base(connection, structuredOutput)
    {
    }

    /// <summary>
    /// Gets the completed structured batch result joined by custom id.
    /// </summary>
    /// <returns>
    /// A collection containing the original request together with the matched structured response and error for each line.
    /// Returns an empty collection when the batch is not yet completed.
    /// </returns>
    public new Task<IReadOnlyList<BatchRunItem<T>>> GetResult()
    {
        return GetResultAsync();
    }

    /// <summary>
    /// Gets the completed structured batch result joined by custom id.
    /// </summary>
    /// <returns>
    /// A collection containing the original request together with the matched structured response and error for each line.
    /// Returns an empty collection when the batch is not yet completed.
    /// </returns>
    public new Task<IReadOnlyList<BatchRunItem<T>>> GetResultAsync()
    {
        return GetStructuredResultAsync<T>();
    }

    /// <summary>
    /// Downloads the successful output lines and converts them into structured results.
    /// </summary>
    /// <returns>The parsed structured output lines.</returns>
    protected Task<IReadOnlyList<BatchRunStructuredResultLine<T>>> DownloadStructuredResultsAsync()
    {
        return base.DownloadStructuredResultsAsync<T>();
    }
}

/// <summary>
/// Request counts for a batch run.
/// </summary>
public class BatchResponseCounts
{
    /// <summary>
    /// Gets the total number of requests in the batch.
    /// </summary>
    [JsonPropertyName("total")]
    public required int Total { get; init; }

    /// <summary>
    /// Gets the number of completed requests in the batch.
    /// </summary>
    [JsonPropertyName("completed")]
    public required int Completed { get; init; }

    /// <summary>
    /// Gets the number of failed requests in the batch.
    /// </summary>
    [JsonPropertyName("failed")]
    public required int Failed { get; init; }
}

/// <summary>
/// A joined batch result item containing the original request and any matched response or error.
/// </summary>
public class BatchRunItem
{
    /// <summary>
    /// Gets or sets the original request line.
    /// </summary>
    public required BatchRunLine Request { get; init; }

    /// <summary>
    /// Gets or sets the matched successful response for the request when present.
    /// </summary>
    public BatchRunResultLine? Response { get; init; }

    /// <summary>
    /// Gets or sets the matched error for the request when present.
    /// </summary>
    public BatchRunErrorLine? Error { get; init; }
}

/// <summary>
/// A joined structured batch result item containing the original request and any matched response or error.
/// </summary>
/// <typeparam name="T">The structured output type returned for the line.</typeparam>
public class BatchRunItem<T>
{
    /// <summary>
    /// Gets or sets the original request line.
    /// </summary>
    public required BatchRunLine Request { get; init; }

    /// <summary>
    /// Gets or sets the matched structured response for the request when present.
    /// </summary>
    public BatchRunStructuredResultLine<T>? Response { get; init; }

    /// <summary>
    /// Gets or sets the matched error for the request when present.
    /// </summary>
    public BatchRunErrorLine? Error { get; init; }
}

/// <summary>
/// A successful parsed line from the batch output file.
/// </summary>
public class BatchRunResultLine
{
    /// <summary>
    /// Gets or sets the custom id for the line.
    /// </summary>
    public required string CustomId { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code for the line.
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// Gets or sets the request id returned by the service.
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Gets or sets the parsed chat messages.
    /// </summary>
    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    /// <summary>
    /// Gets or sets the raw JSON response body.
    /// </summary>
    public JsonObject? RawBody { get; init; }
}

/// <summary>
/// A successful parsed line from the batch output file with structured output.
/// </summary>
/// <typeparam name="T">The structured output type returned for the line.</typeparam>
public class BatchRunStructuredResultLine<T> : BatchRunResultLine
{
    /// <summary>
    /// Gets or sets the structured result for the line.
    /// </summary>
    public T? Result { get; init; }
}

/// <summary>
/// A failed parsed line from the batch error file.
/// </summary>
public class BatchRunErrorLine
{
    /// <summary>
    /// Gets or sets the custom id for the line.
    /// </summary>
    public required string CustomId { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code for the line, if present.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Gets or sets the request id returned by the service, if present.
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets the raw error object.
    /// </summary>
    public JsonObject? RawError { get; init; }

    /// <summary>
    /// Gets or sets the raw response body when present.
    /// </summary>
    public JsonObject? RawBody { get; init; }
}
