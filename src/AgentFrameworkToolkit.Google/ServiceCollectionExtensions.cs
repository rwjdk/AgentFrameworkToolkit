using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an GoogleAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddGoogleAgentFactory(this IServiceCollection services, GoogleConnection connection)
    {
        return services.AddSingleton(new GoogleAgentFactory(connection));
    }

    /// <summary>
    /// Register an GoogleAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddGoogleAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new GoogleAgentFactory(apiKey));
    }
}