using OpenAI;

namespace AgentFrameworkToolkit.OpenRouter;

/// <summary>
/// Represents a connection for OpenRouter
/// </summary>
public class OpenRouterConnection
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