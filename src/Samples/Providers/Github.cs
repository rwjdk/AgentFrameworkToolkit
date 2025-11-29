using AgentFrameworkToolkit.GitHub;
using Microsoft.Agents.AI;

#pragma warning disable OPENAI001

namespace Samples.Providers;

public static class GitHub
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        GitHubAgentFactory factory = new(configuration.GitHubPatToken);

        GitHubAgent agent = factory.CreateAgent(new GitHubAgentOptions
        {
            Model = "microsoft/Phi-4-mini-instruct",
            RawHttpCallDetails = details => { Console.WriteLine(details.RequestData); }
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}