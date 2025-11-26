using AgentFrameworkToolkit.Google;
using Microsoft.Agents.AI;
using Shared;

namespace Samples.Providers;

public static class OpenRouter
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        GoogleAgentFactory factory = new(new GoogleConnection
        {
            ApiKey = configuration.GoogleGeminiApiKey
        });

        GoogleAgent agent = factory.CreateAgent(new GoogleAgentOptions
        {
            DeploymentModelName = GoogleChatModels.Gemini25Flash,
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}