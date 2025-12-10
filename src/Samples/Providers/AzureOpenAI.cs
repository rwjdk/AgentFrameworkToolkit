using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;
#pragma warning disable OPENAI001

namespace Samples.Providers;

public static class AzureOpenAI
{
    public static async Task RunAsync()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        AzureOpenAIAgentFactory factory = new(new AzureOpenAIConnection
        {
            Endpoint = configuration.AzureOpenAiEndpoint,
            Credentials = new AzureCliCredential()
        });

        AzureOpenAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithReasoning
        {
            Model = OpenAIChatModels.Gpt41Mini,
            ReasoningEffort = ChatReasoningEffortLevel.Low,
            Instructions = "Speak like a pirate",
            RawHttpCallDetails = details => Console.WriteLine(details.RequestData)
        });

        AgentRunResponse response = await agent.RunAsync<string>("Hello");
        Console.WriteLine(response);
    }
}