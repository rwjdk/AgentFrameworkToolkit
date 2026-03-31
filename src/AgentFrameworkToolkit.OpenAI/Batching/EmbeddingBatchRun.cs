using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using OpenAI.Files;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Represents an embedding batch run and exposes helpers for retrieving matched requests, responses, and errors.
/// </summary>
[PublicAPI]
public class EmbeddingBatchRun
{
    private readonly OpenAIFileClient? _fileClient;

    internal EmbeddingBatchRun(OpenAIFileClient? fileClient)
    {
        _fileClient = fileClient;
    }

    /// <summary>
    /// Gets the batch identifier.
    /// </summary>
    public string Id { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the status of the batch run.
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
    /// Gets the batch status.
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
    /// Gets the completed embedding batch result joined by custom id.
    /// </summary>
    /// <param name="cleanUpRemoteFilesOnSuccessfulRetrieval">If the files involved in the batch should be removed on successful retrieval</param> 
    /// <returns>
    /// A collection containing the original request together with the matched embedding response and error for each line.
    /// Returns an empty collection when the batch is not yet completed.
    /// </returns>
    public async Task<IList<EmbeddingBatchRunResult>> GetResultAsync(bool cleanUpRemoteFilesOnSuccessfulRetrieval = false)
    {
        if (!IsCompletedStatus(StatusString) || string.IsNullOrWhiteSpace(InputFileId))
        {
            return [];
        }

        (string inputFileContent, string? outputFileContent, string? errorFileContent) = await DownloadBatchFilesAsync();
        IList<EmbeddingBatchRunResult> results = BuildResultItems(inputFileContent, outputFileContent, errorFileContent);
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

    internal static IList<EmbeddingBatchRunResult> BuildResultItems(
        string inputFileContent,
        string? outputFileContent,
        string? errorFileContent)
    {
        IReadOnlyList<EmbeddingBatchRequest> requests = ParseRequestLines(inputFileContent);
        IReadOnlyDictionary<string, EmbeddingBatchRunResponse> responses = ParseResultDictionary(outputFileContent);
        IReadOnlyDictionary<string, ChatBatchRunError> errors = ParseErrorDictionary(errorFileContent);
        List<EmbeddingBatchRunResult> results = [];

        foreach (EmbeddingBatchRequest request in requests)
        {
            string customId = request.CustomId;
            responses.TryGetValue(customId, out EmbeddingBatchRunResponse? response);
            errors.TryGetValue(customId, out ChatBatchRunError? error);

            results.Add(new EmbeddingBatchRunResult
            {
                CustomId = customId,
                Request = request.Value,
                Response = response?.Result,
                Error = error
            });
        }

        return results;
    }

    internal static IReadOnlyList<EmbeddingBatchRequest> ParseRequestLines(string fileContent)
    {
        List<EmbeddingBatchRequest> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = InternalBatchRunner.ParseJsonObject(line);
            JsonObject bodyObject = lineObject["body"]?.AsObject()
                                   ?? throw new AgentFrameworkToolkitException("Batch input line was missing a request body.");

            results.Add(new EmbeddingBatchRequest
            {
                CustomId = lineObject["custom_id"]?.GetValue<string>() ?? Guid.NewGuid().ToString(),
                Value = bodyObject["input"]!.GetValue<string>()
            });
        }

        return results;
    }

    internal static IReadOnlyList<EmbeddingBatchRunResponse> ParseResultLines(string fileContent)
    {
        List<EmbeddingBatchRunResponse> results = [];

        foreach (string line in EnumerateJsonLines(fileContent))
        {
            JsonObject lineObject = InternalBatchRunner.ParseJsonObject(line);
            JsonObject responseObject = lineObject["response"]?.AsObject()
                                        ?? throw new AgentFrameworkToolkitException("Batch result line was missing a response object.");
            JsonObject bodyObject = responseObject["body"]?.AsObject()
                                    ?? throw new AgentFrameworkToolkitException("Batch result line was missing a response body.");

            results.Add(new EmbeddingBatchRunResponse
            {
                CustomId = lineObject["custom_id"]?.GetValue<string>() ?? string.Empty,
                StatusCode = responseObject["status_code"]?.GetValue<int>() ?? 0,
                RequestId = responseObject["request_id"]?.GetValue<string>(),
                Result = ParseGeneratedEmbedding(bodyObject),
                RawBody = bodyObject.DeepClone() as JsonObject
            });
        }

        return results;
    }

    internal static EmbeddingBatchRun FromJson(JsonObject batchObject, OpenAIFileClient fileClient)
    {
        EmbeddingBatchRun batchRun = new(fileClient)
        {
            Id = batchObject["id"]?.GetValue<string>()
                 ?? throw new AgentFrameworkToolkitException("Batch response did not contain an id."),
            StatusString = batchObject["status"]?.GetValue<string>()
                           ?? throw new AgentFrameworkToolkitException("Batch response did not contain a status."),
            Counts = new BatchCounts
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

        return batchRun;
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

    private static IReadOnlyList<string> ParseInputValues(JsonNode? inputNode)
    {
        if (inputNode is JsonValue inputValue && inputValue.TryGetValue(out string? value))
        {
            return string.IsNullOrEmpty(value) ? [] : [value];
        }

        if (inputNode is not JsonArray inputArray)
        {
            return [];
        }

        List<string> values = [];
        foreach (JsonNode? itemNode in inputArray)
        {
            if (itemNode is JsonValue itemValue && itemValue.TryGetValue(out string? itemText))
            {
                values.Add(itemText);
            }
        }

        return values;
    }

    private static Embedding<float> ParseGeneratedEmbedding(JsonObject bodyObject)
    {
        JsonArray dataArray = bodyObject["data"] as JsonArray
                              ?? throw new AgentFrameworkToolkitException("Batch result line did not contain embedding data.");

        List<(int Index, Embedding<float> Embedding)> indexedEmbeddings = [];
        int fallbackIndex = 0;

        foreach (JsonNode? itemNode in dataArray)
        {
            if (itemNode is not JsonObject itemObject)
            {
                continue;
            }

            JsonArray embeddingArray = itemObject["embedding"] as JsonArray
                                       ?? throw new AgentFrameworkToolkitException("Embedding result line did not contain an embedding vector.");

            List<float> vector = [];
            foreach (JsonNode? valueNode in embeddingArray)
            {
                if (valueNode == null)
                {
                    continue;
                }

                vector.Add(valueNode.GetValue<float>());
            }

            indexedEmbeddings.Add((itemObject["index"]?.GetValue<int>() ?? fallbackIndex, new Embedding<float>(vector.ToArray())));
            fallbackIndex++;
        }

        GeneratedEmbeddings<Embedding<float>> embeddings = new(
            indexedEmbeddings
                .OrderBy(item => item.Index)
                .Select(item => item.Embedding));

        if (bodyObject["usage"] is JsonObject usageObject)
        {
            embeddings.Usage = ParseUsage(usageObject);
        }

        AdditionalPropertiesDictionary additionalProperties = BuildAdditionalProperties(bodyObject);
        if (additionalProperties.Count > 0)
        {
            embeddings.AdditionalProperties = additionalProperties;
        }

        return embeddings.First();
    }

    private static UsageDetails ParseUsage(JsonObject usageObject)
    {
        UsageDetails usage = new()
        {
            InputTokenCount = GetInt32Value(usageObject, "input_tokens") ?? GetInt32Value(usageObject, "prompt_tokens"),
            OutputTokenCount = GetInt32Value(usageObject, "output_tokens"),
            TotalTokenCount = GetInt32Value(usageObject, "total_tokens")
        };

        Dictionary<string, long> additionalCounts = [];
        foreach ((string key, JsonNode? valueNode) in usageObject)
        {
            if (valueNode == null || !TryGetInt64Value(valueNode, out long value))
            {
                continue;
            }

            if (key is "input_tokens" or "prompt_tokens" or "output_tokens" or "total_tokens")
            {
                continue;
            }

            additionalCounts[key] = value;
        }

        if (additionalCounts.Count > 0)
        {
            usage.AdditionalCounts = new AdditionalPropertiesDictionary<long>(additionalCounts);
        }

        return usage;
    }

    private static AdditionalPropertiesDictionary BuildAdditionalProperties(JsonObject bodyObject)
    {
        Dictionary<string, object?> values = [];

        foreach ((string key, JsonNode? valueNode) in bodyObject)
        {
            if (key is "data" or "usage")
            {
                continue;
            }

            values[key] = ConvertJsonNodeToObject(valueNode);
        }

        return new AdditionalPropertiesDictionary(values);
    }

    private static object? ConvertJsonNodeToObject(JsonNode? valueNode)
    {
        if (valueNode == null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<object?>(valueNode.ToJsonString());
    }

    private static int? GetInt32Value(JsonObject jsonObject, string propertyName)
    {
        return jsonObject[propertyName] == null ? null : jsonObject[propertyName]!.GetValue<int>();
    }

    private static bool TryGetInt64Value(JsonNode valueNode, out long value)
    {
        try
        {
            value = valueNode.GetValue<long>();
            return true;
        }
        catch
        {
            value = 0;
            return false;
        }
    }

    private static IEnumerable<string> EnumerateJsonLines(string fileContent)
    {
        return fileContent
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static line => !string.IsNullOrWhiteSpace(line));
    }

    private static IReadOnlyDictionary<string, EmbeddingBatchRunResponse> ParseResultDictionary(string? outputFileContent)
    {
        if (string.IsNullOrWhiteSpace(outputFileContent))
        {
            return new Dictionary<string, EmbeddingBatchRunResponse>(StringComparer.Ordinal);
        }

        return ParseResultLines(outputFileContent)
            .ToDictionary(line => line.CustomId, StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, ChatBatchRunError> ParseErrorDictionary(string? errorFileContent)
    {
        if (string.IsNullOrWhiteSpace(errorFileContent))
        {
            return new Dictionary<string, ChatBatchRunError>(StringComparer.Ordinal);
        }

        return ChatBatchRun.ParseErrorLines(errorFileContent)
            .ToDictionary(line => line.CustomId, StringComparer.Ordinal);
    }

    private static bool IsCompletedStatus(string? status)
    {
        return string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase);
    }
}

[PublicAPI]
internal class EmbeddingBatchRunResponse
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
    /// Gets or sets the parsed embeddings response.
    /// </summary>
    public required Embedding<float> Result { get; init; }

    /// <summary>
    /// Gets or sets the raw JSON response body.
    /// </summary>
    public JsonObject? RawBody { get; init; }
}
