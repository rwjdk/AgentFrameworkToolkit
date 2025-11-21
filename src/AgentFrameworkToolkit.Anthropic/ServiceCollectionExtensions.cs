using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Anthropic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnthropicAgentFactory(this IServiceCollection services, AnthropicConnection connection)
    {
        return services.AddSingleton(new AnthropicAgentFactory(connection));
    }
    
    public static IServiceCollection AddAnthropicAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new AnthropicAgentFactory(apiKey));
    }
}