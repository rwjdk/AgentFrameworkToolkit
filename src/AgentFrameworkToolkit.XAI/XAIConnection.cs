using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.XAI;

/// <summary>
/// Represents a connection for XAI
/// </summary>
[PublicAPI]
public class XAIConnection : OpenAIConnection
{
    /// <summary>
    /// The Default XAI Endpoint
    /// </summary>
    public const string DefaultEndpoint = "https://api.x.ai/v1";
}