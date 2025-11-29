using GenerativeAI.Microsoft;
using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// Factory for creating Google Agents
/// </summary>
[PublicAPI]
public class GoogleAgentFactory
{
    private readonly GoogleConnection? _connection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your Google API Key (if you need a more advanced connection use the constructor overload)</param>
    public GoogleAgentFactory(string apiKey)
    {
        _connection = new GoogleConnection
        {
            ApiKey = apiKey
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public GoogleAgentFactory(GoogleConnection connection)
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
    public GoogleAgent CreateAgent(string model, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new GoogleAgentOptions
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
    public GoogleAgent CreateAgent(GoogleAgentOptions options)
    {
        IChatClient client = GetClient(options.Model);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new GoogleAgent(options.ApplyMiddleware(innerAgent));
        }

        return new GoogleAgent(innerAgent);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(GoogleAgentOptions options)
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

        if (options.ThinkingBudget > 0)
        {
            anyOptionsSet = true;
            chatOptions.AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["ThinkingBudget"] = options.ThinkingBudget,
            };
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


    private IChatClient GetClient(string model)
    {
        IChatClient client;
        if (_connection?.Adapter != null)
        {
            client = new GenerativeAIChatClient(_connection.Adapter, model);
        }
        else if (_connection?.ApiKey != null)
        {
            client = new GenerativeAIChatClient(_connection.ApiKey, model);
        }
        else
        {
            throw new Exception("Missing Configuration"); //todo - custom exception + better message
        }

        //todo - Timeout???
        //Todo - Can RawHttpCallDetails somehow be supported?

        return client;
    }
}