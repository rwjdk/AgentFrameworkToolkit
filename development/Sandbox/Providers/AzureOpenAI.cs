using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using AgentFrameworkToolkit.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Secrets;

#pragma warning disable OPENAI001

namespace Sandbox.Providers;

public static class AzureOpenAI
{
    [AITool]
    static string GetWeather()
    {
        return "Sunny af 30 degrees";
    }

    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();

        //Create your AgentFactory (using a connection object for more options)
        AzureOpenAIAgentFactory factory = new AzureOpenAIAgentFactory(new AzureOpenAIConnection
        {
            Endpoint = secrets.AzureOpenAiEndpoint,
            ApiKey = secrets.AzureOpenAiKey,
        });

        AzureOpenAIAgent agent = factory.CreateAgent(OpenAIChatModels.Gpt41, instructions: null);

        AgentResponse response2 = await agent.RunAsync("How is the weather?");
        Console.WriteLine(response2);
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
