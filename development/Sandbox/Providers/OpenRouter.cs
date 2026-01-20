using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.Common;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Sandbox.Providers;

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
            ApiKey = secrets.OpenRouterApiKey
        });

        var toolsFactory = new AIToolsFactory();


        var weatherOptions = WeatherOptions.OpenWeatherMap(secrets.OpenWeatherApiKey, WeatherOptionsUnits.Metric);
        List<AITool> tools =
        [
            new WeatherTools(weatherOptions).GetWeatherForCity()
        ];


        OpenRouterAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = OpenRouterChatModels.OpenAI.Gpt41Nano,
            Tools = tools
        });

        AgentRunResponse response = await agent.RunAsync("What is the weather like in Paris?");
        Console.WriteLine(response);
    }
}
