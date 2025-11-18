using System.ClientModel;
using System.ClientModel.Primitives;
using AgentFrameworkToolkit.OpenAI;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.AzureOpenAI;

public class AzureOpenAIAgentFactory(AzureOpenAIConnection connection)
{
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
            NetworkTimeout = options.NetworkTimeout
        };

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            HttpClient inspectingHttpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
            azureOpenAIClientOptions.Transport = new HttpClientPipelineTransport(inspectingHttpClient);
        }

        connection.AdditionalAzureOpenAIClientOptions?.Invoke(azureOpenAIClientOptions);

        return new AzureOpenAIClient(new Uri(connection.Endpoint), new ApiKeyCredential(connection.ApiKey!), azureOpenAIClientOptions);
    }
}