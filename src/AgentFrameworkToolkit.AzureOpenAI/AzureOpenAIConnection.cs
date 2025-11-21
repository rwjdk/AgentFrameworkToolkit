using Azure.AI.OpenAI;

namespace AgentFrameworkToolkit.AzureOpenAI;

public class AzureOpenAIConnection
{
    public required string Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public Action<AzureOpenAIClientOptions>? AdditionalAzureOpenAIClientOptions { get; set; }

    public TimeSpan? NetworkTimeout { get; set; }

    //todo - Support RBAC
}