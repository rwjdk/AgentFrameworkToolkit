using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.OpenRouter;

/// <summary>
/// Represents a connection for OpenRouter
/// </summary>
[PublicAPI]
public class OpenRouterConnection : OpenAIConnection
{
    /// <summary>
    /// The Default OpenRouter Endpoint
    /// </summary>
    public const string DefaultEndpoint = "https://openrouter.ai/api/v1";
}