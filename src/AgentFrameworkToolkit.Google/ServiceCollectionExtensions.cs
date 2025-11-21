using AgentFrameworkToolkit.Google;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Anthropic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleAgentFactory(this IServiceCollection services, GoogleConnection connection)
    {
        return services.AddSingleton(new GoogleAgentFactory(connection));
    }
    
    public static IServiceCollection AddGoogleAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new GoogleAgentFactory(apiKey));
    }
}