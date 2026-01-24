# Unit Testing Guide for Agent Providers

This guide covers comprehensive unit testing for Agent Provider packages in AgentFrameworkToolkit.

## Testing Framework

**xUnit v3** is used throughout the codebase:
- `xunit.v3`
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`

## Test File Location

All provider tests are located in: `development/Tests/<Provider>Tests.cs`

Examples:
- `development/Tests/AnthropicTests.cs`
- `development/Tests/OpenAITests.cs`
- `development/Tests/GoogleTests.cs`

## Standard Test Class Structure

Every provider test class must inherit from `TestsBase`:

```csharp
public sealed class <Provider>Tests : TestsBase
{
    // Required test methods below
}
```

## Required Test Methods (7 Tests)

### 1. Simple Agent Test
Tests basic agent creation and conversation:

```csharp
[Fact]
public Task AgentFactory_Simple()
    => SimpleAgentTestsAsync(AgentProvider.<Provider>);
```

**What it validates:**
- Agent has valid ID
- Agent has correct name
- Agent responds to "Hello"
- Response contains non-empty text

### 2. Normal Agent Test
Tests agent with logging and full configuration:

```csharp
[Fact]
public Task AgentFactory_Normal()
    => NormalAgentTestsAsync(AgentProvider.<Provider>);
```

**What it validates:**
- Agent has ID, name, and description
- Logging middleware captures agent ID
- Response is valid

### 3. OpenTelemetry & Logging Middleware Test
Tests middleware integration:

```csharp
[Fact]
public Task AgentFactory_OpenTelemetryAndLoggingMiddleware()
    => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.<Provider>);
```

**What it validates:**
- OpenTelemetry traces are created
- Logging middleware works correctly
- Middleware doesn't break agent functionality

### 4. Tool Call Test
Tests function/tool calling capabilities:

```csharp
[Fact]
public Task AgentFactory_ToolCall()
    => ToolCallAgentTestsAsync(AgentProvider.<Provider>);
```

**What it validates:**
- Agent can call the weather tool
- Tool receives correct parameters (city="Paris")
- Response includes tool call results
- Tool calling middleware intercepts calls

### 5. MCP Tool Call Test
Tests Model Context Protocol tool integration:

```csharp
[Fact]
public Task AgentFactory_McpToolCall()
    => McpToolCallAgentTestsAsync(AgentProvider.<Provider>);
