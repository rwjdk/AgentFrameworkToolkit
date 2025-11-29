using Azure.AI.OpenAI;

namespace AgentFrameworkToolkit.AzureOpenAI;

/// <summary>
/// Represents a connection for Azure OpenAI
/// </summary>
public class AzureOpenAIConnection
{
    /// <summary>
    /// The Endpoint of your Azure OpenAI Resource
    /// </summary>
    public required string Endpoint { get; set; }

    /// <summary>
    /// The API Key
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// An Action that allow you to set additional options on the AzureOpenAIClientOptions
    /// </summary>
    public Action<AzureOpenAIClientOptions>? AdditionalAzureOpenAIClientOptions { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    //todo - Support RBAC
}