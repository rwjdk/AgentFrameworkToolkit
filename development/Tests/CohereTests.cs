using AgentFrameworkToolkit.Cohere;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class CohereTests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple_ChatClient() => SimpleAgentTestsAsync(AgentProvider.CohereChatClient);

    [Fact]
    public Task AgentFactory_Normal_ChatClient() => NormalAgentTestsAsync(AgentProvider.CohereChatClient);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware_ChatClient() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.CohereChatClient);

    [Fact]
    public Task AgentFactory_ToolCall_ChatClient() => ToolCallAgentTestsAsync(AgentProvider.CohereChatClient);

    [Fact]
    public Task AgentFactory_McpToolCall_ChatClient() => McpToolCallAgentTestsAsync(AgentProvider.CohereChatClient);

    [Fact]
    public Task AgentFactory_StructuredOutput_ChatClient() => StructuredOutputAgentTestsAsync(AgentProvider.CohereChatClient);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddCohereAgentFactory(secrets.CohereApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<CohereAgentFactory>()
            .CreateAgent("command-a-03-2025")
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddCohereAgentFactory(new CohereConnection
        {
            ApiKey = secrets.CohereApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<CohereAgentFactory>()
            .CreateAgent("command-a-03-2025")
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }
}
