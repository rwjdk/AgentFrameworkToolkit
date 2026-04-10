using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Anthropic;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Sandbox.Providers;

public static class Anthropic
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        string apiKey = secrets.AnthropicApiKey;

//Create your AgentFactory
        AnthropicAgentFactory agentFactory = new AnthropicAgentFactory(secrets.AnthropicApiKey);

        AnthropicAgent agent = agentFactory.CreateAgent(new AnthropicAgentOptions
        {
            Model = AnthropicChatModels.ClaudeOpus46,
            MaxOutputTokens = 10000,
            BudgetTokens = 2000,
            UseAdaptiveThinking = true
        });

        AgentResponse response = await agent.RunAsync("How may people live in france. Answer in exactly 5 words");

        TextReasoningContent? content = response.GetTextReasoningContent();

        //AgentResponse<Result> response = await agent.RunAsync<Result>([new ChatMessage(ChatRole.User, "What is 2+2")]);
    }

    class Result
    {
        public required int Answer { get; set; }
    }
}
