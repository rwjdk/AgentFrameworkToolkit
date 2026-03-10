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

    [Fact]
    public async Task BatchRunner_SingleLine_WaitUntilCompleted()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        BatchRunner batchRunner = new(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);

        try
        {
            BatchRun batchRun = await batchRunner.CreateBatchAsync(
                new BatchRunOptions
                {
                    Model = "gpt-4.1-nano-batch",
                    ClientType = BatchClientType.ChatClient,
                    WaitUntilCompleted = true,
                    MaxOutputTokens = 128,
                    RawHttpCallDetails = details =>
                    {
                        Console.WriteLine("REQUEST URL:");
                        Console.WriteLine(details.RequestUrl);
                        Console.WriteLine("REQUEST DATA:");
                        Console.WriteLine(details.RequestData);
                        Console.WriteLine("RESPONSE DATA:");
                        Console.WriteLine(details.ResponseData);
                    }
                },
                [
                    new BatchRunLine
                    {
                        CustomId = "live-test-1",
                        Messages =
                        [
                            new ChatMessage(ChatRole.System, "You are a concise assistant."),
                            new ChatMessage(ChatRole.User, "Reply with the single word: pong")
                        ]
                    }
                ]);

            Console.WriteLine($"BatchId={batchRun.BatchId}; Status={batchRun.Status}; OutputFileId={batchRun.OutputFileId}; ErrorFileId={batchRun.ErrorFileId}");

            IReadOnlyList<BatchRunResultLine> results = await batchRun.DownloadResultsAsync();
            BatchRunResultLine result = Assert.Single(results);
            ChatMessage message = Assert.Single(result.Messages);

            Assert.Equal(ChatRole.Assistant, message.Role);
            Assert.NotEmpty(message.Text);
        }
        catch (AgentFrameworkToolkitException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    [Fact]
    public async Task BatchRunner_SingleLine_ResponsesApi_WaitUntilCompleted()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        BatchRunner batchRunner = new(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);

        BatchRun batchRun = await batchRunner.CreateBatchAsync(
            new BatchRunOptions
            {
                Model = "gpt-4.1-nano-batch",
                ClientType = BatchClientType.ResponsesApi,
                WaitUntilCompleted = true,
                MaxOutputTokens = 128,
                RawHttpCallDetails = details =>
                {
                    Console.WriteLine("REQUEST URL:");
                    Console.WriteLine(details.RequestUrl);
                    Console.WriteLine("REQUEST DATA:");
                    Console.WriteLine(details.RequestData);
                    Console.WriteLine("RESPONSE DATA:");
                    Console.WriteLine(details.ResponseData);
                }
            },
            [
                new BatchRunLine
                {
                    CustomId = "live-test-responses-1",
                    Messages =
                    [
                        new ChatMessage(ChatRole.System, "You are a concise assistant."),
                        new ChatMessage(ChatRole.User, "Reply with the single word: pong")
                    ]
                }
            ]);

        Console.WriteLine($"BatchId={batchRun.BatchId}; Status={batchRun.Status}; OutputFileId={batchRun.OutputFileId}; ErrorFileId={batchRun.ErrorFileId}");

        IReadOnlyList<BatchRunResultLine> results = await batchRun.DownloadResultsAsync();
        BatchRunResultLine result = Assert.Single(results);
        ChatMessage message = Assert.Single(result.Messages);

        Assert.Equal(ChatRole.Assistant, message.Role);
        Assert.NotEmpty(message.Text);
    }
}
