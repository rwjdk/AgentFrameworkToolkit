using System.ClientModel;
using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.AzureOpenAI.Batching;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenAI.Batching;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Secrets;
#pragma warning disable OPENAI001
#pragma warning disable AFT999

namespace Sandbox.Providers;


class MyObject
{
    public required string City { get; set; }
    public required int PopulationInMillion { get; set; }
}

public static class AzureOpenAI
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();

        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
        ChatClientAgent agent2 = client.GetResponsesClient().AsAIAgent(model: "gpt-4.1");
        AgentResponse agentResponse = await agent2.RunAsync("Hello");


        AzureOpenAIConnection connection = new AzureOpenAIConnection
        {
            Endpoint = secrets.AzureOpenAiEndpoint,
            ApiKey = secrets.AzureOpenAiKey,
        };
        
        AzureOpenAIAgentFactory factory = new AzureOpenAIAgentFactory(connection);

        AzureOpenAIAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-5-mini",
            ReasoningEffort = OpenAIReasoningEffort.Low,
            ClientType = ClientType.ResponsesApi,
            
            RawToolCallDetails = Console.WriteLine
        });

        AgentSession session = await agent.CreateSessionAsync();

        AgentResponse response = await agent.RunAsync("What is the capital of France?", session);

        IList<ChatMessage> chatMessages = session.GetMessages();
    }

    public class MathResult
    {
        public required int Result { get; set; }
    }
}
