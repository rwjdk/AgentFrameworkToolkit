using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Cohere;

/// <summary>
/// Represents a connection for Cohere
/// </summary>
[PublicAPI]
public class CohereConnection : OpenAIConnection
{
    /// <summary>
    /// The Default Cohere Endpoint
    /// </summary>
    public const string DefaultEndpoint = "https://api.cohere.ai/compatibility/v1";
}