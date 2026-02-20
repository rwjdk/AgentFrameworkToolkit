using AgentFrameworkToolkit.Anthropic;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class AnthropicTests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple() => SimpleAgentTestsAsync(AgentProvider.Anthropic);

    [Fact]
    public Task AgentFactory_Normal() => NormalAgentTestsAsync(AgentProvider.Anthropic);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.Anthropic);

    [Fact]
    public Task AgentFactory_ToolCall() => ToolCallAgentTestsAsync(AgentProvider.Anthropic);

    [Fact]
    public Task AgentFactory_McpToolCall() => McpToolCallAgentTestsAsync(AgentProvider.Anthropic);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAnthropicAgentFactory(secrets.AnthropicApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<AnthropicAgentFactory>()
            .CreateAgent(AnthropicChatModels.ClaudeHaiku45, 2000)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAnthropicAgentFactory(new AnthropicConnection
        {
            ApiKey = secrets.AnthropicApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<AnthropicAgentFactory>()
            .CreateAgent(AnthropicChatModels.ClaudeHaiku45, 2000)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }
}
