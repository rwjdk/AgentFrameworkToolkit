using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;

namespace Samples.Providers;

public static class AzureOpenAI
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        AzureOpenAIAgentFactory factory = new(new AzureOpenAIConnection
        {
            Endpoint = configuration.AzureOpenAiEndpoint,
            ApiKey = configuration.AzureOpenAiKey,
        });

        AzureOpenAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
        {
            Model = OpenAIChatModels.Gpt41Mini,
        });

        AgentRunResponse response = await agent.RunAsync<string>("Hello");
        Console.WriteLine(response);
    }
}