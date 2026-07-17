using System.ClientModel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.MicrosoftFoundry;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an MicrosoftFoundryAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddMicrosoftFoundryAgentFactory(this IServiceCollection services, MicrosoftFoundryConnection connection)
    {
        return services.AddSingleton(new MicrosoftFoundryAgentFactory(connection));
    }

    /// <summary>
    /// Register an MicrosoftFoundryAgentFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <param name="endpoint">Your Microsoft Foundry Project Endpoint</param>
    /// <param name="authenticationTokenProvider">Optional TokenProvider used for credentials; if not provided DefaultAzureCredential will be used</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddMicrosoftFoundryAgentFactory(this IServiceCollection services, string endpoint, AuthenticationTokenProvider? authenticationTokenProvider = null)
    {
        return services.AddSingleton(new MicrosoftFoundryAgentFactory(endpoint, authenticationTokenProvider));
    }
}