```

**What it validates:**
- Agent can use MCP tools from remote server
- MCP tools execute correctly

### 6. Dependency Injection (API Key) Test
Tests DI with string API key:

```csharp
[Fact]
public async Task AgentFactory_DependencyInjection()
{
    var secrets = SecretsManager.GetSecrets();
    ServiceCollection services = new();
    services.Add<Provider>AgentFactory(secrets.<Provider>ApiKey);

    ServiceProvider provider = services.BuildServiceProvider();
    var cancellationToken = TestContext.Current.CancellationToken;

    string text = (await provider.GetRequiredService<<Provider>AgentFactory>()
        .CreateAgent(<Provider>ChatModels.DefaultModel, 2000)
        .RunAsync("Hello", cancellationToken: cancellationToken)).Text;

    Assert.NotEmpty(text);
}
```

**What it validates:**
- Service collection registration works
- Factory can be resolved from DI container
- Agent can be created and used

### 7. Dependency Injection (Connection) Test
Tests DI with Connection object:

```csharp
[Fact]
public async Task AgentFactory_DependencyInjection_Connection()
{
    var secrets = SecretsManager.GetSecrets();
    ServiceCollection services = new();
    services.Add<Provider>AgentFactory(new <Provider>Connection
    {
        ApiKey = secrets.<Provider>ApiKey,
        NetworkTimeout = TimeSpan.FromSeconds(10)
    });

    ServiceProvider provider = services.BuildServiceProvider();
    var cancellationToken = TestContext.Current.CancellationToken;

    string text = (await provider.GetRequiredService<<Provider>AgentFactory>()
        .CreateAgent(<Provider>ChatModels.DefaultModel, 2000)
        .RunAsync("Hello", cancellationToken: cancellationToken)).Text;

    Assert.NotEmpty(text);
}
```

**What it validates:**
- Connection object registration works
- Connection configuration is respected
- Factory can use injected connection

## Required Override: GetAgentAsync

You **must** override `GetAgentAsync` in your test class to create provider-specific agents:

```csharp
protected override async Task<AIAgent> GetAgentAsync(
    AgentProvider provider,
    string model,
    string? instructions,
    string? name,
    string? description,
    IList<AITool> tools,
    ILoggerFactory? loggerFactory,
    string? endpoint,
    Action<RawHttpCallDetails>? rawHttpCallDetails,
    AIToolCallingMiddleware? toolCallingMiddleware,
    LoggingMiddleware? loggingMiddleware,
    OpenTelemetryMiddleware? openTelemetryMiddleware)
{
    var secrets = SecretsManager.GetSecrets();

    switch (provider)
    {
        case AgentProvider.<Provider>:
            return new <Provider>AgentFactory(new <Provider>Connection
            {
                ApiKey = secrets.<Provider>ApiKey,
                NetworkTimeout = TimeSpan.FromSeconds(10),
                HttpHandler = rawHttpCallDetails != null
                    ? new RawHttpCallDetailsHandler(rawHttpCallDetails)
                    : null
            }).CreateAgent(new <Provider>AgentOptions
            {
                Model = model,
                MaxOutputTokens = 2000,
                BudgetTokens = 200000,
                Instructions = instructions,
                Name = name,
                Description = description,
                Tools = tools,
                LoggerFactory = loggerFactory,
                ToolCallingMiddleware = toolCallingMiddleware,
                LoggingMiddleware = loggingMiddleware,
                OpenTelemetryMiddleware = openTelemetryMiddleware
            });

        default:
            return await base.GetAgentAsync(provider, model, instructions, name,
                description, tools, loggerFactory, endpoint, rawHttpCallDetails,
                toolCallingMiddleware, loggingMiddleware, openTelemetryMiddleware);
    }
}
```

### OpenAI-Compatible Provider Override Example

```csharp
protected override async Task<AIAgent> GetAgentAsync(...)
{
    var secrets = SecretsManager.GetSecrets();

    switch (provider)
    {
        case AgentProvider.<Provider>:
            return new <Provider>AgentFactory(new <Provider>Connection
            {
                ApiKey = secrets.<Provider>ApiKey,
                Endpoint = endpoint,
                NetworkTimeout = TimeSpan.FromSeconds(10),
                HttpHandler = rawHttpCallDetails != null
                    ? new RawHttpCallDetailsHandler(rawHttpCallDetails)
                    : null
            }).CreateAgent(new AgentOptions
            {
                Model = model,
                MaxOutputTokens = 2000,
                Instructions = instructions,
                Name = name,
                Description = description,
                Tools = tools,
                LoggerFactory = loggerFactory,
                ToolCallingMiddleware = toolCallingMiddleware,
                LoggingMiddleware = loggingMiddleware,
                OpenTelemetryMiddleware = openTelemetryMiddleware
            });

        default:
            return await base.GetAgentAsync(...);
    }
}
```

## Adding Provider to AgentProvider Enum

In `development/Tests/TestBase.cs`, add your provider to the enum (around line 635):

```csharp
public enum AgentProvider
{
    AzureOpenAIChatClient,
    AzureOpenAIResponsesApi,
    OpenAIChatClient,
    OpenAIResponsesApi,
    Anthropic,
    GitHub,
    Google,
    Mistral,
    OpenRouterChatClient,
    OpenRouterResponsesApi,
    CohereChatClient,
    XAIChatClient,
    XAIResponsesApi,
    <Provider>,  // Add your provider here
}
```

## API Keys and Secrets Management

### 1. Add Property to Secrets Class

In `development/Tests/Secrets/Secrets.cs`:

```csharp
public sealed class Secrets
{
    // ... existing properties ...

