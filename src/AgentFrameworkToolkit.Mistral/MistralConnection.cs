namespace AgentFrameworkToolkit.Mistral;

/// <summary>
/// Represents a connection to the Mistral API
/// </summary>
public class MistralConnection
{
    /// <summary>
    /// The API Key for connecting to Mistral
    /// </summary>
    public required string ApiKey { get; set; }

    public TimeSpan? NetworkTimeout { get; set; }
}