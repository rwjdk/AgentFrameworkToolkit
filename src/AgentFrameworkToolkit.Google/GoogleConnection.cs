using GenerativeAI.Core;

namespace AgentFrameworkToolkit.Google;

public class GoogleConnection
{
    public string? ApiKey { get; set; }
    public IPlatformAdapter? Adapter { get; set; }
}