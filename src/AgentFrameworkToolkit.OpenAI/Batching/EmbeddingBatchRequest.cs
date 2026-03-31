using JetBrains.Annotations;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Represents one embedding request line in the batch input file.
/// </summary>
[PublicAPI]
public class EmbeddingBatchRequest
{
    /// <summary>
    /// Gets or sets the optional custom id for the line. If omitted, one is generated.
    /// </summary>
    public string CustomId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the values sent as the embedding input payload.
    /// </summary>
    public required IList<string> Values { get; set; }

    /// <summary>
    /// Creates a single-value <see cref="EmbeddingBatchRequest"/>.
    /// </summary>
    /// <param name="value">The value to embed.</param>
    /// <returns>The created request.</returns>
    public static EmbeddingBatchRequest Create(string value)
    {
        return new EmbeddingBatchRequest
        {
            Values = [value]
        };
    }

    /// <summary>
    /// Creates an <see cref="EmbeddingBatchRequest"/>.
    /// </summary>
    /// <param name="values">The values to embed.</param>
    /// <returns>The created request.</returns>
    public static EmbeddingBatchRequest Create(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return new EmbeddingBatchRequest
        {
            Values = [.. values]
        };
    }
}
