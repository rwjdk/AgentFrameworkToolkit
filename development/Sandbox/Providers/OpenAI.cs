using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.Tools;
using AgentSkillsDotNet;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

#pragma warning disable OPENAI001

namespace Sandbox.Providers;

public static class OpenAI
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        OpenAIAgentFactory factory = new(new OpenAIConnection
        {
            ApiKey = secrets.OpenAiApiKey,
            DefaultClientType = ClientType.ResponsesApi
        });

        var agentSkillsFactory = new AgentSkillsFactory();
        AgentSkills agentSkills = agentSkillsFactory.GetAgentSkills("TestData\\AgentSkills");
        IList<AITool> tools = agentSkills.GetAsTools(AgentSkillsAsToolsStrategy.AvailableSkillsAndLookupTools, new AgentSkillsAsToolsOptions
        {
            IncludeToolForFileContentRead = false
        });

        tools.Add(AIFunctionFactory.Create(PythonRunner.RunPhytonScript, name: "execute_python"));

        OpenAIAgent agent = factory.CreateAgent(new AgentOptions
        {
            ClientType = ClientType.ResponsesApi,
            Instructions = agentSkills.GetInstructions(),
            Model = OpenAIChatModels.Gpt5Nano,
            Tools = tools,
            RawToolCallDetails = details =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(details.ToString());
                Console.ResetColor();
            }
        });

        AgentResponse response = await agent.RunAsync("What is the answer to the extra secret formula (Only return the result)");
        Console.WriteLine(response);
    }
}
