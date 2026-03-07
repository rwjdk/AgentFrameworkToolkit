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

        AnthropicAgent agent = agentFactory.CreateAgent(AnthropicChatModels.ClaudeOpus46, 10000);

        AgentResponse<List<Result>> response = await agent.RunAsync<List<Result>>(new ChatMessage(ChatRole.User, "What is 2+2, 4+4 and 8+8"));
        //AgentResponse<Result> response = await agent.RunAsync<Result>([new ChatMessage(ChatRole.User, "What is 2+2")]);
    }

    class Result
    {
        public required int Answer { get; set; }
    }
}
