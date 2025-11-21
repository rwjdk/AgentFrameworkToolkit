using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Mistral;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMistralAgentFactory(this IServiceCollection services, MistralConnection connection)
    {
        return services.AddSingleton(new MistralAgentFactory(connection));
    }
    
    public static IServiceCollection AddAnthropicAIAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new MistralAgentFactory(apiKey));
    }
}