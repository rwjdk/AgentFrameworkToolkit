using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;
using Ttl = Anthropic.Models.Beta.Messages.Ttl;

namespace Sandbox.Providers;

public static class AnthropicDemo
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
            UseAdaptiveThinking = true,
            RawHttpCallDetails = details =>
            {
                Console.WriteLine(details.RequestData);
                Console.WriteLine(details.ResponseData);
            }
        });

        string prompt = "How may people live in france. Answer in exactly 5 words.";
        AgentResponse response = await agent.RunAsync(prompt);

        TextReasoningContent? content = response.GetTextReasoningContent();

        AgentResponse response2 = await agent.RunAsync(prompt);

        //AgentResponse<Result> response = await agent.RunAsync<Result>([new ChatMessage(ChatRole.User, "What is 2+2")]);
    }

    class Result
    {
        public required int Answer { get; set; }
    }
}
