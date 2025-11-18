# AgentFramework Toolkit
An opinionated C# Toolkit for Microsoft Agent Framework that makes life easier

## NuGet
- [AgentFramework Toolkit for OpenAI](https://www.nuget.org/packages/AgentFrameworkToolkit.OpenAI)
- [AgentFramework Toolkit for Azure OpenAI](https://www.nuget.org/packages/AgentFrameworkToolkit.AzureOpenAI)
- [AgentFramework Toolkit for Google (Gemini)](https://www.nuget.org/packages/AgentFrameworkToolkit.Google)
- [AgentFramework Toolkit for Anthropic (Claude)](https://www.nuget.org/packages/AgentFrameworkToolkit.Anthropic)
- [AgentFramework Toolkit for XAI (Grok)](https://www.nuget.org/packages/AgentFrameworkToolkit.XAI)

## Examples of use
> All agents have AIAgent as base, fully work the  rest of Microsoft Agent Framework)

### OpenAI
```cs
OpenAIAgentFactory factory = new OpenAIAgentFactory(new OpenAIConnection
{
    ApiKey = configuration.OpenAiApiKey
});

OpenAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForResponseApiWithReasoning
{
    DeploymentModelName = "gpt-5",
    ReasoningEffort = ResponseReasoningEffortLevel.High,
    ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
    NetworkTimeout = TimeSpan.FromMinutes(5),
});
```

### Google
```cs
GoogleAgentFactory factory = new(new GoogleConnection
{
    ApiKey = configuration.GoogleGeminiApiKey
});

GoogleAgent agent = factory.CreateAgent(new GoogleAgentOptions
{
    DeploymentModelName = GenerativeAI.GoogleAIModels.Gemini25Pro,
    Tools = [AIFunctionFactory.Create(GetWeather)]
});

```

### Anthropic
```cs
AnthropicAgentFactory factory = new AnthropicAgentFactory(new AnthropicConnection
{
    ApiKey = configuration.AnthropicApiKey
});

factory.CreateAgent(new AnthropicAgentOptions
{
    DeploymentModelName = "claude-sonnet-4-5",
    MaxOutputTokens = 10000, // <-- Force the MaxToken property I always forget
    BudgetTokens = 5000
});
```

### X AI
```cs
XAIAgentFactory factory = new(new XAIConnection
{
    ApiKey = configuration.XAiGrokApiKey
});

XAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
{
    DeploymentModelName = "grok-4-fast-non-reasoning",
    Tools = [AIFunctionFactory.Create(GetWeather)]
});
```

### Azure AI (with every optional setting added for demonstration)

```cs
AzureOpenAIAgent fullBlownAgent = azureOpenAIAgentFactory.CreateAgent(new OpenAIAgentOptionsForResponseApiWithReasoning
{
    Id = "1234",
    Name = "MyAgent",
    Description = "The description of my agent",
    Instructions = "Speak like a pirate",
    DeploymentModelName = "gpt-5-mini",
    ReasoningEffort = ResponseReasoningEffortLevel.Low,
    ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
    NetworkTimeout = TimeSpan.FromMinutes(5),
    Tools = [AIFunctionFactory.Create(GetWeather)],
    RawToolCallDetails = details => { Console.WriteLine(details.ToString()); },
    RawHttpCallDetails = details =>
    {
        Console.WriteLine($"URL: {details.RequestUrl}");
        Console.WriteLine($"Request: {details.RequestJson}");
        Console.WriteLine($"Response: {details.ResponseJson}");
    }
});
```

