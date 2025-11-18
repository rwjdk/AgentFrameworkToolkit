using OpenAI;

namespace AgentFrameworkToolkit.XAI;

public class XAIConnection
{
    public required string ApiKey { get; set; }

    public Action<OpenAIClientOptions>? AdditionalOpenAIClientOptions { get; set; }
}