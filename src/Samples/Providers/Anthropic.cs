using AgentFrameworkToolkit.Anthropic;
using Microsoft.Agents.AI;
using Shared;

namespace Samples.Providers;

public static class Anthropic
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        AnthropicAgentFactory factory = new(configuration.AnthropicApiKey);

        AnthropicAgent agent = factory.CreateAgent(new AnthropicAgentOptions
        {
            DeploymentModelName = AnthropicChatModels.ClaudeHaiku45,
            MaxOutputTokens = 2000,
            BudgetTokens = 1500,
            RawHttpCallDetails = details => { Console.WriteLine(details.RequestJson); }
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}