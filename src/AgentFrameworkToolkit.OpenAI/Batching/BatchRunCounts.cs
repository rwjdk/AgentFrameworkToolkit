using System.Text.Json.Serialization;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Request counts for a batch run.
/// </summary>
public class BatchCounts
{
    /// <summary>
    /// Gets the total number of requests in the batch.
    /// </summary>
    [JsonPropertyName("total")]
    public required int Total { get; init; }

    /// <summary>
    /// Gets the number of completed requests in the batch. [NB: This is only populated by OpenAI, not Azure OpenAI]
    /// </summary>
    [JsonPropertyName("completed")]
    public required int Completed { get; init; }

    /// <summary>
    /// Gets the number of failed requests in the batch.
    /// </summary>
    [JsonPropertyName("failed")]
    public required int Failed { get; init; }
}