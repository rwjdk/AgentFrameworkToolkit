using Azure;
using Azure.AI.Inference;
using Azure.Core.Pipeline;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.GitHub;

/// <summary>
/// Factory for creating GitHub Model Agents
/// </summary>
public class GitHubAgentFactory
{
    private readonly GitHubConnection _connection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="personalAccessToken">The GitHub Personal Access Token with Models Access</param>
    public GitHubAgentFactory(string personalAccessToken)
    {
        _connection = new GitHubConnection
        {
            PersonalAccessToken = personalAccessToken
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public GitHubAgentFactory(GitHubConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the Model to use</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public GitHubAgent CreateAgent(string model, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new GitHubAgentOptions
        {
            Model = model,
            Name = name,
            Instructions = instructions,
            Tools = tools
        });
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public GitHubAgent CreateAgent(GitHubAgentOptions options)
    {
        IChatClient client = GetClient(options);
        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new GitHubAgent(options.ApplyMiddleware(innerAgent));
        }

        return new GitHubAgent(innerAgent);
    }

    private ChatClientAgentOptions CreateChatClientAgentOptions(GitHubAgentOptions options)
    {
        bool anyOptionsSet = false;
        ChatOptions chatOptions = new();
        if (options.Tools != null)
        {
            anyOptionsSet = true;
            chatOptions.Tools = options.Tools;
        }

        if (options.MaxOutputTokens.HasValue)
        {
            anyOptionsSet = true;
            chatOptions.MaxOutputTokens = options.MaxOutputTokens.Value;
        }

        if (options.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = options.Temperature;
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Instructions = options.Instructions,
            Description = options.Description,
            Id = options.Id
        };
        if (anyOptionsSet)
        {
            chatClientAgentOptions.ChatOptions = chatOptions;
        }

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }

    private IChatClient GetClient(GitHubAgentOptions options)
    {
        AzureAIInferenceClientOptions clientOptions = new();

        if (options.RawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
            if (_connection.NetworkTimeout != null)
            {
                inspectingHttpClient.Timeout = _connection.NetworkTimeout.Value;
            }

            clientOptions.Transport = new HttpClientTransport(inspectingHttpClient);
        }
        else if (_connection.NetworkTimeout != null)
        {
            clientOptions.Transport = new HttpClientTransport(new HttpClient
            {
                Timeout = _connection.NetworkTimeout.Value
            });
        }

        _connection.AdditionalAzureAIInferenceClientOptions?.Invoke(clientOptions);

        ChatCompletionsClient client = new(
            new Uri("https://models.github.ai/inference"),
            new AzureKeyCredential(_connection.PersonalAccessToken),
            clientOptions);

        return client.AsIChatClient(options.Model);
    }
}