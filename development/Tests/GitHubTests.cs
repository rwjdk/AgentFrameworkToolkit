using AgentFrameworkToolkit.GitHub;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class GitHubTests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple() => SimpleAgentTestsAsync(AgentProvider.GitHub);

    [Fact]
    public Task AgentFactory_Normal() => NormalAgentTestsAsync(AgentProvider.GitHub);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.GitHub);

    [Fact]
    public Task AgentFactory_ToolCall() => ToolCallAgentTestsAsync(AgentProvider.GitHub);

    [Fact]
    public Task AgentFactory_McpToolCall() => McpToolCallAgentTestsAsync(AgentProvider.GitHub);

    [Fact]
    public Task AgentFactory_StructuredOutput() => StructuredOutputAgentTestsAsync(AgentProvider.GitHub);


    [Fact]
    public void AgentFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddGitHubAgentFactory(secrets.GitHubPatToken);

        ServiceProvider provider = services.BuildServiceProvider();

        GitHubAgentFactory factory = provider.GetRequiredService<GitHubAgentFactory>();
        Assert.NotNull(factory);
        // RunAsync often hangs with GitHub Models in CI, so we only verify resolution.
    }

    [Fact]
    public void AgentFactory_DependencyInjection_Connection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddGitHubAgentFactory(new GitHubConnection
        {
            AccessToken = secrets.GitHubPatToken,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        GitHubAgentFactory factory = provider.GetRequiredService<GitHubAgentFactory>();
        Assert.NotNull(factory);
        // RunAsync often hangs with GitHub Models in CI, so we only verify resolution.
    }
}
