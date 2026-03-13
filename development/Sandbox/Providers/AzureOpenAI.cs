using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Sandbox.Providers;

class MyObject
{
    public required string City { get; set; }
    public required int PopulationInMillion { get; set; }
}

public static class AzureOpenAI
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();


        AzureOpenAIConnection connection = new AzureOpenAIConnection
        {
            Endpoint = secrets.AzureOpenAiEndpoint,
            ApiKey = secrets.AzureOpenAiKey,
        };

        BatchRunner batchRunner = new BatchRunner(connection);
        BatchRun<MyObject> run = await batchRunner.CreateBatchAsync<MyObject>(new BatchRunOptions
        {
            Model = "gpt-4.1-nano-batch",
            WaitUntilCompleted = true
        },
            [
              new BatchRunLine
              {
                  Messages = [new ChatMessage(ChatRole.User, "What is the capital of France?")]
              }  
            ]
            );


        run.DownloadStructuredResultsAsync<>()
        IReadOnlyList<BatchRunStructuredResultLine<MyObject>> batchRunStructuredResultLines = await run.DownloadStructuredResultsAsync();

        AzureOpenAIAgentFactory factory = new AzureOpenAIAgentFactory(connection);

        AzureOpenAIAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-5-mini",
            ReasoningEffort = OpenAIReasoningEffort.Low,
            RawToolCallDetails = Console.WriteLine
        });

        AgentSession session = await agent.CreateSessionAsync();

        AgentResponse response = await agent.RunAsync("What is the capital of France?", session);

        IList<ChatMessage> chatMessages = session.GetMessages();
    }

    public class MathResult
    {
        public required int Result { get; set; }
    }
}
