using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.ModelContextProtocol;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Samples.Providers;

public static class Google
{
    static string GetWeather()
    {
        return "{ \"condition\": \"sunny\", \"degrees\":19 }";
    }

    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        GoogleAgentFactory factory = new(new GoogleConnection
        {
            ApiKey = secrets.GoogleGeminiApiKey
        });

        McpClientTools mcpClientTools = await new AIToolsFactory().GetToolsFromRemoteMcpAsync("https://trellodotnetassistantbackend.azurewebsites.net/runtime/webhooks/mcp?code=Tools");
        AIFunction aiFunction = AIFunctionFactory.Create(GetWeather, "getting_started", description: "General information on how to get started with TrelloDotNet. (Always start by calling this before any other tools)");
        IList<AITool> aiTools = mcpClientTools.Tools;
        GoogleAgent agent = factory.CreateAgent(new GoogleAgentOptions
        {
            Model = GoogleChatModels.Gemini25Flash,
            Tools = [aiTools[1]]
        });

        AgentRunResponse response = await agent.RunAsync("Call the 'getting_started' tool to find what URL the nuget is on");
        Console.WriteLine(response);
    }
}
