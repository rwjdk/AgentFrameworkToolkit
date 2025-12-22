using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Extensions.AI;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public class EmbeddingFactoryTests
{
    [Fact]
    public async Task AzureOpenAIEmbeddingTestsAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        AzureOpenAIEmbeddingFactory factory = new(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);
        IEmbeddingGenerator<string, Embedding<float>> generator = factory.GetEmbeddingGenerator("text-embedding-3-small");
        Embedding<float> embedding = await generator.GenerateAsync("Hello", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(1536, embedding.Dimensions);
    }

    [Fact]
    public async Task OpenAIEmbeddingTestsAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        OpenAIEmbeddingFactory factory = new(secrets.OpenAiApiKey);
        IEmbeddingGenerator<string, Embedding<float>> generator = factory.GetEmbeddingGenerator("text-embedding-3-small");
        Embedding<float> embedding = await generator.GenerateAsync("Hello", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(1536, embedding.Dimensions);
    }
}
