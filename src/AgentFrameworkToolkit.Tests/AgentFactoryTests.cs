using AgentFrameworkToolkit.Anthropic;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.GitHub;
using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.Mistral;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.ModelContextProtocol;
using AgentFrameworkToolkit.XAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Secrets;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.Tests;

public class AgentFactoryTests
{
    private static McpClientTools? _mcpClientTools;
    private const string TestName = "MyAgent";
    private const string TestInstructions = "You are a nice AI";
    private const string TestDescription = "Sampledescription";

    static string GetWeatherWithServiceDependency(IServiceProvider serviceProvider, string city)
    {
        // ReSharper disable once UnusedVariable
        MyDi service = serviceProvider.GetRequiredService<MyDi>();
        return "{ \"condition\": \"sunny\", \"degrees\":19 }";
    }

    static string GetWeather(string city)
    {
        return "{ \"condition\": \"sunny\", \"degrees\":19 }";
    }

    [Theory]
    [InlineData(AgentProvider.AzureOpenAIChatClient)]
    [InlineData(AgentProvider.AzureOpenAIResponsesApi)]
    [InlineData(AgentProvider.OpenAIChatClient)]
    [InlineData(AgentProvider.OpenAIResponsesApi)]
    [InlineData(AgentProvider.Anthropic)]
    [InlineData(AgentProvider.Google)]
    [InlineData(AgentProvider.Mistral)]
    [InlineData(AgentProvider.OpenRouterChatClient)]
    [InlineData(AgentProvider.OpenRouterResponsesApi)]
    [InlineData(AgentProvider.XAIChatClient)]
    [InlineData(AgentProvider.XAIResponsesApi)]
    //[InlineData(AgentProvider.GitHub)] //Often hangs so not active by default :-(
    public async Task SimpleAgentTestsAsync(AgentProvider provider)
    {
        AIAgent agent = await GetAgentForScenarioAsync(provider, AgentScenario.Simple);
        Assert.NotNull(agent.Id);
        Assert.Equal(TestName, agent.Name);
        AgentRunResponse response = await agent.RunAsync("Hello", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(response.Messages);
        Assert.NotEmpty(response.Text);
    }

    [Theory]
    [InlineData(AgentProvider.AzureOpenAIChatClient)]
    [InlineData(AgentProvider.AzureOpenAIResponsesApi)]
    [InlineData(AgentProvider.OpenAIChatClient)]
    [InlineData(AgentProvider.OpenAIResponsesApi)]
    [InlineData(AgentProvider.Anthropic)]
    [InlineData(AgentProvider.Google)]
    [InlineData(AgentProvider.Mistral)]
    [InlineData(AgentProvider.OpenRouterChatClient)]
    [InlineData(AgentProvider.OpenRouterResponsesApi)]
    [InlineData(AgentProvider.XAIChatClient)]
    [InlineData(AgentProvider.XAIResponsesApi)]
    //[InlineData(AgentProvider.GitHub)] //Often hangs so not active by default :-(
    public async Task NormalAgentTestsAsync(AgentProvider provider)
    {
        TestLoggerFactory testLogger = new();
        AIAgent agent = await GetAgentForScenarioAsync(provider, AgentScenario.Normal, testLogger);
        Assert.NotNull(agent.Id);
        Assert.Equal(TestName, agent.Name);
        Assert.Equal(TestDescription, agent.Description);
        AgentRunResponse response = await agent.RunAsync("Hello", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(response.Messages);
        Assert.NotEmpty(response.Text);
        bool condition = testLogger.Logger.Messages.Any(x => x.Contains(agent.Id));
        Assert.True(condition);

        switch (provider)
        {
            case AgentProvider.AzureOpenAIChatClient:
            case AgentProvider.AzureOpenAIResponsesApi:
            case AgentProvider.OpenAIChatClient:
            case AgentProvider.OpenAIResponsesApi:
            case AgentProvider.OpenRouterChatClient:
            case AgentProvider.OpenRouterResponsesApi:
            case AgentProvider.XAIChatClient:
            case AgentProvider.XAIResponsesApi:
                Assert.Contains("ClientFactory Called", testLogger.Logger.Messages);
                break;
        }
    }

    [Theory]
    [InlineData(AgentProvider.AzureOpenAIChatClient)]
    [InlineData(AgentProvider.AzureOpenAIResponsesApi)]
    [InlineData(AgentProvider.OpenAIChatClient)]
    [InlineData(AgentProvider.OpenAIResponsesApi)]
    [InlineData(AgentProvider.Anthropic)]
    [InlineData(AgentProvider.Google)]
    [InlineData(AgentProvider.Mistral)]
    [InlineData(AgentProvider.OpenRouterChatClient)]
    [InlineData(AgentProvider.XAIChatClient)]
    //[InlineData(AgentProvider.GitHub)] //Often hangs so not active by default :-(
    //[InlineData(AgentProvider.OpenRouterResponsesApi)] //OpenRouter have issues combining ResponsesAPI with Tools
    //[InlineData(AgentProvider.XAIResponsesApi)] //XAI have issues combining ResponsesAPI with Tools
    public async Task ToolCallAgentTestsAsync(AgentProvider provider)
    {
        AIAgent agent = await GetAgentForScenarioAsync(provider, AgentScenario.ToolCall);
        AgentRunResponse response = await agent.RunAsync("What is the weather like in Paris", cancellationToken: TestContext.Current.CancellationToken);
        switch (provider)
        {
            case AgentProvider.Google:
                Assert.Single(response.Messages); //Do not give tool call details back
                break;
            default:
                Assert.Single(response.Messages.Where(x => x.Role == ChatRole.Tool).ToList());
                Assert.Equal(3, response.Messages.Count);
                break;
        }

        Assert.Contains("SUNNY", response.Text.ToUpperInvariant());
        Assert.Contains("19", response.Text);
    }

    [Theory]
    [InlineData(AgentProvider.AzureOpenAIChatClient)]
    [InlineData(AgentProvider.AzureOpenAIResponsesApi)]
    [InlineData(AgentProvider.OpenAIChatClient)]
    [InlineData(AgentProvider.OpenAIResponsesApi)]
    [InlineData(AgentProvider.Anthropic)]
    [InlineData(AgentProvider.Mistral)]
    [InlineData(AgentProvider.OpenRouterChatClient)]
    [InlineData(AgentProvider.XAIChatClient)]
    //[InlineData(AgentProvider.GitHub)] //Often hangs so not active by default :-(
    //[InlineData(AgentProvider.OpenRouterResponsesApi)] //OpenRouter have issues combining ResponsesAPI with Tools
    //[InlineData(AgentProvider.XAIResponsesApi)] //XAI have issues combining ResponsesAPI with Tools
    //[InlineData(AgentProvider.Google)] //Unofficial Google Provider fail internally to call here so add back when the real Google Provider comes online
    public async Task McpToolCallAgentTestsAsync(AgentProvider provider)
    {
        AIAgent agent = await GetAgentForScenarioAsync(provider, AgentScenario.McpToolCall);
        AgentRunResponse response = await agent.RunAsync("Call the 'getting_started' tool to find what URL the nuget is on", cancellationToken: TestContext.Current.CancellationToken);
        switch (provider)
        {
            case AgentProvider.Google:
                Assert.Single(response.Messages); //Do not give tool call details back
                break;
            default:
                Assert.True(response.Messages.Count(x => x.Role == ChatRole.Tool) > 0);
                break;
        }

        Assert.Contains("www.nuget.org/packages/TrelloDotNet".ToUpperInvariant(), response.Text.ToUpperInvariant());
    }

    [Theory]
    [InlineData(AgentProvider.AzureOpenAIChatClient)]
    [InlineData(AgentProvider.AzureOpenAIResponsesApi)]
    [InlineData(AgentProvider.OpenAIChatClient)]
    [InlineData(AgentProvider.OpenAIResponsesApi)]
    [InlineData(AgentProvider.Mistral)]
    [InlineData(AgentProvider.OpenRouterChatClient)]
    [InlineData(AgentProvider.OpenRouterResponsesApi)]
    [InlineData(AgentProvider.XAIChatClient)]
    [InlineData(AgentProvider.XAIResponsesApi)]
    //[InlineData(AgentProvider.GitHub)] //Often hangs so not active by default :-(
    //[InlineData(AgentProvider.Anthropic)] //Structured Output not supported
    //[InlineData(AgentProvider.Google)] //Structured Output not supported
    public async Task StructuredOutputAgentTestsAsync(AgentProvider provider)
    {
        AIAgent agent = await GetAgentForScenarioAsync(provider, AgentScenario.Normal);
        ChatClientAgentRunResponse<MovieResult> response = await agent.RunAsync<MovieResult>("Top 3 IMDB Movies", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(3, response.Result.Movies.Count);
    }


    private record MovieResult(List<Movie> Movies);

    private record Movie(string Title, int YearOfRelease);

    private async Task<AIAgent> GetAgentForScenarioAsync(AgentProvider provider, AgentScenario scenario, TestLoggerFactory? testLogger = null)
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton<MyDi>();
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        Func<IChatClient, IChatClient> clientFactory = client =>
        {
            testLogger?.Logger.Log(LogLevel.Debug, "ClientFactory Called");
            return client;
        };

        Secrets.Secrets secrets = SecretsManager.GetSecrets();

        IList<AITool> tools = await GetToolsForScenarioAsync(provider, scenario);
        switch (provider)
        {
            case AgentProvider.AzureOpenAIChatClient:
            {
                AzureOpenAIAgentFactory factory = new(new AzureOpenAIConnection
                {
                    Endpoint = secrets.AzureOpenAiEndpoint,
                    ApiKey = secrets.AzureOpenAiKey,
                    DefaultClientType = ClientType.ChatClient
                });
                string model = OpenAIChatModels.Gpt5Nano;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, secrets.AzureOpenAiEndpoint, ClientType.ChatClient)),
                };
            }
            case AgentProvider.AzureOpenAIResponsesApi:
            {
                AzureOpenAIAgentFactory factory = new(new AzureOpenAIConnection
                {
                    Endpoint = secrets.AzureOpenAiEndpoint,
                    ApiKey = secrets.AzureOpenAiKey,
                    DefaultClientType = ClientType.ResponsesApi
                });
                string model = OpenAIChatModels.Gpt5Nano;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, secrets.AzureOpenAiEndpoint, ClientType.ResponsesApi)),
                };
            }
            case AgentProvider.OpenAIChatClient:
            {
                OpenAIAgentFactory factory = new(new OpenAIConnection
                {
                    ApiKey = secrets.OpenAiApiKey,
                    DefaultClientType = ClientType.ChatClient
                });
                string model = OpenAIChatModels.Gpt5Nano;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, null, ClientType.ChatClient)),
                };
            }
            case AgentProvider.OpenAIResponsesApi:
            {
                OpenAIAgentFactory factory = new(new OpenAIConnection
                {
                    ApiKey = secrets.OpenAiApiKey,
                    DefaultClientType = ClientType.ChatClient
                });
                string model = OpenAIChatModels.Gpt5Nano;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, null, ClientType.ResponsesApi)),
                };
            }
            case AgentProvider.Anthropic:
            {
                AnthropicAgentFactory factory = new(secrets.AnthropicApiKey);
                string model = AnthropicChatModels.ClaudeHaiku45;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, 2000, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetAnthropicOptions(model)),
                };
            }
            case AgentProvider.GitHub:
            {
                GitHubAgentFactory factory = new(secrets.GitHubPatToken);
                string model = "xai/grok-3-mini";
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetGitHubAgentOptions(model)),
                };
            }
            case AgentProvider.Google:
            {
                GoogleAgentFactory factory = new(secrets.GoogleGeminiApiKey);
                string model = GoogleChatModels.Gemini25Flash;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetGoogleAgentOptions(model)),
                };
            }
            case AgentProvider.Mistral:
            {
                MistralAgentFactory factory = new(secrets.MistralApiKey);
                string model = MistalChatModels.MistralSmall;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetMistralAgentOptions(model)),
                };
            }
            case AgentProvider.OpenRouterChatClient:
            {
                OpenRouterAgentFactory factory = new(new OpenRouterConnection
                {
                    ApiKey = secrets.OpenRouterApiKey,
                    DefaultClientType = ClientType.ChatClient
                });
                string model = OpenRouterChatModels.OpenAI.Gpt5Nano;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, OpenRouterConnection.DefaultEndpoint, ClientType.ChatClient)),
                };
            }
            case AgentProvider.OpenRouterResponsesApi:
            {
                OpenRouterAgentFactory factory = new(new OpenRouterConnection
                {
                    ApiKey = secrets.OpenRouterApiKey,
                    DefaultClientType = ClientType.ResponsesApi
                });
                string model = OpenRouterChatModels.OpenAI.Gpt5Nano;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, OpenRouterConnection.DefaultEndpoint, ClientType.ResponsesApi)),
                };
            }
            case AgentProvider.XAIChatClient:
            {
                XAIAgentFactory factory = new(new XAIConnection
                {
                    ApiKey = secrets.XAiGrokApiKey,
                    DefaultClientType = ClientType.ChatClient
                });
                string model = XAIChatModels.Grok41FastNonReasoning;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, XAIConnection.DefaultEndpoint, ClientType.ChatClient)),
                };
            }
            case AgentProvider.XAIResponsesApi:
            {
                XAIAgentFactory factory = new(new XAIConnection
                {
                    ApiKey = secrets.XAiGrokApiKey,
                    DefaultClientType = ClientType.ResponsesApi
                });
                string model = XAIChatModels.Grok41FastNonReasoning;
                return scenario switch
                {
                    AgentScenario.Simple => factory.CreateAgent(model, TestInstructions, TestName, tools),
                    _ => factory.CreateAgent(await GetOpenAiBasedAgentOptions(model, XAIConnection.DefaultEndpoint, ClientType.ResponsesApi)),
                };
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }

        Task<AgentOptions> GetOpenAiBasedAgentOptions(string model, string? endpoint, ClientType clientType)
        {
            bool assertReasoning = false;
            AgentOptions options = new()
            {
                ClientType = clientType,
                Model = model,
                Description = TestDescription,
                Name = TestName,
                MaxOutputTokens = 2000,
                Instructions = TestInstructions,
                Tools = tools,
                Services = serviceProvider,
                LoggerFactory = testLogger,
                ClientFactory = clientFactory,
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                RawHttpCallDetails = details =>
                {
                    if (endpoint != null)
                    {
                        Assert.Contains(endpoint, details.RequestUrl);
                    }

                    switch (provider)
                    {
                        case AgentProvider.AzureOpenAIChatClient:
                        case AgentProvider.OpenAIChatClient:
                        case AgentProvider.OpenRouterChatClient:
                        case AgentProvider.XAIChatClient:
                            Assert.Contains("\"max_completion_tokens\": 2000", details.RequestData);
                            // ReSharper disable once AccessToModifiedClosure
                            if (assertReasoning)
                            {
                                Assert.Contains("\"reasoning_effort\": \"low\"", details.RequestData);
                            }

                            break;
                        case AgentProvider.AzureOpenAIResponsesApi:
                        case AgentProvider.OpenAIResponsesApi:
                        case AgentProvider.OpenRouterResponsesApi:
                        case AgentProvider.XAIResponsesApi:
                            Assert.Contains("\"max_output_tokens\": 2000", details.RequestData);
                            // ReSharper disable once AccessToModifiedClosure
                            if (assertReasoning)
                            {
                                Assert.Contains("\"effort\": \"low\"", details.RequestData);
                                Assert.Contains("\"summary\": \"detailed\"", details.RequestData);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
                    }

                    Assert.Contains(model, details.RequestData);
                    Assert.Contains(TestInstructions, details.RequestData);
                    Assert.Contains($"\"model\": \"{model}\"", details.RequestData);
                    foreach (AITool tool in tools)
                    {
                        Assert.Contains($"\"name\": \"{tool.Name}\"", details.RequestData);
                    }
                },
                RawToolCallDetails = Console.WriteLine
            };

            switch (model)
            {
                case OpenAIChatModels.Gpt5Nano:
                case OpenRouterChatModels.OpenAI.Gpt5Nano:
                    options.ReasoningEffort = OpenAIReasoningEffort.Low;
                    options.ReasoningSummaryVerbosity = OpenAIReasoningSummaryVerbosity.Detailed;
                    assertReasoning = true;
                    break;
                default:
                    options.Temperature = 0;
                    break;
            }

            return Task.FromResult(options);
        }

        Task<AnthropicAgentOptions> GetAnthropicOptions(string model)
        {
            return Task.FromResult(new AnthropicAgentOptions
            {
                Model = model,
                Name = TestName,
                Description = TestDescription,
                MaxOutputTokens = 2000,
                Tools = tools,
                Instructions = TestInstructions,
                BudgetTokens = 1024,
                LoggerFactory = testLogger,
                Services = serviceProvider,
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                RawHttpCallDetails = details =>
                {
                    Assert.Contains(model, details.RequestData);
                    Assert.Contains("\"max_tokens\": 2000", details.RequestData);
                    Assert.Contains("\"budget_tokens\": 1024", details.RequestData);
                    Assert.Contains(TestInstructions, details.RequestData);
                    Assert.Contains($"\"model\": \"{model}\"", details.RequestData);
                    foreach (AITool tool in tools)
                    {
                        Assert.Contains($"\"name\": \"{tool.Name}\"", details.RequestData);
                    }
                }
            });
        }

        Task<GitHubAgentOptions> GetGitHubAgentOptions(string model)
        {
            return Task.FromResult(new GitHubAgentOptions
            {
                Model = model,
                Name = TestName,
                Description = TestDescription,
                MaxOutputTokens = 2000,
                Instructions = TestInstructions,
                Tools = tools,
                Services = serviceProvider,
                LoggerFactory = testLogger,
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                RawHttpCallDetails = details =>
                {
                    Assert.Contains(model, details.RequestData);
                    Assert.Contains("\"max_tokens\": 2000,", details.RequestData);
                    Assert.Contains(TestInstructions, details.RequestData);
                    Assert.Contains($"\"model\": \"{model}\"", details.RequestData);
                    foreach (AITool tool in tools)
                    {
                        Assert.Contains($"\"name\": \"{tool.Name}\"", details.RequestData);
                    }
                }
            });
        }

        Task<GoogleAgentOptions> GetGoogleAgentOptions(string model)
        {
            return Task.FromResult(new GoogleAgentOptions
            {
                Model = model,
                MaxOutputTokens = 2000,
                Description = TestDescription,
                Name = TestName,
                Tools = tools,
                LoggerFactory = testLogger,
                Instructions = TestInstructions,
            });
        }

        Task<MistralAgentOptions> GetMistralAgentOptions(string model)
        {
            return Task.FromResult(new MistralAgentOptions
            {
                Model = model,
                Name = TestName,
                MaxOutputTokens = 2000,
                Description = TestDescription,
                Tools = tools,
                Services = serviceProvider,
                Instructions = TestInstructions,
                LoggerFactory = testLogger,
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                RawHttpCallDetails = details =>
                {
                    Assert.Contains(model, details.RequestData);
                    Assert.Contains("\"max_tokens\": 2000,", details.RequestData);
                    Assert.Contains(TestInstructions, details.RequestData);
                    Assert.Contains($"\"model\": \"{model}\"", details.RequestData);
                    foreach (AITool tool in tools)
                    {
                        Assert.Contains($"\"name\": \"{tool.Name}\"", details.RequestData);
                    }
                }
            });
        }
    }

    private static async Task<IList<AITool>> GetToolsForScenarioAsync(AgentProvider provider, AgentScenario scenario)
    {
        List<AITool> tools = [];
        switch (scenario)
        {
            case AgentScenario.ToolCall:
                switch (provider)
                {
                    case AgentProvider.Google:
                        tools = [AIFunctionFactory.Create(GetWeather, "get_weather")];
                        break;
                    default:
                        tools = [AIFunctionFactory.Create(GetWeatherWithServiceDependency, "get_weather")];
                        break;
                }

                break;
            case AgentScenario.McpToolCall:
                _mcpClientTools = await new AIToolsFactory().GetToolsFromRemoteMcpAsync("https://trellodotnetassistantbackend.azurewebsites.net/runtime/webhooks/mcp?code=Tools");
                return _mcpClientTools.Tools;
        }

        return tools;
    }
}

internal class MyDi
{
    //Empty
}

public enum AgentScenario
{
    Simple,
    Normal,
    ToolCall,
    McpToolCall,
}

public enum AgentProvider
{
    AzureOpenAIChatClient,
    AzureOpenAIResponsesApi,
    OpenAIChatClient,
    OpenAIResponsesApi,
    Anthropic,
    GitHub,
    Google,
    Mistral,
    OpenRouterChatClient,
    OpenRouterResponsesApi,
    XAIChatClient,
    XAIResponsesApi,
}
