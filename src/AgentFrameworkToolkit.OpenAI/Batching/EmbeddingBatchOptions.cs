using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Options for an embedding batch run.
/// </summary>
public class EmbeddingBatchOptions
{
    /// <summary>
    /// Gets or sets the model or deployment name used for every line in the batch.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the run should wait until completed.
    /// </summary>
    public bool WaitUntilCompleted { get; set; }

    /// <summary>
    /// Gets or sets options applied to every embedding request in the batch.
    /// </summary>
    public EmbeddingGenerationOptions? GenerationOptions { get; set; }
}
