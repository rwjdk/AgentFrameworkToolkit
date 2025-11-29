using OpenAI;

namespace AgentFrameworkToolkit.XAI;

/// <summary>
/// Represents a connection for XAI
/// </summary>
public class XAIConnection
{
    /// <summary>
    /// The API Key to be used
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// An Action that allow you to set additional options on the OpenAIClientOptions
    /// </summary>
    public Action<OpenAIClientOptions>? AdditionalOpenAIClientOptions { get; set; }
}