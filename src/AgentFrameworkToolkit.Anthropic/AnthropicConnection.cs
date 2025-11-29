namespace AgentFrameworkToolkit.Anthropic;

/// <summary>
/// Represents a connection for Anthropic
/// </summary>
public class AnthropicConnection
{
    /// <summary>
    /// The API Key to be used
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }
}