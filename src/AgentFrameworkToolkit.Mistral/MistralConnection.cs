namespace AgentFrameworkToolkit.Mistral;

public class MistralConnection
{
    public required string ApiKey { get; set; }

    public TimeSpan? NetworkTimeout { get; set; }
}