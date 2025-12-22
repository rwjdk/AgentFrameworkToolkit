using Anthropic.Models.Messages;
using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MessageCreateParams = Anthropic.Models.Messages.MessageCreateParams;

namespace AgentFrameworkToolkit.Anthropic;

/// <summary>
/// Factory for creating Anthropic Agents
/// </summary>
[PublicAPI]
public class AnthropicAgentFactory
{
    private readonly AnthropicConnection _connection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your Anthropic API Key (if you need a more advanced connection use the constructor overload)</param>
    public AnthropicAgentFactory(string apiKey)
    {
        _connection = new AnthropicConnection
        {
            ApiKey = apiKey
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public AnthropicAgentFactory(AnthropicConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the Model to use</param>
    /// <param name="maxTokenCount">Max Token Count this Agent may use per call</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public AnthropicAgent CreateAgent(string model, int maxTokenCount, string? instructions = null, string? name = null, IList<AITool>? tools = null)
    {
        return CreateAgent(new AnthropicAgentOptions
        {
            Model = model,
            MaxOutputTokens = maxTokenCount,
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
    public AnthropicAgent CreateAgent(AnthropicAgentOptions options)
    {
        IChatClient client = _connection.GetClient(options.RawHttpCallDetails).AsIChatClient();

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options), options.LoggerFactory, options.Services);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AnthropicAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AnthropicAgent(innerAgent);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(AnthropicAgentOptions options)
    {
        ChatOptions chatOptions = new()
        {
            ModelId = options.Model,
            Instructions = options.Instructions
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

        if (options.BudgetTokens > 0)
        {
            chatOptions.RawRepresentationFactory = _ => new MessageCreateParams
            {
                MaxTokens = options.MaxOutputTokens,
                Messages = [],
                Model = options.Model,
                Thinking = new ThinkingConfigParam(new ThinkingConfigEnabled() { BudgetTokens = options.BudgetTokens }),
            };
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Description = options.Description,
            Id = options.Id,
            ChatOptions = chatOptions
        };

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }
}
