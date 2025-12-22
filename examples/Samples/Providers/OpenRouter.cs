using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Samples.Providers;

public static class OpenRouter
{
    static string GetWeather(string city)
    {
        return "{ \"condition\": \"sunny\", \"degrees\":19 }";
    }

    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        OpenRouterAgentFactory factory = new(new OpenRouterConnection
        {
            ApiKey = secrets.OpenRouterApiKey,
            DefaultClientType = ClientType.ResponsesApi
        });

        OpenRouterAgent agent = factory.CreateAgent(
            new AgentOptions
            {
                //ClientType = ClientType.ResponsesApi,
                Model = OpenRouterChatModels.OpenAI.Gpt5Nano,
                Description = "Test",
                Name = "Test",
                MaxOutputTokens = 2000,
                Instructions = "You are a weather expert",
                ReasoningEffort = OpenAIReasoningEffort.Low,
                ReasoningSummaryVerbosity = OpenAIReasoningSummaryVerbosity.Detailed,
                Tools = [AIFunctionFactory.Create(GetWeather, "get_weather")],
                RawHttpCallDetails = details => { Console.Write("dd"); },
                RawToolCallDetails = details => { }
            }
        );

        AgentRunResponse response = await agent.RunAsync("What is the Weather like in Paris?");
        Console.WriteLine(response);
    }
}
