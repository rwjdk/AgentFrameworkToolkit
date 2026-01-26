using Google.Apis.Auth.OAuth2;
using Google.GenAI;
using Google.GenAI.Types;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// Represents a connection for Google
/// </summary>
public class GoogleConnection
{
    /// <summary>
    /// Optional String for the <a href="https://ai.google.dev/gemini-api/docs/api-key">API key</a>. Gemini API only.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Vertex Only: Optional Boolean for whether to use Vertex AI APIs. If not specified here nor in the environment variable, defaults to false.
    /// </summary>
    public bool? VertexAI { get; set; }

    /// <summary>
    /// Vertex Only: Optional - see:Google.Apis.Auth.OAuth2.GoogleCredential.
    /// </summary>
    public ICredential? Credential { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Vertex Only: Optional String for the project ID. Find it here: https://cloud.google.com/resource-manager/docs/creating-managing-projects#identifying_projects
    /// </summary>
    public string? Project { get; set; }

    /// <summary>
    /// Vertex Only: Optional String for the location. See: https://cloud.google.com/vertex-ai/generative-ai/docs/learn/locations
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Options for the HTTP Request
    /// </summary>
    public HttpOptions? HttpOptions { get; set; }

    /// <summary>
    /// Get a Raw Client
    /// </summary>
    /// <returns></returns>
    public Client GetClient()
    {
        HttpOptions? httpOptions = HttpOptions;
        if (NetworkTimeout.HasValue)
        {
            if (httpOptions == null)
            {
                httpOptions = new HttpOptions
                {
                    Timeout = Convert.ToInt32(NetworkTimeout.Value.TotalMicroseconds)
                };
            }
            else
            {
                httpOptions.Timeout ??= Convert.ToInt32(NetworkTimeout.Value.TotalMicroseconds);
            }
        }

        Client client = new(
            vertexAI: VertexAI,
            apiKey: ApiKey,
            credential: Credential,
            project: Project,
            location: Location,
            httpOptions: httpOptions);

        return client;
    }
}
