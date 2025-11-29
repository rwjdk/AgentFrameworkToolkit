using OpenAI;

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Represents a connection for OpenAI
/// </summary>
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
}