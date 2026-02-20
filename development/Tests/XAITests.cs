using AgentFrameworkToolkit.XAI;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class XAITests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple_ChatClient() => SimpleAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_Normal_ChatClient() => NormalAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware_ChatClient() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_ToolCall_ChatClient() => ToolCallAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_McpToolCall_ChatClient() => McpToolCallAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_StructuredOutput_ChatClient() => StructuredOutputAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_Simple_ResponsesApi() => SimpleAgentTestsAsync(AgentProvider.XAIResponsesApi);

    [Fact]
    public Task AgentFactory_Normal_ResponsesApi() => NormalAgentTestsAsync(AgentProvider.XAIResponsesApi);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware_ResponsesApi() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.XAIResponsesApi);

    [Fact]
    public Task AgentFactory_StructuredOutput_ResponsesApi() => StructuredOutputAgentTestsAsync(AgentProvider.XAIResponsesApi);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddXAIAgentFactory(secrets.XAiGrokApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<XAIAgentFactory>()
            .CreateAgent(XAIChatModels.Grok41FastNonReasoning)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddXAIAgentFactory(new XAIConnection
        {
            ApiKey = secrets.XAiGrokApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<XAIAgentFactory>()
            .CreateAgent(XAIChatModels.Grok41FastNonReasoning)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }
}
