using AgentFrameworkToolkit.OpenAI;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.AzureOpenAI;

/// <summary>
/// Factory for creating AzureOpenAI Agents
/// </summary>
public class AzureOpenAIAgentFactory
{
    private readonly AzureOpenAIConnection _connection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endpoint">Your AzureOpenAI Endpoint (not to be confused with a Microsoft Foundry Endpoint. format: 'https://YourName.openai.azure.com' or 'https://YourName.services.azure.com')</param>
    /// <param name="apiKey">Your AzureOpenAI API Key (if you need a more advanced connection use the constructor overload)</param>
    public AzureOpenAIAgentFactory(string endpoint, string apiKey)
    {
        _connection = new AzureOpenAIConnection
        {
            Endpoint = endpoint,
            ApiKey = apiKey
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public AzureOpenAIAgentFactory(AzureOpenAIConnection connection)
    {
        _connection = connection;
    }


    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the model to use</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public AzureOpenAIAgent CreateAgent(string model, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
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
    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithoutReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, options, null, null);

        ChatClientAgent innerAgent = client
            .GetOpenAIResponseClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, options, null);

        AIAgent innerAgent = client
            .GetOpenAIResponseClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithoutReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, options, null, null, null);

        AIAgent innerAgent = client
            .GetChatClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = CreateChatClientAgentOptions(options, null, null, null, options);

        AIAgent innerAgent = client
            .GetChatClient(options.Model)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
    }

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

    private AzureOpenAIClient CreateClient(OpenAIAgentOptions options)
    {
        //todo - support RBAC
        AzureOpenAIClientOptions azureOpenAIClientOptions = new()
        {
            NetworkTimeout = _connection.NetworkTimeout
        };

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
            azureOpenAIClientOptions.Transport = new HttpClientPipelineTransport(inspectingHttpClient);
        }

        _connection.AdditionalAzureOpenAIClientOptions?.Invoke(azureOpenAIClientOptions);

        return new AzureOpenAIClient(new Uri(_connection.Endpoint), new ApiKeyCredential(_connection.ApiKey!), azureOpenAIClientOptions);
    }
}