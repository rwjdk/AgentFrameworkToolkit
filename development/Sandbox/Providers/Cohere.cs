using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Cohere;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Sandbox.Providers;

public static class Cohere
{
    static string GetWeather(string city)
    {
        return "{ \"condition\": \"sunny\", \"degrees\":19 }";
    }

    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        CohereAgentFactory factory = new(new CohereConnection
        {
            ApiKey = secrets.CohereApiKey,
        });

        CohereAgent agent = factory.CreateAgent(
            new AgentOptions
            {
                ClientType = ClientType.ChatClient,
                Model = "command-a-03-2025",
                MaxOutputTokens = 2000,
                Instructions = "You are a nice AI",
                Tools = [AIFunctionFactory.Create(GetWeather, "get_weather")],
            }
        );

        AgentResponse<WeatherReport> response = await agent.RunAsync<WeatherReport>("What is the Weather like in Paris?");
        Console.WriteLine(response);
    }

    class WeatherReport
    {
        public required string Condition { get; set; }
    }
}
