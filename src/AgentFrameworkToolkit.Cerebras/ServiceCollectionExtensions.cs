using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Cerebras;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register a CerebrasAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddCerebrasAgentFactory(this IServiceCollection services, CerebrasConnection connection)
    {
        return services.AddSingleton(new CerebrasAgentFactory(connection));
    }

    /// <summary>
    /// Register a CerebrasAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddCerebrasAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new CerebrasAgentFactory(apiKey));
    }
}
