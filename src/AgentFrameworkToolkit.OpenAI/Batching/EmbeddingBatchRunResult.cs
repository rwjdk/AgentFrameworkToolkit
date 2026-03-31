using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// A joined embedding batch result item containing the original request and any matched response or error.
/// </summary>
[PublicAPI]
public class EmbeddingBatchRunResult
{
    /// <summary>
    /// Gets or sets the optional custom id for the line. If omitted, one is generated.
    /// </summary>
    public required string CustomId { get; set; }

    /// <summary>
    /// Gets or sets the values sent as the request payload.
    /// </summary>
    public required string Request { get; set; }

    /// <summary>
    /// Gets or sets the embeddings returned for the request.
    /// </summary>
    public Embedding<float>? Response { get; init; }

    /// <summary>
    /// Gets or sets the matched error for the request when present.
    /// </summary>
    public ChatBatchRunError? Error { get; init; }
}
