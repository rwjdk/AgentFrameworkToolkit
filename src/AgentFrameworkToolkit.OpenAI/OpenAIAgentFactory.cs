using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Factory for creating OpenAI Agents
/// </summary>
[PublicAPI]
public class OpenAIAgentFactory
{
    /// <summary>
    /// Connection
    /// </summary>
    public OpenAIConnection Connection { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your OpenAI API Key (if you need a more advanced connection use the constructor overload)</param>
    public OpenAIAgentFactory(string apiKey)
    {
        Connection = new OpenAIConnection
        {
            ApiKey = apiKey
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public OpenAIAgentFactory(OpenAIConnection connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Create a simple Agent with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the Model to use</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public OpenAIAgent CreateAgent(string model, string? instructions = null, string? name = null, IList<AITool>? tools = null)
    {
        return CreateAgent(new AgentOptions
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
    public OpenAIAgent CreateAgent(AgentOptions options)
    {
        OpenAIClient client = Connection.GetClient(options.RawHttpCallDetails);

        ChatClientAgent innerAgent = GetChatClientAgent(options, client, options.Model, Connection.DefaultClientType);
        return new OpenAIAgent(MiddlewareHelper.ApplyMiddleware(
            innerAgent,
            options.RawToolCallDetails,
            options.ToolCallingMiddleware,
            options.OpenTelemetryMiddleware,
            options.LoggingMiddleware,
            options.Services));
    }

    internal static ChatClientAgent GetChatClientAgent(AgentOptions options, OpenAIClient client, string model, ClientType defaultClientType)
    {
        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, defaultClientType);
        Func<IChatClient, IChatClient>? clientFactory = options.ClientFactory;
        ILoggerFactory? loggerFactory = options.LoggerFactory;
        IServiceProvider? services = options.Services;
        return options.ClientType switch
        {
            ClientType.ChatClient => client.GetChatClient(model).AsAIAgent(chatClientAgentOptions, clientFactory, loggerFactory, services),
            ClientType.ResponsesApi => client.GetResponsesClient(model).AsAIAgent(chatClientAgentOptions, clientFactory, loggerFactory, services),
            null => defaultClientType switch
            {
                ClientType.ChatClient => client.GetChatClient(model).AsAIAgent(chatClientAgentOptions, clientFactory, loggerFactory, services),
                ClientType.ResponsesApi => client.GetResponsesClient(model).AsAIAgent(chatClientAgentOptions, clientFactory, loggerFactory, services),
                _ => throw new ArgumentOutOfRangeException()
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(AgentOptions options, ClientType defaultClientType)
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

        if (options.Temperature != null && !OpenAIChatModels.ReasoningModels.Contains(options.Model))
        {
            anyOptionsSet = true;
            chatOptions.Temperature = options.Temperature;
        }

        string? instructions = options.Instructions;
        if (!string.IsNullOrWhiteSpace(instructions))
        {
            anyOptionsSet = true;
            chatOptions.Instructions = instructions;
        }

        string? reasoningEffortAsString = null;
        OpenAIReasoningEffort? effort = options.ReasoningEffort;
        switch (effort)
        {
            case OpenAIReasoningEffort.None:
                reasoningEffortAsString = "none";
                break;
            case OpenAIReasoningEffort.Minimal:
                reasoningEffortAsString = "minimal";
                break;
            case OpenAIReasoningEffort.Low:
                reasoningEffortAsString = "low";
                break;
            case OpenAIReasoningEffort.Medium:
                reasoningEffortAsString = "medium";
                break;
            case OpenAIReasoningEffort.High:
                reasoningEffortAsString = "high";
                break;
            case OpenAIReasoningEffort.ExtraHigh:
                reasoningEffortAsString = "xhigh";
                break;
        }

        if (!string.IsNullOrWhiteSpace(reasoningEffortAsString) && !OpenAIChatModels.NonReasoningModels.Contains(options.Model))
        {
            anyOptionsSet = true;

            OpenAIReasoningSummaryVerbosity? summaryVerbosity = options.ReasoningSummaryVerbosity;
            switch (options.ClientType)
            {
                case ClientType.ChatClient:
                    chatOptions = chatOptions.WithOpenAIChatClientReasoning(new ChatReasoningEffortLevel(reasoningEffortAsString));
                    break;
                case ClientType.ResponsesApi:
                    chatOptions = summaryVerbosity switch
                    {
                        OpenAIReasoningSummaryVerbosity.Auto => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Auto),
                        OpenAIReasoningSummaryVerbosity.Concise => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Concise),
                        OpenAIReasoningSummaryVerbosity.Detailed => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Detailed),
                        null => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString)),
                        _ => chatOptions
                    };

                    break;
                case null:
                    chatOptions = defaultClientType switch
                    {
                        ClientType.ChatClient => chatOptions.WithOpenAIChatClientReasoning(new ChatReasoningEffortLevel(reasoningEffortAsString)),
                        ClientType.ResponsesApi => summaryVerbosity switch
                        {
                            OpenAIReasoningSummaryVerbosity.Auto => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Auto),
                            OpenAIReasoningSummaryVerbosity.Concise => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Concise),
                            OpenAIReasoningSummaryVerbosity.Detailed => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Detailed),
                            null => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString)),
                            _ => chatOptions
                        },
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
