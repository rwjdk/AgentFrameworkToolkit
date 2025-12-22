using Azure;
using Azure.AI.Inference;
using Azure.Core.Pipeline;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.GitHub;

/// <summary>
/// Represents a connection for GitHub Models
/// </summary>
[PublicAPI]
public class GitHubConnection
{
    /// <summary>
    /// The GitHub Personal Access Token (fine-grained with Models Access) 
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Action to set additional options on the underlying AzureAIInferenceClientOptions if needed
    /// </summary>
    public Action<AzureAIInferenceClientOptions>? AdditionalAzureAIInferenceClientOptions { get; set; }

    /// <summary>
    /// Get a Raw Client
    /// </summary>
    /// <param name="rawHttpCallDetails">An Action, if set, will attach an HTTP Message Handler so you can see the raw HTTP Calls that are sent to the LLM</param>
    /// <returns>The Raw Client</returns>
    public ChatCompletionsClient GetClient(Action<RawCallDetails>? rawHttpCallDetails = null)
    {
        AzureAIInferenceClientOptions clientOptions = new();

        if (rawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(rawHttpCallDetails));
            if (NetworkTimeout != null)
            {
                inspectingHttpClient.Timeout = NetworkTimeout.Value;
            }

            clientOptions.Transport = new HttpClientTransport(inspectingHttpClient);
        }
        else if (NetworkTimeout != null)
        {
            clientOptions.Transport = new HttpClientTransport(new HttpClient
            {
                Timeout = NetworkTimeout.Value
            });
        }

        AdditionalAzureAIInferenceClientOptions?.Invoke(clientOptions);

        ChatCompletionsClient client = new(
            new Uri("https://models.github.ai/inference"),
            new AzureKeyCredential(AccessToken),
            clientOptions);

        return client;
    }
}
