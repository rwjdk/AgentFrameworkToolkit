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

        new TimeTools().Get(TimeTool.All);

        //Create your AgentFactory (using a connection object for more options)
        AzureOpenAIAgentFactory factory = new AzureOpenAIAgentFactory(new AzureOpenAIConnection
        {
            Endpoint = secrets.AzureOpenAiEndpoint,
            ApiKey = secrets.AzureOpenAiKey,
        });


        var toolsFactory = new AIToolsFactory();
        IList<AITool> allTheTools = toolsFactory.GetToolsFromMultipleSources(
            toolsWithAttributeFromTypes:
            [
                typeof(MyToolsInType), //add more
            ],
            toolsWithAttributeFromObjectInstances:
            [
                new MyToolsInInstance(), //add more
            ],
            toolsFromAgentSkillFolders:
            [
                new AgentSkillFolder("TestData\\AgentSkills"), //add more
            ],
            timeTools: TimeTool.All,
            otherTools:
            [
                //Advanced Common Tools
                new WeatherTools(WeatherOptions.OpenWeatherMap(secrets.OpenWeatherApiKey)).GetWeatherForCity(),
                //Hosted Tools
                new HostedCodeInterpreterTool(),
                new HostedWebSearchTool(),

                //Your own tools
                AIFunctionFactory.Create(MyTool)
            ]
        );


        WeatherOptions weatherOptions = WeatherOptions.OpenWeatherMap(secrets.OpenWeatherApiKey, WeatherOptionsUnits.Metric);
        AIAgent agent = factory.CreateAgent(new AgentOptions
        {
            Model = OpenAIChatModels.Gpt41Nano,
            Tools = allTheTools,
            RawToolCallDetails = Console.WriteLine
        });

        AgentRunResponse response2 = await agent.RunAsync("What is the weather like in Paris?");
        Console.WriteLine(response2);
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
