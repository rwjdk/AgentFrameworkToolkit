using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Shared;

namespace Samples;

public static class OpenAI
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        OpenAIAgentFactory factory = new OpenAIAgentFactory(configuration.OpenAiApiKey);

        OpenAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForResponseApiWithoutReasoning
        {
            DeploymentModelName = "gpt-4.1-mini"
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}