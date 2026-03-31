using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.AzureOpenAI.Batching;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenAI.Batching;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;
#pragma warning disable OPENAI001

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

        AzureOpenAIBatchRunner batchRunner = new(connection);
        ChatBatchRun<MyObject> run = await batchRunner.RunChatBatchAsync<MyObject>(new ChatBatchOptions
            {
                Model = "gpt-4.1-nano-batch",

            },
            [
                ChatBatchRequest.Create("What is the capital of France?"),
            ]
        );

        while (run.Status != BatchRunStatus.Completed)
        {
            run = await batchRunner.GetChatBatchAsync<MyObject>(run.Id);
            Console.WriteLine(run.Status+$" [Total: {run.Counts.Total} - Completed: {run.Counts.Completed} - Failed: {run.Counts.Failed}]");
            await Task.Delay(5000);
        }

        IList<ChatBatchRunResult<MyObject>> items = await run.GetResultAsync();

        foreach (ChatBatchRunResult<MyObject> item in items)
        {
            Console.WriteLine(item.ResponseObject?.City);
        }

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
