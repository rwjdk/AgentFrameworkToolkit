using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Tools;

/// <summary>
/// Extension Methods for IServiceCollection
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register an AIToolFactory as a Singleton
    /// </summary>
    /// <param name="services">The IServiceCollection collection</param>
    /// <returns>The ServiceCollection</returns>
    public static IServiceCollection AddAIToolFactory(this IServiceCollection services)
    {
        return services.AddSingleton(new AIToolsFactory());
    }
}