using AgentFrameworkToolkit.OpenAI;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.AzureOpenAI;

public class AzureOpenAIAgentFactory
{
    private readonly AzureOpenAIConnection _connection;

    public AzureOpenAIAgentFactory(string endpoint, string apiKey)
    {
        _connection = new AzureOpenAIConnection
        {
            Endpoint = endpoint,
            ApiKey = apiKey
        };
    }

    public AzureOpenAIAgentFactory(AzureOpenAIConnection connection)
    {
        _connection = connection;
    }


    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="deploymentModelName">Name of the Model to use</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public AzureOpenAIAgent CreateAgent(string deploymentModelName, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
        {
            DeploymentModelName = deploymentModelName,
            Name = name,
            Instructions = instructions,
            Tools = tools
        });
    }

    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithoutReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = OpenAIAgentFactory.CreateChatClientAgentOptions(options, null, options, null, null);

        ChatClientAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
    }

    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = OpenAIAgentFactory.CreateChatClientAgentOptions(options, null, null, options, null);

        AIAgent innerAgent = client
            .GetOpenAIResponseClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
    }

    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithoutReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = OpenAIAgentFactory.CreateChatClientAgentOptions(options, options, null, null, null);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
    }

    public AzureOpenAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithReasoning options)
    {
        AzureOpenAIClient client = CreateClient(options);

        ChatClientAgentOptions chatClientAgentOptions = OpenAIAgentFactory.CreateChatClientAgentOptions(options, null, null, null, options);

        AIAgent innerAgent = client
            .GetChatClient(options.DeploymentModelName)
            .CreateAIAgent(chatClientAgentOptions);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AzureOpenAIAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AzureOpenAIAgent(innerAgent);
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