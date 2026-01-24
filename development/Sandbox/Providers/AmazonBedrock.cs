using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AmazonBedrock;
using Amazon;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Sandbox.Providers;

public static class AmazonBedrock
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();

        //Create your AgentFactory (using a connection object for more options)
        AmazonBedrockAgentFactory agentFactory = new AmazonBedrockAgentFactory(new AmazonBedrockConnection
        {
            Region = RegionEndpoint.EUNorth1,
            ApiKey = secrets.AmazonBedrockApiKey,
            NetworkTimeout = TimeSpan.FromMinutes(5)
        });

        //Create your Agent
        AmazonBedrockAgent agent = agentFactory.CreateAgent(new AmazonBedrockAgentOptions
        {
            //Mandatory
            Model = "eu.anthropic.claude-haiku-4-5-20251001-v1:0",

            //Optional (Common)
            Name = "MyAgent",
            MaxOutputTokens = 2000,
        });

        AgentResponse response = await agent.RunAsync("Hello World");
        Console.WriteLine(response);
    }
}
