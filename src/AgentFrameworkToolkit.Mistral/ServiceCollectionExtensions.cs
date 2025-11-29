using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Mistral;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an MistralAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddMistralAgentFactory(this IServiceCollection services, MistralConnection connection)
    {
        return services.AddSingleton(new MistralAgentFactory(connection));
    }

    /// <summary>
    /// Register an MistralAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddMistralAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new MistralAgentFactory(apiKey));
    }
}