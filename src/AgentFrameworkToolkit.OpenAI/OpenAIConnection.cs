using OpenAI;

namespace AgentFrameworkToolkit.OpenAI;

public class OpenAIConnection
{
    public required string ApiKey { get; set; }

    public string? Endpoint { get; set; }

    public TimeSpan? NetworkTimeout { get; set; }

    public Action<OpenAIClientOptions>? AdditionalOpenAIClientOptions { get; set; }
}