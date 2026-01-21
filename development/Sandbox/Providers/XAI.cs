using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.XAI;
using Microsoft.Agents.AI;
using Secrets;

namespace Sandbox.Providers;

public static class XAI
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        XAIAgentFactory factory = new(new XAIConnection
        {
            ApiKey = secrets.XAiGrokApiKey,
        });

        XAIAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = XAIChatModels.Grok4FastNonReasoning,
        });

        AgentResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}