    /// <summary>
    /// API key for <Provider> integration.
    /// </summary>
    public string <Provider>ApiKey { get; set; } = "";
}
```

### 2. Add API Key to secrets.json

In `development/Tests/Secrets/secrets.json`:

```json
{
  "AnthropicApiKey": "...",
  "OpenAiApiKey": "...",
  "<Provider>ApiKey": "your-api-key-here"
}
```

**Important:** Never commit actual API keys to version control!

## Test Utilities from TestsBase

### TestLogger
Captures log messages for assertion:

```csharp
TestLoggerFactory testLogger = new();
AIAgent agent = await GetAgentForScenarioAsync(provider, AgentScenario.Normal, testLogger);

// Assert agent ID appears in logs
bool condition = testLogger.Logger.Messages.Any(x => x.Contains(agent.Id));
Assert.True(condition);
```

### RawHttpCallDetailsHandler
Inspects raw HTTP requests:

```csharp
Action<RawHttpCallDetails> rawHttpCallDetails = details =>
{
    Assert.Contains(model, details.RequestData);
    Assert.Contains(instructions, details.RequestData);
    Assert.Contains($"\"model\": \"{model}\"", details.RequestData);
};
```

## Test Scenarios Explained

The `AgentScenario` enum defines test scenarios:

```csharp
public enum AgentScenario
{
    Simple,                              // Minimal configuration
    Normal,                              // Full configuration with logging
    ToolCall,                            // Tool calling via weather function
    McpToolCall,                         // Model Context Protocol tools
    OpenTelemetryAndLoggingMiddleware,   // Middleware integration
}
```

## Complete Example: XAITests.cs

```csharp
public sealed class XAITests : TestsBase
{
    [Fact]
    public Task AgentFactory_Simple_ChatClient()
        => SimpleAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_Normal_ChatClient()
        => NormalAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_OpenTelemetryAndLoggingMiddleware_ChatClient()
        => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_ToolCall_ChatClient()
        => ToolCallAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public Task AgentFactory_McpToolCall_ChatClient()
        => McpToolCallAgentTestsAsync(AgentProvider.XAIChatClient);

