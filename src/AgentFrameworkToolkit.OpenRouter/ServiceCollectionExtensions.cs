using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.OpenRouter;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an OpenRouterAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddOpenRouterAgentFactory(this IServiceCollection services, OpenRouterConnection connection)
    {
        return services.AddSingleton(new OpenRouterAgentFactory(connection));
    }

    /// <summary>
    /// Register an OpenRouterAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddOpenRouterAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new OpenRouterAgentFactory(apiKey));
    }
}