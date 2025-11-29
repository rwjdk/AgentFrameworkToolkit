using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Anthropic;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an AnthropicAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddAnthropicAgentFactory(this IServiceCollection services, AnthropicConnection connection)
    {
        return services.AddSingleton(new AnthropicAgentFactory(connection));
    }

    /// <summary>
    /// Register an AnthropicAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddAnthropicAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new AnthropicAgentFactory(apiKey));
    }
}