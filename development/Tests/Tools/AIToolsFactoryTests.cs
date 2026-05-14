using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.Common;
using AgentFrameworkToolkit.Tools.ModelContextProtocol;
using AgentSkillsDotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.Tests.Tools;

public class AIToolsFactoryTests
{
    [Fact]
    public void GetToolsFromInstance()
    {
        AIToolsFactory factory = new();
        IList<AITool> tools = factory.GetTools(new TestToolsWithAttributes());

        Assert.Equal(4, tools.Count);
        Assert.Equal("tool_1", tools[0].Name);
        Assert.Equal("This is tool1", tools[0].Description);
        Assert.Equal("tool_2", tools[1].Name);
        Assert.Equal("This is tool2", tools[1].Description);
        Assert.Equal("tool_3", tools[2].Name);
        Assert.Equal("This is tool3", tools[2].Description);
        Assert.Equal("tool_4", tools[3].Name);
        Assert.Equal("This is tool4", tools[3].Description);
    }

    [Fact]
    public void GetToolsFromType()
    {
        AIToolsFactory factory = new();
        IList<AITool> tools = factory.GetTools(typeof(TestToolsWithAttributes));

        Assert.Equal(4, tools.Count);
        Assert.Equal("tool_1", tools[0].Name);
        Assert.Equal("This is tool1", tools[0].Description);
        Assert.Equal("tool_2", tools[1].Name);
        Assert.Equal("This is tool2", tools[1].Description);
        Assert.Equal("tool_3", tools[2].Name);
        Assert.Equal("This is tool3", tools[2].Description);
        Assert.Equal("tool_4", tools[3].Name);
        Assert.Equal("This is tool4", tools[3].Description);
    }

    [Fact]
    public async Task GetRemoteMcpToolsAsync()
    {
        AIToolsFactory factory = new();
        await using McpClientTools clientTools = await factory.GetToolsFromRemoteMcpAsync("https://trellodotnetassistantbackend.azurewebsites.net/runtime/webhooks/mcp?code=Tools");

        IList<AITool> tools = clientTools.Tools;
        Assert.Equal(2, tools.Count);
        Assert.Equal("TrelloDotNetCodeAssistant", tools[0].Name);
        Assert.Equal("Call this Tool in order to get information on how NuGet Package TrelloDotNet is used, how to do various actions in the API", tools[0].Description);
        Assert.Equal("getting_started", tools[1].Name);
        Assert.Equal("General information on how to get started with TrelloDotNet. (Always start by calling this before any other tools)", tools[1].Description);
    }

    [Fact]
    public async Task GetLocalMcpToolsAsync()
    {
        AIToolsFactory factory = new();
        await using McpClientTools clientTools = await factory.GetToolsFromLocalMcpAsync("npx", ["@playwright/mcp@latest"]);
        Assert.True(clientTools.Tools.Count > 0);
        Assert.Contains("browser_click", clientTools.Tools.Select(x => x.Name));
    }

    [Fact]
    public void InjectAIToolFactoryFactoryTest()
    {
        ServiceCollection services = new();
        services.AddAIToolFactory();

        ServiceProvider provider = services.BuildServiceProvider();

        AIToolsFactory aiToolsFactory = provider.GetRequiredService<AIToolsFactory>();
        IList<AITool> tools = aiToolsFactory.GetTools(typeof(DiTools));
        Assert.Single(tools);
    }

    [Fact]
    public void GetRandomTools_Default_ReturnsIntegerAndDecimalTools()
    {
        AIToolsFactory factory = new();
        IList<AITool> tools = factory.GetRandomTools();

        Assert.Equal(2, tools.Count);
        Assert.Equal("get_random_integer", tools[0].Name);
        Assert.Equal("get_random_double", tools[1].Name);
    }

    [Fact]
    public void GetRandomTools_CanDisableDecimalTool()
    {
        AIToolsFactory factory = new();
        IList<AITool> tools = factory.GetRandomTools(new GetRandomToolsOptions
        {
            GetRandomInteger = true,
            GetRandomDouble = false
        });

        Assert.Single(tools);
        Assert.Equal("get_random_integer", tools[0].Name);
    }

    [Fact]
    public void GetRandomTools_CanOverrideToolName()
    {
        AIToolsFactory factory = new();
        IList<AITool> tools = factory.GetRandomTools(new GetRandomToolsOptions
        {
            GetRandomIntegerOptions = new GetRandomIntegerOptions
            {
                DefaultMin = 10,
                DefaultMax = 20
            },
            GetRandomIntegerToolName = "random_between_10_and_20",
            GetRandomDouble = false
        });

        Assert.Single(tools);
        Assert.Equal("random_between_10_and_20", tools[0].Name);
    }

    [PublicAPI]
    private class TestToolsWithAttributes
    {
        [AITool("tool_1", "This is tool1")]
        public string Tool1(string input1)
        {
            return input1;
        }

        [AITool("tool_2", "This is tool2")]
        private string Tool2(string input1)
        {
            return input1;
        }

        [AITool("tool_3", "This is tool3")]
        public static string Tool3(string input1)
        {
            return input1;
        }

        [AITool("tool_4", "This is tool4")]
        private static string Tool4(string input1)
        {
            return input1;
        }

        private string NotATool(string input1)
        {
            return input1;
        }
    }

    private class DiTools
    {
        [AITool]
        [UsedImplicitly]
        public static string GetWeather(string city)
        {
            return "{ \"condition\": \"sunny\", \"degrees\":19 }";
        }
    }
}
