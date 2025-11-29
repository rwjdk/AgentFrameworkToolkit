using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.XAI;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an XAIAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddXAIAgentFactory(this IServiceCollection services, XAIConnection connection)
    {
        return services.AddSingleton(new XAIAgentFactory(connection));
    }

    /// <summary>
    /// Register an XAIAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddXAIAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new XAIAgentFactory(apiKey));
    }
}