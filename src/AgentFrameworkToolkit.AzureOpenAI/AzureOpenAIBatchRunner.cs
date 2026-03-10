using Azure.Core;
using Microsoft.Extensions.AI;
#pragma warning disable CS1591 //todo add  Missing XML comment for publicly visible type or member

namespace AgentFrameworkToolkit.AzureOpenAI;

/// <summary>
/// Azure OpenAI Runner for BatchJobs
/// </summary>
public class AzureOpenAIBatchRunner
{
    /// <summary>
    /// Connection
    /// </summary>
    public AzureOpenAIConnection Connection { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endpoint">Your AzureOpenAI Endpoint (not to be confused with a Microsoft Foundry Endpoint. format: 'https://YourName.openai.azure.com' or 'https://YourName.services.azure.com')</param>
    /// <param name="apiKey">Your AzureOpenAI API Key (if you need a more advanced connection use the constructor overload)</param>
    public AzureOpenAIBatchRunner(string endpoint, string apiKey)
    {
        Connection = new AzureOpenAIConnection
        {
            Endpoint = endpoint,
            ApiKey = apiKey
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endpoint">Your AzureOpenAI Endpoint (not to be confused with a Microsoft Foundry Endpoint. format: 'https://YourName.openai.azure.com' or 'https://YourName.services.azure.com')</param>
    /// <param name="credentials">Your RBAC Credentials (if you need a more advanced connection use the constructor overload)</param>
    public AzureOpenAIBatchRunner(string endpoint, TokenCredential credentials)
    {
        Connection = new AzureOpenAIConnection
        {
            Endpoint = endpoint,
            Credentials = credentials
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public AzureOpenAIBatchRunner(AzureOpenAIConnection connection)
    {
        Connection = connection;
    }


    public async Task<BatchRun> CreateBatchAsync(BatchOptions options, IList<ChatMessage> messages)
    {
        throw new NotImplementedException();
    }

    public async Task<BatchRun> GetBatchAsync(string batchId)
    {
        throw new NotImplementedException();
    }
}

public class BatchOptions
{
    public BatchClientType ClientType { get; set; } = BatchClientType.ChatClient;
    public bool WaitUntilCompleted { get; set; }
}

public enum BatchClientType
{
    ChatClient,
    ResponsesApi
}