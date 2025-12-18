# AgentFramework Toolkit
An opinionated C# Toolkit for Microsoft Agent Framework that makes life easier

## NuGet
[OpenAI](https://www.nuget.org/packages/AgentFrameworkToolkit.OpenAI) |
[Azure OpenAI](https://www.nuget.org/packages/AgentFrameworkToolkit.AzureOpenAI) |
[Google (Gemini)](https://www.nuget.org/packages/AgentFrameworkToolkit.Google) | 
[Anthropic (Claude)](https://www.nuget.org/packages/AgentFrameworkToolkit.Anthropic) | 
[XAI (Grok)](https://www.nuget.org/packages/AgentFrameworkToolkit.XAI) |
[Mistral](https://www.nuget.org/packages/AgentFrameworkToolkit.Mistral) | 
[OpenRouter](https://www.nuget.org/packages/AgentFrameworkToolkit.OpenRouter) |
[GitHub](https://www.nuget.org/packages/AgentFrameworkToolkit.GitHub)

## Repository layout
- [src/](src/) – Core libraries and tests (packages live under src/*, tests in [src/AgentFrameworkToolkit.Tests](src/AgentFrameworkToolkit.Tests)).
- [examples/Samples](examples/Samples) – Console sample app with per-provider examples.
- [tools/AppHost](tools/AppHost) – Aspire host to orchestrate DevUI and service defaults in dev.
- [tools/DevUI](tools/DevUI) – Web UI to try all agents with your own keys.
- [tools/ServiceDefaults](tools/ServiceDefaults) – Shared dev infra (health checks, telemetry defaults).

## Examples of use
> All agents have AIAgent as base, fully work the  rest of Microsoft Agent Framework)

### OpenAI
```cs
OpenAIAgentFactory factory = new OpenAIAgentFactory(new OpenAIConnection
{
    ApiKey = configuration.OpenAiApiKey,
    NetworkTimeout = TimeSpan.FromMinutes(5)
});

OpenAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForResponseApiWithReasoning
{
    Model = "gpt-5",
    ReasoningEffort = ResponseReasoningEffortLevel.High,
    ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,    
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
    Model = GenerativeAI.GoogleAIModels.Gemini25Pro,
    Tools = [AIFunctionFactory.Create(GetWeather)]
});

```

### Anthropic
```cs
AnthropicAgentFactory factory = new AnthropicAgentFactory("<AnthropicApiKey">);

factory.CreateAgent(new AnthropicAgentOptions
{
    Model = "claude-sonnet-4-5",
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
    Model = "grok-4-fast-non-reasoning",
    Tools = [AIFunctionFactory.Create(GetWeather)]
});
```

### Mistral
```cs
MistralAgentFactory mistralAgentFactory = new MistralAgentFactory("<MistralApiKey>");
MistralAgent mistralAgent = mistralAgentFactory.CreateAgent(new MistralAgentOptions
{
    Model = Mistral.SDK.ModelDefinitions.MistralSmall
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
    Model = "gpt-5-mini",
    ReasoningEffort = ResponseReasoningEffortLevel.Low,
    ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
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

