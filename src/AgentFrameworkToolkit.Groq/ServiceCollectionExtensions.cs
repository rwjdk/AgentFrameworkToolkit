using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Groq;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register a <see cref="GroqAgentFactory"/> as a singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connection">Connection details.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddGroqAgentFactory(this IServiceCollection services, GroqConnection connection)
    {
        return services.AddSingleton(new GroqAgentFactory(connection));
    }

    /// <summary>
    /// Register a <see cref="GroqAgentFactory"/> as a singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddGroqAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new GroqAgentFactory(apiKey));
    }
}
