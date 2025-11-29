using GenerativeAI.Core;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// Represents a connection for Google
/// </summary>
public class GoogleConnection
{
    /// <summary>
    /// API Key to use
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Interface for platform-specific operations required to integrate with external APIs.
    /// </summary>
    public IPlatformAdapter? Adapter { get; set; }
}