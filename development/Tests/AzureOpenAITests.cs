using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public sealed class AzureOpenAITests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple_ChatClient() => SimpleAgentTestsAsync(AgentProvider.AzureOpenAIChatClient);

    [Fact]
    public Task AgentFactory_Normal_ChatClient() => NormalAgentTestsAsync(AgentProvider.AzureOpenAIChatClient);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware_ChatClient() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.AzureOpenAIChatClient);

    [Fact]
    public Task AgentFactory_ToolCall_ChatClient() => ToolCallAgentTestsAsync(AgentProvider.AzureOpenAIChatClient);

    [Fact]
    public Task AgentFactory_McpToolCall_ChatClient() => McpToolCallAgentTestsAsync(AgentProvider.AzureOpenAIChatClient);

    [Fact]
    public Task AgentFactory_StructuredOutput_ChatClient() => StructuredOutputAgentTestsAsync(AgentProvider.AzureOpenAIChatClient);

    [Fact]
    public Task AgentFactory_Simple_ResponsesApi() => SimpleAgentTestsAsync(AgentProvider.AzureOpenAIResponsesApi);

    [Fact]
    public Task AgentFactory_Normal_ResponsesApi() => NormalAgentTestsAsync(AgentProvider.AzureOpenAIResponsesApi);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware_ResponsesApi() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.AzureOpenAIResponsesApi);

    [Fact]
    public Task AgentFactory_ToolCall_ResponsesApi() => ToolCallAgentTestsAsync(AgentProvider.AzureOpenAIResponsesApi);

    [Fact]
    public Task AgentFactory_McpToolCall_ResponsesApi() => McpToolCallAgentTestsAsync(AgentProvider.AzureOpenAIResponsesApi);

    [Fact]
    public Task AgentFactory_StructuredOutput_ResponsesApi() => StructuredOutputAgentTestsAsync(AgentProvider.AzureOpenAIResponsesApi);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAzureOpenAIAgentFactory(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<AzureOpenAIAgentFactory>()
            .CreateAgent(OpenAIChatModels.Gpt5Nano)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAzureOpenAIAgentFactory(new AzureOpenAIConnection
        {
            Endpoint = secrets.AzureOpenAiEndpoint,
            ApiKey = secrets.AzureOpenAiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string text = (await provider.GetRequiredService<AzureOpenAIAgentFactory>()
            .CreateAgent(OpenAIChatModels.Gpt5Nano)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;
        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task EmbeddingFactory()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        AzureOpenAIEmbeddingFactory factory = new(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);
        IEmbeddingGenerator<string, Embedding<float>> generator = factory.GetEmbeddingGenerator("text-embedding-3-small");
        Embedding<float> embedding = await generator.GenerateAsync("Hello", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(1536, embedding.Dimensions);
    }

    [Fact]
    public async Task EmbeddingFactory_DependencyInjection()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAzureOpenAIEmbeddingFactory(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        AzureOpenAIEmbeddingFactory embeddingFactory = provider.GetRequiredService<AzureOpenAIEmbeddingFactory>();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Embedding<float> embedding = await embeddingFactory.GetEmbeddingGenerator("text-embedding-3-small")
            .GenerateAsync("Hello", cancellationToken: cancellationToken);
        Assert.Equal(1536, embedding.Dimensions);
    }
}
