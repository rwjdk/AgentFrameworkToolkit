using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using Microsoft.Agents.AI;
using Secrets;

namespace Samples.Providers;

public static class Mistral
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        OpenRouterAgentFactory factory = new(new OpenRouterConnection
        {
            ApiKey = secrets.OpenRouterApiKey
        });

        OpenRouterAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = OpenRouterChatModels.OpenAI.Gpt41Mini,
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}
