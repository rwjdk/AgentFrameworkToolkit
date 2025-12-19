using AgentFrameworkToolkit.Anthropic;
using Microsoft.Agents.AI;

namespace Samples.Providers;

public static class Anthropic
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        AnthropicAgentFactory factory = new(configuration.AnthropicApiKey);

        AnthropicAgent agent = factory.CreateAgent(new AnthropicAgentOptions
        {
            Model = AnthropicChatModels.ClaudeHaiku45,
            MaxOutputTokens = 2000,
            BudgetTokens = 1500,
            RawHttpCallDetails = details => { Console.WriteLine(details.RequestData); }
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}