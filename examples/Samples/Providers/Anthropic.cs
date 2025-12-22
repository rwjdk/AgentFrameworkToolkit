using AgentFrameworkToolkit.Anthropic;
using Microsoft.Agents.AI;
using Secrets;

namespace Samples.Providers;

public static class Anthropic
{
    public static async Task Run()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        AnthropicAgentFactory factory = new(secrets.AnthropicApiKey);

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
