using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

#pragma warning disable OPENAI001

namespace Samples.Providers;

public static class AzureOpenAI
{
    [AITool]
    static string GetWeather()
    {
        return "Sunny";
    }

    public static async Task RunAsync()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        AzureOpenAIConnection connection = new()
        {
            Endpoint = configuration.AzureOpenAiEndpoint,
            ApiKey = configuration.AzureOpenAiKey
        };

        AzureOpenAIAgentFactory factory = new(connection);

        AIToolsFactory aiToolsFactory = new();

        IList<AITool> aiTools = aiToolsFactory.GetTools(typeof(AzureOpenAI));
        AzureOpenAIAgent agent = factory.CreateAgent(new AgentOptions
        {
            ClientType = ClientType.ChatClient,
            Model = OpenAIChatModels.Gpt41Mini,
            Temperature = 0,
            Instructions = "Speak like a pirate",
            RawHttpCallDetails = details => Console.WriteLine(details.RequestData),
            Tools = aiTools,
        });

        AgentRunResponse response = await agent.RunAsync<string>("How is the weather?");
        Console.WriteLine(response);
    }
}