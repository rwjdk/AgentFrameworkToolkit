using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an OpenAIAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddOpenAIAgentFactory(this IServiceCollection services, OpenAIConnection connection)
    {
        return services.AddSingleton(new OpenAIAgentFactory(connection));
    }

    /// <summary>
    /// Register an OpenAIAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddOpenAIAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new OpenAIAgentFactory(apiKey));
    }
}