using AgentFrameworkToolkit.MicrosoftFoundry;
using AgentFrameworkToolkit.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Responses;
using Secrets;
#pragma warning disable OPENAI001

namespace Sandbox.Providers;

public static class MicrosoftFoundry
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        MicrosoftFoundryConnection connection = new(secrets.MicrosoftFoundryEndpoint, new AzureCliCredential());

        MicrosoftFoundryAgentFactory factory = new(connection);

        //MicrosoftFoundryAgent agent2 = factory.DeclarativeAgentFactory.CreateAgent("myTest", "gpt-5.6-luna");
        /*
        MicrosoftFoundryAgent agent3 = factory.DeclarativeAgentFactory.CreateAgent(new DeclarativeAgentOptions
        {
            Name = "MyCoolAgent",
            Model = "gpt-5.6-luna",
            Instructions = "Speak like a pirate",
            ReasoningEffort = ResponseReasoningEffortLevel.Low,
            WebSearchTool = true,
            CodeInterpreterTool = true,
            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed
        });
        */
        /*
        IList<MicrosoftFoundryAgent> agents = factory.DeclarativeAgentFactory.GetAgents();

        foreach (MicrosoftFoundryAgent a in agents)
        {
            IList<ProjectsAgentVersion> versions = factory.DeclarativeAgentFactory.GetAgentVersions(a.Name!);
            Console.WriteLine("");
        }

        IList<ProjectsAgentVersion> agentVersions = factory.DeclarativeAgentFactory.GetAgentVersions("myTest");
        */
        //MicrosoftFoundryAgent agent = factory.DeclarativeAgentFactory.GetAgent("myTest", "1");

        
        MicrosoftFoundryAgent agent3 = factory.CreateAgent(new AgentOptions
        {
            Model = "gpt-5.6-luna",RawHttpCallDetails = details =>
            {

                Console.WriteLine(details.RequestData);
                Console.WriteLine(details.ResponseData);
            }
        });
        /*
        AIProjectClient client = connection.GetClient();

        ChatClientAgent agent = client.GetProjectOpenAIClient().GetProjectResponsesClient().AsAIAgent(model: "gpt-5.6-luna");
        */
        AgentSession session = await agent3.CreateSessionAsync();

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            AgentResponse response = await agent3.RunAsync(input, session);
            Console.WriteLine(response);

            Console.WriteLine("------------------");
        }
    }
}