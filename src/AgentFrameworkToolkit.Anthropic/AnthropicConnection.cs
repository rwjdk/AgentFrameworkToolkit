namespace AgentFrameworkToolkit.Anthropic;

public class AnthropicConnection
{
    public required string ApiKey { get; set; }

    public TimeSpan? NetworkTimeout { get; set; }
}