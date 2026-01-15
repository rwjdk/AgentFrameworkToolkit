using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class GoogleTests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple() => SimpleAgentTestsAsync(AgentProvider.Google);

    [Fact]
    public Task AgentFactory_Normal() => NormalAgentTestsAsync(AgentProvider.Google);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.Google);

    [Fact]
    public Task AgentFactory_ToolCall() => ToolCallAgentTestsAsync(AgentProvider.Google);

    [Fact]
    public Task AgentFactory_McpToolCall() => McpToolCallAgentTestsAsync(AgentProvider.Google);

    [Fact]
    public Task AgentFactory_StructuredOutput() => StructuredOutputAgentTestsAsync(AgentProvider.Google);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        var secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddGoogleAgentFactory(secrets.GoogleGeminiApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        var cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<GoogleAgentFactory>()
            .CreateAgent(GoogleChatModels.Gemini25Flash)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        var secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddGoogleAgentFactory(new GoogleConnection
        {
            ApiKey = secrets.GoogleGeminiApiKey
        });

        ServiceProvider provider = services.BuildServiceProvider();

        var cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<GoogleAgentFactory>()
            .CreateAgent(GoogleChatModels.Gemini25Flash)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task EmbeddingFactory()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        GoogleEmbeddingFactory factory = new(secrets.GoogleGeminiApiKey);
        IEmbeddingGenerator<string, Embedding<float>> generator = factory.GetEmbeddingGenerator(GoogleEmbeddingModels.GoogleEmbedding001);
        Embedding<float> embedding = await generator.GenerateAsync("Hello", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(3072, embedding.Dimensions);
    }

    [Fact]
    public async Task EmbeddingFactory_DependencyInjection()
    {
        var secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddGoogleEmbeddingFactory(secrets.GoogleGeminiApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        GoogleEmbeddingFactory embeddingFactory = provider.GetRequiredService<GoogleEmbeddingFactory>();
        var cancellationToken = TestContext.Current.CancellationToken;
        Embedding<float> embedding = await embeddingFactory.GetEmbeddingGenerator(GoogleEmbeddingModels.GoogleEmbedding001)
            .GenerateAsync("Hello", cancellationToken: cancellationToken);
        Assert.Equal(3072, embedding.Dimensions);
    }
}
