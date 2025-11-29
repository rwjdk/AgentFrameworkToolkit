using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.GitHub;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an GitHubAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddGitHubAgentFactory(this IServiceCollection services, GitHubConnection connection)
    {
        return services.AddSingleton(new GitHubAgentFactory(connection));
    }

    /// <summary>
    /// Register an GitHubAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="apiKey">The API Key</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddGitHubAgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new GitHubAgentFactory(apiKey));
    }
}