using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using OpenAI.Files;

namespace AgentFrameworkToolkit.AzureOpenAI;

/// <summary>
/// Represents a batch run and exposes helpers for downloading results.
/// </summary>
public class BatchRun
{
    private readonly AzureOpenAIConnection _connection;

    internal BatchRun(AzureOpenAIConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Gets the batch identifier.
    /// </summary>
    public required string BatchId { get; init; }

    /// <summary>
    /// Gets the batch status.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>
    /// Gets the request counts.
    /// </summary>
    [JsonPropertyName("request_counts")]
    public required BatchResponseCounts Counts { get; init; }

    /// <summary>
    /// Gets the input file id.
    /// </summary>
    [JsonPropertyName("input_file_id")]
    public string? InputFileId { get; init; }

    /// <summary>
    /// Gets the batch endpoint.
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets the output file id.
    /// </summary>
    [JsonPropertyName("output_file_id")]
    public required string? OutputFileId { get; init; }

    /// <summary>
    /// Gets the error file id.
    /// </summary>
    [JsonPropertyName("error_file_id")]
    public required string? ErrorFileId { get; init; }

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
                ErrorCode = errorObject?["code"]?.GetValue<string>(),
                ErrorMessage = errorObject?["message"]?.GetValue<string>(),
                RawError = errorObject?.DeepClone() as JsonObject,
                RawBody = bodyObject?.DeepClone() as JsonObject
            });
        }

        return results;
    }

    internal static BatchRun FromJson(JsonObject batchObject, AzureOpenAIConnection connection)
    {
        return new BatchRun(connection)
        {
            BatchId = batchObject["id"]?.GetValue<string>()
                      ?? throw new AgentFrameworkToolkitException("Batch response did not contain an id."),
            Status = batchObject["status"]?.GetValue<string>()
                     ?? throw new AgentFrameworkToolkitException("Batch response did not contain a status."),
            Counts = new BatchResponseCounts
            {
                Total = batchObject["request_counts"]?["total"]?.GetValue<int>() ?? 0,
                Completed = batchObject["request_counts"]?["completed"]?.GetValue<int>() ?? 0,
                Failed = batchObject["request_counts"]?["failed"]?.GetValue<int>() ?? 0
            },
            InputFileId = batchObject["input_file_id"]?.GetValue<string>(),
            Endpoint = batchObject["endpoint"]?.GetValue<string>(),
            OutputFileId = batchObject["output_file_id"]?.GetValue<string>(),
            ErrorFileId = batchObject["error_file_id"]?.GetValue<string>()
        };
    }

    private async Task<string> DownloadFileAsStringAsync(string fileId)
    {
        Azure.AI.OpenAI.AzureOpenAIClient client = _connection.GetClient();
        OpenAIFileClient fileClient = client.GetOpenAIFileClient();
        System.ClientModel.ClientResult<BinaryData> download = await fileClient.DownloadFileAsync(fileId);
        return download.Value.ToString();
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
