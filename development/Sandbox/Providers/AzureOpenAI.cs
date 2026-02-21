using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.Common;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Responses;
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

        AzureOpenAIAgent aiAgent = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-4.1-mini",
            RawToolCallDetails = Console.WriteLine
        });

        AgentResponse<int> runAsync = await aiAgent.RunAsync<int>("What is 2+2");

        List<AITool> tools = [];
        tools.AddRange(EmailTools.All(new EmailToolsOptions
        {
            Host = "send.one.com",
            FromAddress = "mail@rwj.dk",
            ConfineSendingToTheseDomains = ["gmail.com"],
            FromDisplayName = "RWJ",
            Port = 587,
            UseSecureConnection = true,
            Username = secrets.EmailUsername,
            Password = secrets.EmailPassword
        }));

        AzureOpenAIAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-4.1-mini",
            Tools = tools,
            RawToolCallDetails = Console.WriteLine 
        });


        AgentResponse agentResponse = await agent.RunAsync("send a mail to rwj@relewise.com with a poem about ducks");
    }

    public class MathResult
    {
        public required int Result { get; set; }
    }
}
