using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.OpenAI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAIAgentFactory(this IServiceCollection services, OpenAIConnection connection)
    {
        return services.AddSingleton(new OpenAIAgentFactory(connection));
    }
    
    public static IServiceCollection AddOpenAIAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new OpenAIAgentFactory(apiKey));
    }
}