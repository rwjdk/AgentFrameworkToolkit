using AgentFrameworkToolkit.Anthropic;
using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.XAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Samples.Providers;

public static class XAI
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        XAIAgentFactory factory = new(new XAIConnection
        {
            ApiKey = configuration.XAiGrokApiKey
        });

        XAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
        {
            Model = XAIChatModels.Grok4FastNonReasoning,
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}