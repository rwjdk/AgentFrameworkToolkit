using Azure.AI.OpenAI;
using Azure.Core;
using System.ClientModel;
using System.ClientModel.Primitives;

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
    /// The API Key (or use Credentials instead for RBAC)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Credentials for Role-Based Access Control (Example 'DefaultAzureCredential' or 'AzureCliCredential') [or use ApiKey instead]
    /// </summary>
    public TokenCredential? Credentials { get; set; }

    /// <summary>
    /// An Action that allow you to set additional options on the AzureOpenAIClientOptions
    /// </summary>
    public Action<AzureOpenAIClientOptions>? AdditionalAzureOpenAIClientOptions { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Get a Raw Client
    /// </summary>
    /// <param name="rawHttpCallDetails">An Action, if set, will attach an HTTP Message Handler so you can see the raw HTTP Calls that are sent to the LLM</param>
    /// <returns>The Raw Client</returns>
    public AzureOpenAIClient GetClient(Action<RawCallDetails>? rawHttpCallDetails = null)
    {
        AzureOpenAIClientOptions azureOpenAIClientOptions = new()
        {
            NetworkTimeout = NetworkTimeout
        };

        // ReSharper disable once InvertIf
        if (rawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(rawHttpCallDetails));
            azureOpenAIClientOptions.Transport = new HttpClientPipelineTransport(inspectingHttpClient);
        }

        AdditionalAzureOpenAIClientOptions?.Invoke(azureOpenAIClientOptions);

        Uri endpoint = new(Endpoint);
        if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            return new AzureOpenAIClient(endpoint, new ApiKeyCredential(ApiKey!), azureOpenAIClientOptions);
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (Credentials != null)
        {
            return new AzureOpenAIClient(endpoint, Credentials, azureOpenAIClientOptions);
        }

        throw new AgentFrameworkToolkitException("Neither APIKey nor TokenCredentials was provided in the AzureConnection");
    }
}