[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/rwjdk/AgentFrameworkToolkit/Build.yml?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/actions)
[![GitHub Issues or Pull Requests by label](https://img.shields.io/github/issues/rwjdk/AgentFrameworkToolkit/bug?style=for-the-badge&label=Bugs)](https://github.com/rwjdk/AgentFrameworkToolkit/issues?q=is%3Aissue%20state%3Aopen%20label%3Abug)
[![Libraries.io dependency status for GitHub repo](https://img.shields.io/librariesio/github/rwjdk/AgentFrameworkToolkit?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/network/dependencies)

# Agent Framework Toolkit
### Welcome to Agent Framework Toolkit; An opinionated C# Wrapper for [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) that makes life easier

When using the Microsoft Agent Framework, it is very simple, as long as you do not need to do anything advanced. So setting things like 'model', 'instructions', and 'tools' is a breeze.
But the second you need to do something slightly more advanced, you end up with questions:

- How to work with Anthropic's Claude? 
- How do you set the reasoning effort in OpenAI or Google? 
- How do you add Tool Calling Middleware?
- How do you create Tools from a class or MCP Server?

Things like the above, while doable, are very cumbersome and are not discoverable, as Microsoft Agent Framework have decided to be very generic.

Agent Framework Toolkit resolves these issues by offering Provider-specific wrappers around Microsoft Agent Framwork, that are tailored to the specific provider while keeping 100% compatibility with the rest of Microsoft Agent Framework. The result is less code and much easier code for you to write.

![With and Without Agent Framework Toolkit](https://i.imgur.com/NN18Ets.png)
*The above sample shows how the code looks with and without the Agent Framework Toolkit. You can get more details in [this video](https://youtu.be/OWjy4vkj-8o).*

## Supported Providers
The following providers are currently supported (check out the individual README's for details and samples)

| Provider | Supported Features |   |  |
|---|---|---|--|
| **Antropic (Claude)** | `AgentFactory`, `AIToolsFactory` | [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.Anthropic) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.Anthropic/README.md) |
| **Azure OpenAI** | `AgentFactory`, `AIToolsFactory`, `EmbeddingFactory` | [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.AzureOpenAI) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.AzureOpenAI/README.md) |
| **GitHub Models** | `AgentFactory`, `AIToolsFactory` | [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.GitHub) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.GitHub/README.md) |
| **Google (Gemini)** | `AgentFactory`, `AIToolsFactory` | [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.Google) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.Google/README.md) |
| **Mistral** | `AgentFactory`, `AIToolsFactory`, `EmbeddingFactory`| [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.Mistral) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.Mistral/README.md) |
| **OpenAI** | `AgentFactory`, `AIToolsFactory`, `EmbeddingFactory` | [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.OpenAI) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.OpenAI/README.md) |
| **OpenRouter** | `AgentFactory`, `AIToolsFactory`, `EmbeddingFactory` | [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.OpenRouter) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.OpenRouter/README.md) |
| **XAI (Grok)** | `AgentFactory`, `AIToolsFactory` | [![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/AgentFrameworkToolkit.XAI) | [![README](https://img.shields.io/badge/-README-gray?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/tree/main/src/AgentFrameworkToolkit.XAI/README.md) |

> Tip: For other OpenAI-based providers, you can use the OpenAI Package and provide a custom endpoint

## Code sample (AgentFactory)
> using Azure OpenAI, easily setting Reasoning Effort and Tool Calling Middleware (see individual Provider README for detailed examples)

```cs
AzureOpenAIAgentFactory agentFactory = new("<endpoint>", "<apiKey>");

AzureOpenAIAgent agent = agentFactory.CreateAgent(new AgentOptions
{
    Model = OpenAIChatModels.Gpt5Mini,
    ReasoningEffort = OpenAIReasoningEffort.Low,
    Tools = [AIFunctionFactory.Create(WeatherTool.GetWeather)],
    RawToolCallDetails = details => { Console.WriteLine(details.ToString()); }
});

string question = "What is the Weather like in Paris";
ChatClientAgentRunResponse<WeatherReport> response = await agent.RunAsync<WeatherReport>(question);
WeatherReport weatherReport = response.Result;
```

## Code sample (AIToolsFactory)
```cs
//1. Make your tool-class and add [AITool] attributes

public class MyTools
{
    [AITool]
    public string MyTool1()
    {
        return "hello";
    }

    [AITool]
    public string MyTool2()
    {
        return "world";
    }
}

//2. Get your tool by either instance or Type (if not contructor dependencies)

IList<AITool> tools = aiToolsFactory.GetTools(typeof(MyTools));
//or
IList<AITool> tools = aiToolsFactory.GetTools(new MyTools());
```

**More Info**

[![WIKI](https://img.shields.io/badge/Wiki-brown?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/wiki)
[![Changelog](https://img.shields.io/badge/-Changelog-darkgreen?style=for-the-badge)](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/CHANGELOG.md)
[![YouTube](https://img.shields.io/badge/-YouTube-darkred?style=for-the-badge)](https://www.youtube.com/playlist?list=PLhGl0l5La4sa2XT_K5ndgdStQNchSHe5x)

> **Contributing**: See [CONTRIBUTING.md](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/CONTRIBUTING.md) for build instructions and development guidelines.