    [Fact]
    public async Task AgentFactory_DependencyInjection()
    {
        var secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddXAIAgentFactory(secrets.XaiApiKey);

        ServiceProvider provider = services.BuildServiceProvider();
        var cancellationToken = TestContext.Current.CancellationToken;

        string text = (await provider.GetRequiredService<XAIAgentFactory>()
            .CreateAgent(XAIChatModels.GrokBeta)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;

        Assert.NotEmpty(text);
    }

    [Fact]
    public async Task AgentFactory_DependencyInjection_Connection()
    {
        var secrets = SecretsManager.GetSecrets();
        ServiceCollection services = new();
        services.AddXAIAgentFactory(new XAIConnection
        {
            ApiKey = secrets.XaiApiKey,
            NetworkTimeout = TimeSpan.FromSeconds(10)
        });

        ServiceProvider provider = services.BuildServiceProvider();
        var cancellationToken = TestContext.Current.CancellationToken;

        string text = (await provider.GetRequiredService<XAIAgentFactory>()
            .CreateAgent(XAIChatModels.GrokBeta)
            .RunAsync("Hello", cancellationToken: cancellationToken)).Text;

        Assert.NotEmpty(text);
    }

    protected override async Task<AIAgent> GetAgentAsync(
        AgentProvider provider,
        string model,
        string? instructions,
        string? name,
        string? description,
        IList<AITool> tools,
        ILoggerFactory? loggerFactory,
        string? endpoint,
        Action<RawHttpCallDetails>? rawHttpCallDetails,
        AIToolCallingMiddleware? toolCallingMiddleware,
        LoggingMiddleware? loggingMiddleware,
        OpenTelemetryMiddleware? openTelemetryMiddleware)
    {
        var secrets = SecretsManager.GetSecrets();

        switch (provider)
        {
            case AgentProvider.XAIChatClient:
                return new XAIAgentFactory(new XAIConnection
                {
                    ApiKey = secrets.XaiApiKey,
                    Endpoint = endpoint,
                    NetworkTimeout = TimeSpan.FromSeconds(10),
                    HttpHandler = rawHttpCallDetails != null
                        ? new RawHttpCallDetailsHandler(rawHttpCallDetails)
                        : null
                }).CreateAgent(new AgentOptions
                {
                    Model = model,
                    MaxOutputTokens = 2000,
                    Instructions = instructions,
                    Name = name,
                    Description = description,
                    Tools = tools,
                    LoggerFactory = loggerFactory,
                    ToolCallingMiddleware = toolCallingMiddleware,
                    LoggingMiddleware = loggingMiddleware,
                    OpenTelemetryMiddleware = openTelemetryMiddleware
                });

            default:
                return await base.GetAgentAsync(provider, model, instructions, name,
                    description, tools, loggerFactory, endpoint, rawHttpCallDetails,
                    toolCallingMiddleware, loggingMiddleware, openTelemetryMiddleware);
        }
    }
}
```

## Running Tests

```bash
# Run all tests
dotnet test --configuration Release

# Run specific provider tests
dotnet test --configuration Release --filter "FullyQualifiedName~<Provider>Tests"

# Run with verbose output
dotnet test --configuration Release --logger "console;verbosity=detailed"
```

## Integration Testing Note

**Important:** These are **integration tests** that make real API calls:
- They require valid API keys from the provider
- They test end-to-end functionality
- They verify actual model responses
- They may incur costs on the provider's platform

This approach ensures:
- Real-world compatibility with provider APIs
- Middleware pipeline works correctly
- Tool calling functions as expected
- DI registration is configured properly

## Optional: Additional Tests

Consider adding provider-specific tests for:

### Embedding Support (if applicable)
```csharp
[Fact]
public async Task EmbeddingFactory()
{
    Secrets.Secrets secrets = SecretsManager.GetSecrets();
    <Provider>EmbeddingFactory factory = new(secrets.<Provider>ApiKey);
    IEmbeddingGenerator<string, Embedding<float>> generator =
        factory.GetEmbeddingGenerator("<embedding-model-id>");

    Embedding<float> embedding = await generator.GenerateAsync(
        "Hello",
        cancellationToken: TestContext.Current.CancellationToken);

    Assert.Equal(expectedDimensions, embedding.Dimensions);
}
```

### Streaming Support
```csharp
[Fact]
public async Task StreamingResponse()
{
    var secrets = SecretsManager.GetSecrets();
    var factory = new <Provider>AgentFactory(secrets.<Provider>ApiKey);
    var agent = factory.CreateAgent(<Provider>ChatModels.DefaultModel);

    await foreach (var chunk in agent.RunStreamingAsync("Hello"))
    {
        Assert.NotNull(chunk);
    }
}
```

## Common Testing Issues

### Provider-Specific Caveats

Some providers may have limitations in CI/CD environments. Example from GitHubTests.cs:

```csharp
[Fact]
public void AgentFactory_DependencyInjection()
{
    // RunAsync often hangs with GitHub Models in CI, so we only verify resolution
    GitHubAgentFactory factory = provider.GetRequiredService<GitHubAgentFactory>();
    Assert.NotNull(factory);
}
```

If your provider has similar issues, document them and adjust tests accordingly.

## Reference Files

See these files for complete examples:
- `development/Tests/AnthropicTests.cs` - Custom provider example
- `development/Tests/OpenAITests.cs` - Full-featured provider with multiple variants
- `development/Tests/XAITests.cs` - OpenAI-compatible provider example
- `development/Tests/TestBase.cs` - Base class with test scenarios
- `development/Tests/Secrets/Secrets.cs` - Secrets management

## Summary Checklist

- [ ] Created `development/Tests/<Provider>Tests.cs`
- [ ] Inherited from `TestsBase`
- [ ] Implemented all 7 required test methods
- [ ] Added provider to `AgentProvider` enum
- [ ] Overrode `GetAgentAsync` method
- [ ] Added API key property to `Secrets.cs`
- [ ] Added API key to `secrets.json` (local only, not committed)
- [ ] Tests pass: `dotnet test --configuration Release`
