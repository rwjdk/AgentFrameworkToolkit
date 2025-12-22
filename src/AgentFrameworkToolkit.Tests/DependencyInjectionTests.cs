using AgentFrameworkToolkit.Anthropic;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.GitHub;
using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.Mistral;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.XAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Secrets;

namespace AgentFrameworkToolkit.Tests;

public class DependencyInjectionTests
{
    [AITool]
    static string GetWeather(string city)
    {
        return "{ \"condition\": \"sunny\", \"degrees\":19 }";
    }

    [Fact]
    public void InjectAIToolFactoryFactoryTest()
    {
        ServiceCollection services = new();
        services.AddAIToolFactory();

        ServiceProvider provider = services.BuildServiceProvider();

        AIToolsFactory aiToolsFactory = provider.GetRequiredService<AIToolsFactory>();
        IList<AITool> tools = aiToolsFactory.GetTools(typeof(DependencyInjectionTests));
        Assert.Single(tools);
    }

    [Fact]
    public async Task InjectEmbeddingFactoryTestAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddAzureOpenAIEmbeddingFactory(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);
        services.AddOpenAIEmbeddingFactory(secrets.OpenAiApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        AzureOpenAIEmbeddingFactory azureOpenAIEmbeddingFactory = provider.GetRequiredService<AzureOpenAIEmbeddingFactory>();
        OpenAIEmbeddingFactory openAIEmbeddingFactory = provider.GetRequiredService<OpenAIEmbeddingFactory>();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        Embedding<float> azureOpenAIEmbedding = await azureOpenAIEmbeddingFactory.GetEmbeddingGenerator("text-embedding-3-small").GenerateAsync("Hello", cancellationToken: cancellationToken);
        Assert.Equal(1536, azureOpenAIEmbedding.Dimensions);

        Embedding<float> openAIEmbedding = await azureOpenAIEmbeddingFactory.GetEmbeddingGenerator("text-embedding-3-small").GenerateAsync("Hello", cancellationToken: cancellationToken);
        Assert.Equal(1536, openAIEmbedding.Dimensions);
    }

    [Fact]
    public async Task InjectAgentFactoryTestsAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();

        services.AddAnthropicAgentFactory(secrets.AnthropicApiKey);
        services.AddAzureOpenAIAgentFactory(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);
        services.AddGitHubAgentFactory(secrets.GitHubPatToken);
        services.AddGoogleAgentFactory(secrets.GoogleGeminiApiKey);
        services.AddMistralAgentFactory(secrets.MistralApiKey);
        services.AddOpenAIAgentFactory(secrets.OpenAiApiKey);
        services.AddOpenRouterAgentFactory(secrets.OpenRouterApiKey);
        services.AddXAIAgentFactory(secrets.XAiGrokApiKey);

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Assert.NotEmpty((await provider.GetRequiredService<AnthropicAgentFactory>().CreateAgent(AnthropicChatModels.ClaudeHaiku45, 2000).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<AzureOpenAIAgentFactory>().CreateAgent(OpenAIChatModels.Gpt5Nano).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<OpenAIAgentFactory>().CreateAgent(OpenAIChatModels.Gpt5Nano).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<OpenRouterAgentFactory>().CreateAgent(OpenRouterChatModels.OpenAI.Gpt5Nano).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<GoogleAgentFactory>().CreateAgent(GoogleChatModels.Gemini25Flash).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<MistralAgentFactory>().CreateAgent(MistalChatModels.MistralSmall).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<XAIAgentFactory>().CreateAgent(XAIChatModels.Grok41FastNonReasoning).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        //Assert.NotEmpty((await provider.GetRequiredService<GitHubAgentFactory>().CreateAgent("xai/grok-3-mini").RunAsync("Hello", cancellationToken: cancellationToken)).Text);
    }

    [Fact]
    public async Task InjectAgentFactoryTestsConnectionsAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();

        services.AddAnthropicAgentFactory(new AnthropicConnection
        {
            ApiKey = secrets.AnthropicApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        services.AddAzureOpenAIAgentFactory(new AzureOpenAIConnection
        {
            Endpoint = secrets.AzureOpenAiEndpoint,
            ApiKey = secrets.AzureOpenAiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });
        services.AddGitHubAgentFactory(new GitHubConnection
        {
            AccessToken = secrets.GitHubPatToken,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });
        services.AddGoogleAgentFactory(new GoogleConnection
        {
            ApiKey = secrets.GoogleGeminiApiKey
        });
        services.AddMistralAgentFactory(new MistralConnection
        {
            ApiKey = secrets.MistralApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });
        services.AddOpenAIAgentFactory(new OpenAIConnection
        {
            ApiKey = secrets.OpenAiApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });
        services.AddOpenRouterAgentFactory(new OpenRouterConnection
        {
            ApiKey = secrets.OpenRouterApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });
        services.AddXAIAgentFactory(new XAIConnection
        {
            ApiKey = secrets.XAiGrokApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Assert.NotEmpty((await provider.GetRequiredService<AnthropicAgentFactory>().CreateAgent(AnthropicChatModels.ClaudeHaiku45, 2000).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<AzureOpenAIAgentFactory>().CreateAgent(OpenAIChatModels.Gpt5Nano).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<OpenAIAgentFactory>().CreateAgent(OpenAIChatModels.Gpt5Nano).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<OpenRouterAgentFactory>().CreateAgent(OpenRouterChatModels.OpenAI.Gpt5Nano).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<GoogleAgentFactory>().CreateAgent(GoogleChatModels.Gemini25Flash).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<MistralAgentFactory>().CreateAgent(MistalChatModels.MistralSmall).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        Assert.NotEmpty((await provider.GetRequiredService<XAIAgentFactory>().CreateAgent(XAIChatModels.Grok41FastNonReasoning).RunAsync("Hello", cancellationToken: cancellationToken)).Text);
        //Assert.NotEmpty((await provider.GetRequiredService<GitHubAgentFactory>().CreateAgent("openAi/gpt-4.1-nano").RunAsync("Hello", cancellationToken: cancellationToken)).Text);
    }
}
