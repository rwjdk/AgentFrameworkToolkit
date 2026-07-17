using System.Diagnostics.CodeAnalysis;
using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Groq;

/// <summary>
/// Represents a connection for Groq.
/// </summary>
[PublicAPI]
public class GroqConnection : OpenAIConnection
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public GroqConnection()
    {
        // Empty
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="apiKey">The API key to use.</param>
    [SetsRequiredMembers]
    public GroqConnection(string apiKey) : base(apiKey)
    {
        // Empty
    }

    /// <summary>
    /// The default Groq endpoint.
    /// </summary>
    public const string DefaultEndpoint = "https://api.groq.com/openai/v1";
}
