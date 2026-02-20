using AgentFrameworkToolkit.AmazonBedrock;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class AmazonBedrockTests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple() => SimpleAgentTestsAsync(AgentProvider.AmazonBedrock);

    [Fact]
    public Task AgentFactory_Normal() => NormalAgentTestsAsync(AgentProvider.AmazonBedrock);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.AmazonBedrock);

    [Fact]
    public Task AgentFactory_ToolCall() => ToolCallAgentTestsAsync(AgentProvider.AmazonBedrock);

    [Fact]
    public Task AgentFactory_McpToolCall() => McpToolCallAgentTestsAsync(AgentProvider.AmazonBedrock);

    [Fact]
    public Task AgentFactory_StructuredOutput() => StructuredOutputAgentTestsAsync(AgentProvider.AmazonBedrock);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAmazonBedrockAgentFactory(RegionEndpoint.EUNorth1, secrets.AmazonBedrockApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<AmazonBedrockAgentFactory>()
            .CreateAgent("eu.anthropic.claude-haiku-4-5-20251001-v1:0")
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAmazonBedrockAgentFactory(new AmazonBedrockConnection
        {
            Region = RegionEndpoint.EUNorth1,
            ApiKey = secrets.AmazonBedrockApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<AmazonBedrockAgentFactory>()
            .CreateAgent("eu.anthropic.claude-haiku-4-5-20251001-v1:0")
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }
}
