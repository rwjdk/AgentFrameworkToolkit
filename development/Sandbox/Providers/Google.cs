using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Google;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Sandbox.Providers;

public static class Google
{
    static string GetWeather()
    {
        return "{ \"condition\": \"sunny\", \"degrees\":19 }";
    }

    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        //Create your AgentFactory (using a connection object for more options)
        GoogleAgentFactory agentFactory = new GoogleAgentFactory(new GoogleConnection
        {
            ApiKey = secrets.GoogleGeminiApiKey,
            
        });

        AIAgent agent = agentFactory.CreateAgent(new GoogleAgentOptions
        {
            Model = "gemini-3.1-pro-preview",
            RawHttpCallDetails = details =>
            {
                Console.WriteLine(details.RequestUrl);
                Console.WriteLine(details.RequestData);
                Console.WriteLine(details.ResponseData);
            }
        });
        await agent.RunAsync<string>("Hello");


        AgentResponse response = await agent.RunAsync("Why is the Sky Blue");
        Console.WriteLine(response);
    }
}
