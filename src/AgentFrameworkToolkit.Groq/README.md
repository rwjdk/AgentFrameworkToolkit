# Agent Framework Toolkit @ Groq

> This package targets Groq as an LLM provider. Check out the [general README](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/README.md) for other providers and shared features in Agent Framework Toolkit.

## What is Agent Framework Toolkit?

Agent Framework Toolkit is an opinionated C# wrapper on top of the [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) that makes advanced agent configuration, tools, middleware, and structured output easier to use.

Groq exposes an OpenAI-compatible API, so this provider uses the shared `AgentFrameworkToolkit.OpenAI.AgentOptions` configuration surface. Both Chat Completions (`ClientType.ChatClient`) and the Responses API (`ClientType.ResponsesApi`) are supported. Chat Completions is the default.

## Getting Started

1. Install the `AgentFrameworkToolkit.Groq` NuGet package: `dotnet add package AgentFrameworkToolkit.Groq`
2. Get a [Groq API key](https://console.groq.com/keys)
3. Create a `GroqAgentFactory`
4. Create and run a `GroqAgent`

### Minimal Example

```cs
GroqAgentFactory agentFactory = new("<API Key>");
GroqAgent agent = agentFactory.CreateAgent(GroqChatModels.GptOss20B);

AgentResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response.Text);
```

### Options Example

```cs
GroqAgentFactory agentFactory = new(new GroqConnection
{
    ApiKey = "<API Key>",
    NetworkTimeout = TimeSpan.FromMinutes(5),
    DefaultClientType = ClientType.ResponsesApi
});

GroqAgent agent = agentFactory.CreateAgent(new AgentOptions
{
    Model = GroqChatModels.GptOss20B,
    ClientType = ClientType.ResponsesApi,
    Instructions = "You are a helpful AI.",
    MaxOutputTokens = 2000,
    Tools = []
});

AgentResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response.Text);
```

The endpoint defaults to `https://api.groq.com/openai/v1` and can be overridden through `GroqConnection.Endpoint`.

## Model Constants

`GroqChatModels` contains the chat-compatible production models, production systems, and preview models published in the [Groq supported-models catalog](https://console.groq.com/docs/models). Audio-only models are not exposed because they cannot be used with `GroqAgentFactory`. Preview models can be discontinued at short notice; consult the Groq catalog before relying on one in production.
