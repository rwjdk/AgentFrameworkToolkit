using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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
    private readonly OpenAIConnection _connection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your OpenAI API Key (if you need a more advanced connection use the constructor overload)</param>
    public OpenAIAgentFactory(string apiKey)
    {
        _connection = new OpenAIConnection
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
        _connection = connection;
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
        OpenAIClient client = _connection.GetClient(options.RawHttpCallDetails);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options);

        ChatClientAgent innerAgent;
        switch (options.ClientType)
        {
            case ClientType.ChatClient:
                innerAgent = client
                    .GetChatClient(options.Model)
                    .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services);
                break;
            case ClientType.ResponsesApi:
                innerAgent = client
                    .GetResponsesClient(options.Model)
                    .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services);
                break;
            case null:
                innerAgent = _connection.DefaultClientType switch
                {
                    ClientType.ChatClient => client
                        .GetChatClient(options.Model)
                        .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services),
                    ClientType.ResponsesApi => client
                        .GetResponsesClient(options.Model)
                        .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services),
                    _ => throw new ArgumentOutOfRangeException()
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithoutReasoning options)
    {
        OpenAIClient client = _connection.GetClient(options.RawHttpCallDetails);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, options, null, null);

        ChatClientAgent innerAgent = client
            .GetResponsesClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithReasoning options)
    {
        OpenAIClient client = _connection.GetClient(options.RawHttpCallDetails);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, options, null);

        AIAgent innerAgent = client
            .GetResponsesClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithoutReasoning options)
    {
        OpenAIClient client = _connection.GetClient(options.RawHttpCallDetails);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, options, null, null, null);

        AIAgent innerAgent = client
            .GetChatClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithReasoning options)
    {
        OpenAIClient client = _connection.GetClient(options.RawHttpCallDetails);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, null, options);

        AIAgent innerAgent = client
            .GetChatClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions, options.ClientFactory, options.LoggerFactory, options.Services);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    private ChatClientAgentOptions CreateChatClientAgentOptions(OpenAIAgentOptions options, OpenAIAgentOptionsForChatClientWithoutReasoning? chatClientWithoutReasoning, OpenAIAgentOptionsForResponseApiWithoutReasoning? responseWithoutReasoning, OpenAIAgentOptionsForResponseApiWithReasoning? responsesApiReasoningOptions, OpenAIAgentOptionsForChatClientWithReasoning? chatClientReasoningOptions)
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

        if (chatClientWithoutReasoning?.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = chatClientWithoutReasoning.Temperature;
        }

        if (responseWithoutReasoning?.Temperature != null)
        {
            anyOptionsSet = true;
            chatOptions.Temperature = responseWithoutReasoning.Temperature;
        }

        if (!string.IsNullOrWhiteSpace(options.Instructions))
        {
            anyOptionsSet = true;
            chatOptions.Instructions = options.Instructions;
        }

        if (responsesApiReasoningOptions != null)
        {
            if (responsesApiReasoningOptions.ReasoningEffort != null || responsesApiReasoningOptions.ReasoningSummaryVerbosity.HasValue)
            {
                anyOptionsSet = true;
                chatOptions = chatOptions.WithOpenAIResponsesApiReasoning(responsesApiReasoningOptions.ReasoningEffort, responsesApiReasoningOptions.ReasoningSummaryVerbosity);
            }
        }

        if (chatClientReasoningOptions?.ReasoningEffort != null)
        {
            anyOptionsSet = true;
            chatOptions = chatOptions.WithOpenAIChatClientReasoning(chatClientReasoningOptions.ReasoningEffort);
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Description = options.Description,
            Id = options.Id,
        };
        if (anyOptionsSet)
        {
            chatClientAgentOptions.ChatOptions = chatOptions;
        }

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }

    private ChatClientAgentOptions CreateChatClientAgentOptions(AgentOptions options)
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

        if (!string.IsNullOrWhiteSpace(options.Instructions))
        {
            anyOptionsSet = true;
            chatOptions.Instructions = options.Instructions;
        }

        string? reasoningEffortAsString = null;
        switch (options.ReasoningEffort)
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

            switch (options.ClientType)
            {
                case ClientType.ChatClient:
                    chatOptions = chatOptions.WithOpenAIChatClientReasoning(new ChatReasoningEffortLevel(reasoningEffortAsString));
                    break;
                case ClientType.ResponsesApi:
                    chatOptions = options.ReasoningSummaryVerbosity switch
                    {
                        OpenAIReasoningSummaryVerbosity.Auto => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Auto),
                        OpenAIReasoningSummaryVerbosity.Concise => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Concise),
                        OpenAIReasoningSummaryVerbosity.Detailed => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString), ResponseReasoningSummaryVerbosity.Detailed),
                        null => chatOptions.WithOpenAIResponsesApiReasoning(new ResponseReasoningEffortLevel(reasoningEffortAsString)),
                        _ => chatOptions
                    };

                    break;
                case null:
                    chatOptions = _connection.DefaultClientType switch
                    {
                        ClientType.ChatClient => chatOptions.WithOpenAIChatClientReasoning(new ChatReasoningEffortLevel(reasoningEffortAsString)),
                        ClientType.ResponsesApi => options.ReasoningSummaryVerbosity switch
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
        };
        if (anyOptionsSet)
        {
            chatClientAgentOptions.ChatOptions = chatOptions;
        }

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }
}
