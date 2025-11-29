namespace AgentFrameworkToolkit.Mistral;

/// <summary>
/// Represents a connection for Mistral
/// </summary>
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
}