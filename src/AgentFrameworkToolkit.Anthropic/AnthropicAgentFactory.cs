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
    /// <summary>
    /// Connection
    /// </summary>
    public AnthropicConnection Connection { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your Anthropic API Key (if you need a more advanced connection use the constructor overload)</param>
    public AnthropicAgentFactory(string apiKey)
    {
        Connection = new AnthropicConnection
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
        Connection = connection;
    }

    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the Model to use</param>
    /// <param name="maxOutputTokens">Max Token Count this Agent may use per call</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public AnthropicAgent CreateAgent(string model, int maxOutputTokens, string? instructions = null, string? name = null, IList<AITool>? tools = null)
    {
        return CreateAgent(new AnthropicAgentOptions
        {
            Model = model,
            MaxOutputTokens = maxOutputTokens,
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
        IChatClient client = Connection.GetClient(options.RawHttpCallDetails).AsIChatClient();

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options),
            options.LoggerFactory,
            options.Services);

        return new AnthropicAgent(MiddlewareHelper.ApplyMiddleware(
            innerAgent,
            options.RawToolCallDetails,
            options.ToolCallingMiddleware,
            options.OpenTelemetryMiddleware,
            options.LoggingMiddleware,
            options.Services));
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(AnthropicAgentOptions options)
    {
        ChatOptions chatOptions = new()
        {
            ModelId = options.Model,
            MaxOutputTokens = options.MaxOutputTokens,
        };

        if (options.Tools != null)
        {
            chatOptions.Tools = options.Tools;
        }

        if (!string.IsNullOrWhiteSpace(options.Instructions))
        {
            chatOptions.Instructions = options.Instructions;
        }

        if (options.Temperature != null)
        {
            chatOptions.Temperature = options.Temperature;
        }

        if (options.BudgetTokens != null)
        {
            chatOptions.RawRepresentationFactory = _ => new MessageCreateParams
            {
                MaxTokens = options.MaxOutputTokens,
                Messages = [],
                Model = options.Model,
                Thinking = new ThinkingConfigParam(new ThinkingConfigEnabled() { BudgetTokens = options.BudgetTokens.Value }),
            };
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Description = options.Description,
            Id = options.Id,
            ChatOptions = chatOptions,
            AIContextProviderFactory = options.AIContextProviderFactory,
            ChatMessageStoreFactory = options.ChatMessageStoreFactory
        };

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }
}
