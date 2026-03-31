using OpenAI;
using System.Text.Json;
using JetBrains.Annotations;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Batch Runner for Azure OpenAI
/// </summary>
[PublicAPI]
public class OpenAIBatchRunner
{
    private readonly InternalBatchRunner _internalBatchRunner;

    /// <summary>
    /// Connection
    /// </summary>
    public OpenAIConnection Connection { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your OpenAI API Key (if you need a more advanced connection use the constructor overload)</param>
    public OpenAIBatchRunner(string apiKey)
    {
        Connection = new OpenAIConnection
        {
            ApiKey = apiKey
        };
        OpenAIClient client = Connection.GetClient();
        _internalBatchRunner = new InternalBatchRunner(client.GetBatchClient(), client.GetOpenAIFileClient(), true);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public OpenAIBatchRunner(OpenAIConnection connection)
    {
        Connection = connection;
        OpenAIClient client = Connection.GetClient();
        _internalBatchRunner = new InternalBatchRunner(client.GetBatchClient(), client.GetOpenAIFileClient(), false);
    }

    /// <summary>
    /// Creates a new batch run.
    /// </summary>
    /// <param name="options">Options applied to every entry in the batch.</param>
    /// <param name="requests">The batch entries to submit.</param>
    /// <returns>The created batch run.</returns>
    public Task<ChatBatchRun> RunChatBatchAsync(ChatBatchOptions options, IList<ChatBatchRequest> requests)
    {
        return _internalBatchRunner.RunChatBatchAsync(options, requests);
    }

    /// <summary>
    /// Creates a new batch run with structured output for every line.
    /// </summary>
    /// <typeparam name="T">The structured output type returned for each line.</typeparam>
    /// <param name="options">Options applied to every entry in the batch.</param>
    /// <param name="requests">The batch entries to submit.</param>
    /// <param name="serializerOptions">Optional serializer options used for schema generation and result deserialization.</param>
    /// <returns>The created structured batch run.</returns>
    public Task<ChatBatchRun<T>> RunChatBatchAsync<T>(ChatBatchOptions options, IList<ChatBatchRequest> requests, JsonSerializerOptions? serializerOptions = null)
    {
        return _internalBatchRunner.RunChatBatchAsync<T>(options, requests, serializerOptions);
    }

    /// <summary>
    /// Gets an existing batch run.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <returns>The batch run.</returns>
    public Task<ChatBatchRun> GetChatBatchAsync(string batchId)
    {
        return _internalBatchRunner.GetChatBatchAsync(batchId);
    }

    /// <summary>
    /// Gets an existing structured batch run.
    /// </summary>
    /// <typeparam name="T">The structured output type returned for each line.</typeparam>
    /// <param name="batchId">The batch identifier.</param>
    /// <param name="serializerOptions">Optional serializer options used for schema generation and result deserialization.</param>
    /// <returns>The structured batch run.</returns>
    public Task<ChatBatchRun<T>> GetChatBatchAsync<T>(string batchId, JsonSerializerOptions? serializerOptions = null)
    {
        return _internalBatchRunner.GetChatBatchAsync<T>(batchId, serializerOptions);
    }

    /// <summary>
    /// Creates a new embedding batch run.
    /// </summary>
    /// <param name="options">Options applied to every entry in the batch.</param>
    /// <param name="requests">The batch entries to submit.</param>
    /// <returns>The created embedding batch run.</returns>
    public Task<EmbeddingBatchRun> RunEmbeddingBatchAsync(EmbeddingBatchOptions options, IList<EmbeddingBatchRequest> requests)
    {
        return _internalBatchRunner.RunEmbeddingBatchAsync(options, requests);
    }

    /// <summary>
    /// Gets an existing embedding batch run.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <returns>The embedding batch run.</returns>
    public Task<EmbeddingBatchRun> GetEmbeddingBatchAsync(string batchId)
    {
        return _internalBatchRunner.GetEmbeddingBatchAsync(batchId);
    }

}