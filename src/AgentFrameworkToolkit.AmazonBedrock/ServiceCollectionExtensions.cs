using Amazon;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.AmazonBedrock;

/// <summary>
/// Service Collection Extensions for Amazon Bedrock
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add an AmazonBedrockAgentFactory to DI
    /// </summary>
    /// <param name="services">The Service Collection</param>
    /// <param name="region">AWS region for Amazon Bedrock Runtime</param>
    /// <param name="apiKey">Amazon Bedrock API key (Bearer token)</param>
    /// <returns>The Service Collection</returns>
    public static IServiceCollection AddAmazonBedrockAgentFactory(this IServiceCollection services, RegionEndpoint region, string apiKey)
    {
        services.AddSingleton(new AmazonBedrockAgentFactory(region, apiKey));
        return services;
    }

    /// <summary>
    /// Add an AmazonBedrockAgentFactory to DI
    /// </summary>
    /// <param name="services">The Service Collection</param>
    /// <param name="connection">Connection Details</param>
    /// <returns>The Service Collection</returns>
    public static IServiceCollection AddAmazonBedrockAgentFactory(this IServiceCollection services, AmazonBedrockConnection connection)
    {
        services.AddSingleton(new AmazonBedrockAgentFactory(connection));
        return services;
    }
}
