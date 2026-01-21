using System.Runtime.CompilerServices;
using System.Text;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using AgentFrameworkToolkit.Tools;
using AgentSkillsDotNet;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
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


        ChatClientAgent a = factory.Connection.GetClient().GetChatClient(OpenRouterChatModels.OpenAI.Gpt41Nano).AsAIAgent();

        ChatClientAgentResponse<MovieResult> response = await a.RunAsync<MovieResult>("Give me the top 3 movies according to IMDB");

        OpenRouterAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = OpenRouterChatModels.OpenAI.Gpt41Nano,
            RawToolCallDetails = Console.WriteLine
        });

        ChatClientAgentResponse<MovieResult> response2 = await agent.RunAsync<MovieResult>("Give me the top 3 movies according to IMDB");
    }

    private class MovieResult
    {
        public required List<Movie> List { get; set; }
    }

    private class Movie
    {
        public required string Title { get; set; }
    }
}
