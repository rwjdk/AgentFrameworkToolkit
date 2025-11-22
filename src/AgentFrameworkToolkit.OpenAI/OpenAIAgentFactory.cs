using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Create an OpenAI Agent Factory
/// </summary>
public class OpenAIAgentFactory
{
    private readonly OpenAIConnection _connection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">API Key for your OpenAI API Account (for more advanced connections use the constructor overload)</param>
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
    /// <param name="connection">The OpenAI Connection details</param>
    public OpenAIAgentFactory(OpenAIConnection connection)
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
    public OpenAIAgent CreateAgent(string model, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
        {
            DeploymentModelName = model,
            Name = name,
            Instructions = instructions,
            Tools = tools
        });
    }

    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithoutReasoning options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, options, null, null);

        ChatClientAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithReasoning options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, options, null);

        AIAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithoutReasoning options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, options, null, null, null);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    public OpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithReasoning options)
    {
        OpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, null, options);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new OpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new OpenAIAgent(innerAgent);
    }

    private OpenAIClient CreateClient(OpenAIAgentOptions options)
    {
        OpenAIClientOptions openAIClientOptions = new()
        {
            NetworkTimeout = _connection.NetworkTimeout
        };

        if (!string.IsNullOrWhiteSpace(_connection.Endpoint))
        {
            openAIClientOptions.Endpoint = new Uri(_connection.Endpoint);
        }

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
            openAIClientOptions.Transport = new HttpClientPipelineTransport(inspectingHttpClient);
        }

        _connection.AdditionalOpenAIClientOptions?.Invoke(openAIClientOptions);

        return new OpenAIClient(new ApiKeyCredential(_connection.ApiKey), openAIClientOptions);
    }

    public static ChatClientAgentOptions CreateChatClientAgentOptions(OpenAIAgentOptions options, OpenAIAgentOptionsForChatClientWithoutReasoning? chatClientWithoutReasoning, OpenAIAgentOptionsForResponseApiWithoutReasoning? responseWithoutReasoning, OpenAIAgentOptionsForResponseApiWithReasoning? responsesApiReasoningOptions, OpenAIAgentOptionsForChatClientWithReasoning? chatClientReasoningOptions)
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

        if (responsesApiReasoningOptions != null)
        {
            if (responsesApiReasoningOptions.ReasoningEffort != null || responsesApiReasoningOptions.ReasoningSummaryVerbosity.HasValue)
            {
                anyOptionsSet = true;
                chatOptions.RawRepresentationFactory = _ =>
                {
                    ResponseCreationOptions responseCreationOptions = new()
                    {
                        ReasoningOptions = new ResponseReasoningOptions
                        {
                            ReasoningEffortLevel = responsesApiReasoningOptions.ReasoningEffort,
                            ReasoningSummaryVerbosity = responsesApiReasoningOptions.ReasoningSummaryVerbosity
                        }
                    };
                    return responseCreationOptions;
                };
            }
        }

        if (chatClientReasoningOptions?.ReasoningEffort != null)
        {
            anyOptionsSet = true;
            chatOptions.RawRepresentationFactory = _ => new ChatCompletionOptions
            {
                ReasoningEffortLevel = chatClientReasoningOptions.ReasoningEffort
            };
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Instructions = options.Instructions,
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