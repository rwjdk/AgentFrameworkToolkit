using AgentFrameworkToolkit.Groq;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Secrets;

namespace Sandbox.Providers;

public static class Groq
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        GroqAgentFactory factory = new(new GroqConnection
        {
            ApiKey = secrets.GroqApiKey
        });

        GroqAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = GroqChatModels.GptOss20B
        });

        AgentResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response.Text);
    }
}
