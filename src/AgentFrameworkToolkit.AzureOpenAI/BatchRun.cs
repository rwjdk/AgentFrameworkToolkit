using System.Text.Json.Serialization;
#pragma warning disable CS1591 //todo - add Missing XML comment for publicly visible type or member

namespace AgentFrameworkToolkit.AzureOpenAI;

public class BatchRun
{
    public required string BatchId { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; init; } //Options: validating, failed, in_progress, finalizing, completed, expired, cancelling, cancelled

    [JsonPropertyName("request_counts")]
    public required BatchResponseCounts Counts { get; init; }

    [JsonPropertyName("output_file_id")]
    public required string? OutputFileId { get; init; }

    [JsonPropertyName("error_file_id")]
    public required string? ErrorFileId { get; init; }

}

public class BatchResponseCounts
{
    [JsonPropertyName("total")]
    public required int Total { get; init; }

    [JsonPropertyName("completed")]
    public required int Completed { get; init; }

    [JsonPropertyName("failed")]
    public required int Failed { get; init; }
}