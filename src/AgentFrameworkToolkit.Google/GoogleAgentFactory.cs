using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// Create an Agent Factory for creating Google Based Agents
/// </summary>
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
    /// <param name="connection">Connection Details for connecting and authenticating with Google</param>
    public GoogleAgentFactory(GoogleConnection connection)
    {
        _connection = connection;
    }

    public GoogleAgent CreateAgent(string model, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new GoogleAgentOptions
        {
            DeploymentModelName = model,
            Name = name,
            Instructions = instructions,
            Tools = tools
        });
    }

    /// <summary>
    /// Create an agent with the specified options
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public GoogleAgent CreateAgent(GoogleAgentOptions options)
    {
        IChatClient client = GetClient(options.DeploymentModelName);

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