using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.AzureOpenAI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureOpenAIAgentFactory(this IServiceCollection services, AzureOpenAIConnection connection)
    {
        return services.AddSingleton(new AzureOpenAIAgentFactory(connection));
    }
    
    public static IServiceCollection AddAzureOpenAIAgentFactory(this IServiceCollection services, string endpoint, string apiKey)
    {
        return services.AddSingleton(new AzureOpenAIAgentFactory(endpoint, apiKey));
    }
}