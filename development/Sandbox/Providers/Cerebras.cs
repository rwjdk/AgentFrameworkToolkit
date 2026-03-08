using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Cerebras;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Secrets;

namespace Sandbox.Providers;

public static class Cerebras
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        CerebrasAgentFactory factory = new(new CerebrasConnection
        {
            ApiKey = secrets.CerebrasApiKey,
        });

        CerebrasAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = CerebrasChatModels.Llama318B,
        });

        AgentResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}
