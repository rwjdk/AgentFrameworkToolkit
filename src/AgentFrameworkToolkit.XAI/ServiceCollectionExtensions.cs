using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.XAI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXAIAgentFactory(this IServiceCollection services, XAIConnection connection)
    {
        return services.AddSingleton(new XAIAgentFactory(connection));
    }
    
    public static IServiceCollection AddXAIAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new XAIAgentFactory(apiKey));
    }
}