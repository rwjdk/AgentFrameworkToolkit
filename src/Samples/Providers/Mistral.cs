using AgentFrameworkToolkit.Anthropic;
using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.Mistral;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;

namespace Samples.Providers;

public static class Mistral
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        OpenRouterAgentFactory factory = new(new OpenRouterConnection
        {
            ApiKey = configuration.OpenRouterApiKey
        });

        OpenRouterAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
        {
            DeploymentModelName = OpenRouterChatModels.OpenAI.Gpt41Mini,
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}