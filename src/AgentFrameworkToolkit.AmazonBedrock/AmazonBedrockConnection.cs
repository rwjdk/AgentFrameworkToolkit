using Amazon;
using Amazon.BedrockRuntime;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.AmazonBedrock;

/// <summary>
/// Represents a connection for Amazon Bedrock
/// </summary>
[PublicAPI]
public class AmazonBedrockConnection
{
    /// <summary>
    /// Amazon Bedrock API key (Bearer token). It will be applied via the
    /// <c>AWS_BEARER_TOKEN_BEDROCK</c> environment variable (used by the AWS Bedrock MEAI extensions).
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// AWS region for Amazon Bedrock Runtime
    /// </summary>
    public required RegionEndpoint Region { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Get a Raw Client
    /// </summary>
    /// <returns>The Raw Client</returns>
    public IAmazonBedrockRuntime GetClient()
    {
        Environment.SetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK", ApiKey);

        AmazonBedrockRuntimeConfig config = new()
        {
            RegionEndpoint = Region
        };

        if (NetworkTimeout.HasValue)
        {
            config.Timeout = NetworkTimeout.Value;
        }

        return new AmazonBedrockRuntimeClient(config);
    }
}
