using JetBrains.Annotations;
using Mistral.SDK;

namespace AgentFrameworkToolkit.Mistral;

/// <summary>
/// Represents a connection for Mistral
/// </summary>
[PublicAPI]
public class MistralConnection
{
    /// <summary>
    /// The API Key to be used
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Get a Raw Client
    /// </summary>
    /// <param name="rawHttpCallDetails">An Action, if set, will attach an HTTP Message Handler so you can see the raw HTTP Calls that are sent to the LLM</param>
    /// <returns>The Raw Client</returns>
    public MistralClient GetClient(Action<RawCallDetails>? rawHttpCallDetails = null)
    {
        HttpClient? httpClient = null;

        // ReSharper disable once InvertIf
        if (rawHttpCallDetails != null)
        {
            httpClient = new HttpClient(new RawCallDetailsHttpHandler(rawHttpCallDetails));
        }

        if (NetworkTimeout.HasValue)
        {
            httpClient ??= new HttpClient();
            httpClient.Timeout = NetworkTimeout.Value;
        }

        return new MistralClient(new APIAuthentication(ApiKey), httpClient);
    }
}