using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// Factory for creating OpenAI EmbeddingGenerator
/// </summary>
[PublicAPI]
public class GoogleEmbeddingFactory
{
    /// <summary>
    /// Connection
    /// </summary>
    public GoogleConnection Connection { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your OpenAI API Key (if you need a more advanced connection use the constructor overload)</param>
    public GoogleEmbeddingFactory(string apiKey)
    {
        Connection = new GoogleConnection
        {
            ApiKey = apiKey,
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public GoogleEmbeddingFactory(GoogleConnection connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Create an EmbeddingGenerator
    /// </summary>
    /// <param name="model">The Embedding Model to use</param>
    /// <param name="dimensions">The number of dimensions (768, 1536, or 3072) Default is 3072 for gemini-embedding-001</param>
    /// <returns>An Embedding Generator</returns>
    public IEmbeddingGenerator<string, Embedding<float>> GetEmbeddingGenerator(string model, int? dimensions = null)
    {
        return Connection.GetClient().AsIEmbeddingGenerator(model, dimensions);
    }

    /// <summary>
    /// Generate an embedding
    /// </summary>
    /// <param name="value">String to embed</param>
    /// <param name="model">Embedding model to use</param>
    /// <param name="dimensions">The number of dimensions (768, 1536, or 3072) Default is 3072 for gemini-embedding-001</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The Embedding</returns>
    public async Task<Embedding<float>> GenerateAsync(string value, string model, int? dimensions = null, CancellationToken cancellationToken = default)
    {
        return await GenerateAsync(value, model, null, dimensions, cancellationToken);
    }

    /// <summary>
    /// Generate an embedding
    /// </summary>
    /// <param name="values">Strings to embed</param>
    /// <param name="model">Embedding model to use</param>
    /// <param name="dimensions">The number of dimensions (768, 1536, or 3072) Default is 3072 for gemini-embedding-001</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The Embeddings</returns>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, string model, int? dimensions = null, CancellationToken cancellationToken = default)
    {
        return await GenerateAsync(values, model, null, dimensions, cancellationToken);
    }

    /// <summary>
    /// Generate an embedding
    /// </summary>
    /// <param name="value">String to embed</param>
    /// <param name="model">Model to use for embedding</param>
    /// <param name="options">Options for the Embedding</param>
    /// <param name="dimensions">The number of dimensions (768, 1536, or 3072) Default is 3072 for gemini-embedding-001</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The Embedding</returns>
    public async Task<Embedding<float>> GenerateAsync(string value, string model, EmbeddingGenerationOptions? options, int? dimensions = null, CancellationToken cancellationToken = default)
    {
        IEmbeddingGenerator<string, Embedding<float>> generator = GetEmbeddingGenerator(model);
        return await generator.GenerateAsync(value, options, cancellationToken);
    }


    /// <summary>
    /// Generate an embedding
    /// </summary>
    /// <param name="values">Strings to embed</param>
    /// <param name="model">Model to use for embedding</param>
    /// <param name="options">Options for the Embedding</param>
    /// <param name="dimensions">The number of dimensions (768, 1536, or 3072) Default is 3072 for gemini-embedding-001</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The Embeddings</returns>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, string model, EmbeddingGenerationOptions? options, int? dimensions = null, CancellationToken cancellationToken = default)
    {
        IEmbeddingGenerator<string, Embedding<float>> generator = GetEmbeddingGenerator(model, dimensions);
        return await generator.GenerateAsync(values, options, cancellationToken);
    }
}
