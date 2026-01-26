using AgentFrameworkToolkit;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.Common;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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

        AIToolsFactory toolsFactory = new();

        List<AITool> tools = [];
        tools.AddRange(toolsFactory.GetHttpClientTools());
        tools.AddRange(toolsFactory.GetTimeTools());
        tools.AddRange(toolsFactory.GetWeatherTools(new OpenWeatherMapOptions
        {
            ApiKey = secrets.OpenWeatherApiKey
        }));
        tools.AddRange(toolsFactory.GetFileSystemTools(new GetFileSystemToolsOptions
        {
            FileSystemToolsOptions = new FileSystemToolsOptions
            {
                ConfinedToTheseFolderPaths = ["C:\\TestAI"]
            }
        }));
        tools.AddRange(toolsFactory.GetWebsiteTools());
        
        AIAgent agent = factory.CreateAgent(new AgentOptions
        {
            Instructions = $"Using Trello API Key: '{secrets.TrelloApiKey}' and Token '{secrets.TrelloToken}' - your file operating folder it C:\\TestAI",
            Model = OpenAIChatModels.Gpt41Nano,
            Tools = tools,
            RawToolCallDetails = Console.WriteLine
        });

        AgentThread thread = await agent.GetNewThreadAsync();
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            if (input == "/new")
            {
                Console.Clear();
                thread = await agent.GetNewThreadAsync();
                continue;
            }
            AgentResponse response = await agent.RunAsync(input, thread);
            Console.WriteLine(response);
            Console.WriteLine($"(Input: {response.Usage!.InputTokenCount} - Output: {response.Usage.OutputTokenCount})");


            Console.WriteLine();
            Console.WriteLine("---");
            Console.WriteLine();
        }
    }

    static string MyTool()
    {
        return "1234";
    }

    private class MyToolsInType
    {
        [AITool("my_type_tool1")]
        public string MyTypeTool1()
        {
            return "42";
        }

        [AITool("my_type_tool2")]
        public string MyTypeTool2()
        {
            return "999";
        }
    }

    private class MyToolsInInstance
    {
        [AITool("my_instance_tool1")]
        public string MyInstanceTool1()
        {
            return "42";
        }

        [AITool("my_instance_tool2")]
        public string MyInstanceTool2()
        {
            return "999";
        }
    }
}
