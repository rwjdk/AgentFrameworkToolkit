using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using Microsoft.Agents.AI;

namespace Samples.Providers;

public static class Mistral
{
    public static async Task RunAsync()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        OpenRouterAgentFactory factory = new(new OpenRouterConnection
        {
            ApiKey = configuration.OpenRouterApiKey
        });

        OpenRouterAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = OpenRouterChatModels.OpenAI.Gpt41Mini,
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}