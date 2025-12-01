using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mistral.SDK;

namespace AgentFrameworkToolkit.Mistral;

/// <summary>
/// Factory for creating Mistral Agents
/// </summary>
[PublicAPI]
public class MistralAgentFactory
{
    private readonly MistralConnection _connection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your Mistral API Key (if you need a more advanced connection use the constructor overload)</param>
    public MistralAgentFactory(string apiKey)
    {
        _connection = new MistralConnection
        {
            ApiKey = apiKey
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public MistralAgentFactory(MistralConnection connection)
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
    public MistralAgent CreateAgent(string model, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new MistralAgentOptions
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
    public MistralAgent CreateAgent(MistralAgentOptions options)
    {
        IChatClient client = GetClient(options);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new MistralAgent(options.ApplyMiddleware(innerAgent));
        }

        return new MistralAgent(innerAgent);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(MistralAgentOptions options)
    {
        ChatOptions chatOptions = new()
        {
            ModelId = options.Model
        };

        if (options.Tools != null)
        {
            chatOptions.Tools = options.Tools;
        }

        chatOptions.MaxOutputTokens = options.MaxOutputTokens;

        if (options.Temperature != null)
        {
            chatOptions.Temperature = options.Temperature;
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Instructions = options.Instructions,
            Description = options.Description,
            Id = options.Id,
            ChatOptions = chatOptions
        };

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }


    private IChatClient GetClient(MistralAgentOptions options)
    {
        HttpClient? httpClient = null;

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            httpClient = new HttpClient(new RawCallDetailsHttpHandler(options.RawHttpCallDetails));
        }

        if (_connection.NetworkTimeout.HasValue)
        {
            httpClient ??= new HttpClient();
            httpClient.Timeout = _connection.NetworkTimeout.Value;
        }

        MistralClient mistralClient = new(new APIAuthentication(_connection.ApiKey), httpClient);
        return mistralClient.Completions;
    }
}