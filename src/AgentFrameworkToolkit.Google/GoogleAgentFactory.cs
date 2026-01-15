using Google.GenAI;
using Google.GenAI.Types;
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
    /// <summary>
    /// Connection
    /// </summary>
    public GoogleConnection Connection { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your Google API Key (if you need a more advanced connection use the constructor overload)</param>
    public GoogleAgentFactory(string apiKey)
    {
        Connection = new GoogleConnection
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
        Connection = connection;
    }

    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the Model to use</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public GoogleAgent CreateAgent(string model, string? instructions = null, string? name = null, IList<AITool>? tools = null)
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
        IChatClient client = Connection.GetClient().AsIChatClient(options.Model);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options), options.LoggerFactory, options.Services);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null || options.ToolCallingMiddleware != null || options.OpenTelemetryMiddleware != null || options.LoggingMiddleware != null)
        {
            return new GoogleAgent(MiddlewareHelper.ApplyMiddleware(
                innerAgent,
                options.RawToolCallDetails,
                options.ToolCallingMiddleware,
                options.OpenTelemetryMiddleware,
                options.LoggingMiddleware,
                options.Services));
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

        if (options.ThinkingBudget.HasValue || options.ThinkingLevel.HasValue)
        {
            anyOptionsSet = true;
            if (options.ThinkingLevel.HasValue)
            {
                chatOptions.RawRepresentationFactory = _ => new GenerateContentConfig
                {
                    ThinkingConfig = new ThinkingConfig
                    {
                        ThinkingLevel = options.ThinkingLevel,
                        IncludeThoughts = options.IncludeThoughts
                    }
                };
            }
            else if (options.ThinkingBudget.HasValue)
            {
                chatOptions.RawRepresentationFactory = _ => new GenerateContentConfig
                {
                    ThinkingConfig = new ThinkingConfig
                    {
                        ThinkingBudget = options.ThinkingBudget.Value,
                        IncludeThoughts = options.IncludeThoughts
                    }
                };
            }
        }

        if (options.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = options.Temperature;
        }


        if (!string.IsNullOrWhiteSpace(options.Instructions))
        {
            anyOptionsSet = true;
            chatOptions.Instructions = options.Instructions;
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Description = options.Description,
            Id = options.Id,
            AIContextProviderFactory = options.AIContextProviderFactory,
            ChatMessageStoreFactory = options.ChatMessageStoreFactory,
        };
        if (anyOptionsSet)
        {
            chatClientAgentOptions.ChatOptions = chatOptions;
        }

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }
}
