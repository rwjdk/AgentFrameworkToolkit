using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Secrets;

namespace Sandbox.Providers;

public static class AzureOpenAI
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();

        AzureOpenAIAgentFactory factory = new AzureOpenAIAgentFactory(new AzureOpenAIConnection
        {
            Endpoint = secrets.AzureOpenAiEndpoint,
            ApiKey = secrets.AzureOpenAiKey,
        });

        AzureOpenAIAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-4.1-mini",
            RawToolCallDetails = Console.WriteLine
        });

        AIAgent agent2A = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-4.1-mini",
        });

        AIAgent agent2B = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-4.1-mini",
            RawToolCallDetails = Console.WriteLine
        });
        string question = "What is 2+2";

        AgentResponse<MathResult> response1 = await agent.RunAsync<MathResult>(question);

        AgentResponse<MathResult> response2A = await agent2A.RunAsync<MathResult>(question); //Fail do to Microsoft Bug
        
        AgentResponse<MathResult> response2B = await agent2B.RunAsync<MathResult>(question); //Fail do to Microsoft Bug

#pragma warning disable CS0618 // Type or member is obsolete
        AgentResponse<MathResult> response2BFix1 = await AgentFrameworkToolkit.AIAgentExtensions.RunAsync<MathResult>(agent2B, question);
#pragma warning restore CS0618 // Type or member is obsolete

        AgentResponse<MathResult> response3Fix2 = await ((Agent)agent2B).RunAsync<MathResult>(question);

    }

    public class MathResult
    {
        public required int Result { get; set; }
    }
}
