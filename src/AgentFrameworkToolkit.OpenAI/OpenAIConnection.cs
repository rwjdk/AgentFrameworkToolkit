using JetBrains.Annotations;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Data.Common;

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Represents a connection for OpenAI
/// </summary>
[PublicAPI]
public class OpenAIConnection
{
    /// <summary>
    /// The API Key to be used
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// The Endpoint to be used (only need to be set if you use OpenAI-spec against a 3rd party provider)
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// An Action that allow you to set additional options on the OpenAIClientOptions
    /// </summary>
    public Action<OpenAIClientOptions>? AdditionalOpenAIClientOptions { get; set; }
    
    /// <summary>
    /// Get a Raw Client
    /// </summary>
    /// <param name="rawHttpCallDetails">An Action, if set, will attach an HTTP Message Handler so you can see the raw HTTP Calls that are sent to the LLM</param>
    /// <returns>The Raw Client</returns>
    public OpenAIClient GetClient(Action<RawCallDetails>? rawHttpCallDetails = null)
    {
        OpenAIClientOptions openAIClientOptions = new()
        {
            NetworkTimeout = NetworkTimeout
        };

        if (!string.IsNullOrWhiteSpace(Endpoint))
        {
            openAIClientOptions.Endpoint = new Uri(Endpoint);
        }

        // ReSharper disable once InvertIf
        if (rawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(rawHttpCallDetails));
            openAIClientOptions.Transport = new HttpClientPipelineTransport(inspectingHttpClient);
        }

        AdditionalOpenAIClientOptions?.Invoke(openAIClientOptions);

        return new OpenAIClient(new ApiKeyCredential(ApiKey), openAIClientOptions);
    }
}