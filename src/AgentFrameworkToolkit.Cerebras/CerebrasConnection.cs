using System.Diagnostics.CodeAnalysis;
using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Cerebras;

/// <summary>
/// Represents a connection for Cerebras
/// </summary>
[PublicAPI]
public class CerebrasConnection : OpenAIConnection
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CerebrasConnection()
    {
        //Empty
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">The API Key to be used</param>
    [SetsRequiredMembers]
    public CerebrasConnection(string apiKey) : base(apiKey)
    {
        //Empty
    }

    /// <summary>
    /// The Default Cerebras Endpoint
    /// </summary>
    public const string DefaultEndpoint = "https://api.cerebras.ai/v1";
}
