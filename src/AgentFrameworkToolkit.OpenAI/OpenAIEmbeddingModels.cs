using JetBrains.Annotations;

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// A List of the most common OpenAI Models
/// </summary>
[PublicAPI]
public static class OpenAIEmbeddingModels
{
    /// <summary>
    /// text-embedding-3-small (1536 Dimensions)
    /// </summary>
    public const string TextEmbedding3Small = "text-embedding-3-small";

    /// <summary>
    /// text-embedding-3-large (3072 Dimensions)
    /// </summary>
    public const string TextEmbedding3Large = "text-embedding-3-large";

    /// <summary>
    /// text-embedding-ada-002 (1536 Dimensions)
    /// </summary>
    public const string TextEmbeddingAda002 = "text-embedding-ada-002";
}
