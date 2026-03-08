using AgentFrameworkToolkit.Cerebras;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class CerebrasTests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple_ChatClient() => SimpleAgentTestsAsync(AgentProvider.CerebrasChatClient);

    [Fact]
    public Task AgentFactory_Normal_ChatClient() => NormalAgentTestsAsync(AgentProvider.CerebrasChatClient);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware_ChatClient() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.CerebrasChatClient);

    [Fact]
    public Task AgentFactory_ToolCall_ChatClient() => ToolCallAgentTestsAsync(AgentProvider.CerebrasChatClient);

    [Fact]
    public Task AgentFactory_McpToolCall_ChatClient() => McpToolCallAgentTestsAsync(AgentProvider.CerebrasChatClient);

    [Fact]
    public Task AgentFactory_StructuredOutput_ChatClient() => StructuredOutputAgentTestsAsync(AgentProvider.CerebrasChatClient);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddCerebrasAgentFactory(secrets.CerebrasApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<CerebrasAgentFactory>()
            .CreateAgent(CerebrasChatModels.Llama318B)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddCerebrasAgentFactory(new CerebrasConnection
        {
            ApiKey = secrets.CerebrasApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<CerebrasAgentFactory>()
            .CreateAgent(CerebrasChatModels.Llama318B)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }
}
