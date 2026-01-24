# Custom Provider Template

Use this template when the LLM provider has a unique API that is NOT OpenAI-compatible. This approach requires implementing all components but provides full control over the integration.

## Examples in Codebase

- `src/AgentFrameworkToolkit.Anthropic/` - Claude models
- `src/AgentFrameworkToolkit.GitHub/` - GitHub Models
- `src/AgentFrameworkToolkit.Google/` - Google Gemini
- `src/AgentFrameworkToolkit.Mistral/` - Mistral AI

## Step-by-Step Implementation

### 1. Create Project Structure

```
src/AgentFrameworkToolkit.<Provider>/
├── <Provider>Connection.cs
├── <Provider>AgentFactory.cs
├── <Provider>Agent.cs
├── <Provider>AgentOptions.cs
├── <Provider>ChatModels.cs
├── ServiceCollectionExtensions.cs
├── README.md
└── AgentFrameworkToolkit.<Provider>.csproj
```

### 2. Create .csproj File

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>true</IsPackable>
    <RootNamespace>AgentFrameworkToolkit.<Provider></RootNamespace>
    <Description>Agent provider for <Provider> using AgentFrameworkToolkit</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\AgentFrameworkToolkit\AgentFrameworkToolkit.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Add the provider's official SDK package reference -->
    <!-- Version should be specified in Directory.Packages.props -->
    <PackageReference Include="<Provider>.SDK" />
  </ItemGroup>

</Project>
```

### 3. Implement Connection Class

```csharp
using JetBrains.Annotations;
using <Provider>.SDK; // Replace with actual SDK namespace

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Connection configuration for <Provider> API.
/// </summary>
[PublicAPI]
public class <Provider>Connection
{
    /// <summary>
    /// Gets or sets the API key for <Provider>.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the custom API endpoint (optional).
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the network timeout (optional).
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Creates a configured <Provider> client.
    /// </summary>
    /// <param name="rawHttpCallDetails">
    /// Optional callback for inspecting raw HTTP requests/responses.
    /// </param>
    /// <returns>A configured <Provider> SDK client.</returns>
    public <Provider>Client GetClient(
        Action<RawCallDetails>? rawHttpCallDetails = null)
    {
        // Create HTTP client with optional timeout and interceptor
        var httpClient = new HttpClient();

        if (NetworkTimeout.HasValue)
        {
            httpClient.Timeout = NetworkTimeout.Value;
        }

        if (rawHttpCallDetails != null)
        {
            // Add HTTP message handler for inspection
            // Implementation depends on SDK capabilities
        }

        // Configure and return SDK client
        var client = new <Provider>Client(ApiKey, httpClient);

        if (!string.IsNullOrEmpty(Endpoint))
        {
            client.BaseUrl = Endpoint;
        }

        return client;
    }
}
```

### 4. Implement Agent Options

```csharp
using AgentFrameworkToolkit.Middleware;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Configuration options for <Provider> agents.
/// </summary>
[PublicAPI]
public class <Provider>AgentOptions
{
    /// <summary>
    /// Gets or sets the model identifier (required).
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Gets or sets the maximum output tokens (required).
    /// </summary>
    public required int MaxOutputTokens { get; set; }

    /// <summary>
    /// Gets or sets the agent name (optional).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the system instructions (optional).
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Gets or sets the available tools (optional).
    /// </summary>
    public IList<AITool>? Tools { get; set; }

    /// <summary>
    /// Gets or sets the temperature for response randomness (optional, 0-1).
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the top-p sampling parameter (optional, 0-1).
    /// </summary>
    public float? TopP { get; set; }

    /// <summary>
    /// Gets or sets provider-specific option (example: budget tokens).
    /// Add provider-specific properties here.
    /// </summary>
    public int? BudgetTokens { get; set; }

    // Middleware and Infrastructure

    /// <summary>
    /// Gets or sets the service provider for dependency injection (optional).
    /// </summary>
    public IServiceProvider? Services { get; set; }

    /// <summary>
    /// Gets or sets the logger factory (optional).
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    /// Gets or sets the logging middleware configuration (optional).
    /// </summary>
    public LoggingMiddleware? LoggingMiddleware { get; set; }

    /// <summary>
    /// Gets or sets the OpenTelemetry middleware configuration (optional).
    /// </summary>
    public OpenTelemetryMiddleware? OpenTelemetryMiddleware { get; set; }

    /// <summary>
    /// Gets or sets the tool-calling middleware delegate (optional).
    /// </summary>
    public ToolCallingMiddlewareDelegate? ToolCallingMiddleware { get; set; }

    /// <summary>
    /// Gets or sets the raw tool call details callback (optional).
    /// </summary>
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }

    /// <summary>
    /// Gets or sets the raw HTTP call details callback (optional).
    /// </summary>
    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }

    /// <summary>
    /// Gets or sets additional configuration for ChatClientAgent (optional).
    /// </summary>
    public Action<ChatClientAgentOptions>? AdditionalChatClientAgentOptions { get; set; }
}
```

### 5. Implement Agent Factory

```csharp
using AgentFrameworkToolkit.Middleware;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Factory for creating <Provider> agents.
/// </summary>
[PublicAPI]
public class <Provider>AgentFactory
{
    /// <summary>
    /// Gets the <Provider> connection configuration.
    /// </summary>
    public <Provider>Connection Connection { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="<Provider>AgentFactory"/> class
    /// with an API key.
    /// </summary>
    /// <param name="apiKey">The <Provider> API key.</param>
    public <Provider>AgentFactory(string apiKey)
        : this(new <Provider>Connection { ApiKey = apiKey })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="<Provider>AgentFactory"/> class
    /// with a connection configuration.
    /// </summary>
    /// <param name="connection">The <Provider> connection configuration.</param>
    public <Provider>AgentFactory(<Provider>Connection connection)
    {
        Connection = connection;
    }

    /// <summary>
    /// Creates a new <Provider> agent with the specified options.
    /// </summary>
    /// <param name="options">The agent configuration options.</param>
    /// <returns>A configured <see cref="<Provider>Agent"/>.</returns>
    public <Provider>Agent CreateAgent(<Provider>AgentOptions options)
    {
        // Step 1: Get SDK client
        var client = Connection.GetClient(options.RawHttpCallDetails);

        // Step 2: Convert SDK client to IChatClient
        // This depends on the SDK - some provide IChatClient directly,
        // others need an adapter
        IChatClient chatClient = CreateChatClient(client, options);

        // Step 3: Create ChatClientAgentOptions
        var chatClientAgentOptions = new ChatClientAgentOptions
        {
            Name = options.Name,
            Instructions = options.Instructions,
            Tools = options.Tools,
            Services = options.Services,
            LoggerFactory = options.LoggerFactory
        };

        // Apply provider-specific additional options
        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        // Step 4: Create base ChatClientAgent
        var chatClientAgent = new ChatClientAgent(chatClient, chatClientAgentOptions);

        // Step 5: Apply middleware pipeline
        AIAgent agent = ApplyMiddleware(chatClientAgent, options);

        // Step 6: Wrap in provider-specific agent
        return new <Provider>Agent(agent);
    }

    /// <summary>
    /// Creates an IChatClient from the SDK client.
    /// </summary>
    private IChatClient CreateChatClient(
        <Provider>Client client,
        <Provider>AgentOptions options)
    {
        // Implementation depends on the provider SDK
        // Some SDKs provide IChatClient directly:
        // return client.AsChatClient(options.Model);

        // Others may need a wrapper or adapter
        // Check SDK documentation or existing provider implementations

        throw new NotImplementedException(
            "Implement SDK client to IChatClient conversion");
    }

    /// <summary>
    /// Applies middleware pipeline to the agent.
    /// CRITICAL: Middleware must be applied in this exact order.
    /// </summary>
    private static AIAgent ApplyMiddleware(
        AIAgent agent,
        <Provider>AgentOptions options)
    {
        // Order: Logging → OpenTelemetry → ToolCalling → RawToolDetails
        // (outermost to innermost)

        // 1. Raw tool details (innermost)
        if (options.RawToolCallDetails != null)
        {
            agent = agent.AsBuilder()
                .UseToolCallingRawLogging(options.RawToolCallDetails)
                .Build();
        }

        // 2. Tool-calling middleware
        if (options.ToolCallingMiddleware != null)
        {
            agent = agent.AsBuilder()
                .UseToolCallingMiddleware(options.ToolCallingMiddleware)
                .Build();
        }

        // 3. OpenTelemetry middleware
        if (options.OpenTelemetryMiddleware != null)
        {
            agent = agent.AsBuilder()
                .UseOpenTelemetry(
                    options.OpenTelemetryMiddleware.LoggerFactory,
                    options.OpenTelemetryMiddleware.SourceName)
                .Build();
        }

        // 4. Logging middleware (outermost)
        if (options.LoggingMiddleware != null)
        {
            agent = agent.AsBuilder()
                .UseLogging(options.LoggingMiddleware.LoggerFactory)
                .Build();
        }

        return agent;
    }
}
```

### 6. Implement Agent Wrapper

```csharp
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// <Provider> AI agent implementation.
/// </summary>
/// <param name="innerAgent">The underlying AI agent.</param>
[PublicAPI]
public class <Provider>Agent(AIAgent innerAgent) : AIAgent
{
    /// <summary>
    /// Gets the inner AI agent.
    /// </summary>
    public AIAgent InnerAgent => innerAgent;

    /// <inheritdoc/>
    protected override Task<AgentResponse> RunCoreAsync(
        string input,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return innerAgent.RunAsync(input, thread, options, cancellationToken);
    }

    /// <inheritdoc/>
    protected override IAsyncEnumerable<StreamingAgentResponse> RunCoreStreamingAsync(
        string input,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return innerAgent.RunStreamingAsync(input, thread, options, cancellationToken);
    }

    /// <inheritdoc/>
    protected override Task<AgentThread> GetNewThreadAsync(
        CancellationToken cancellationToken = default)
    {
        return innerAgent.GetNewThreadAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<AgentThread> DeserializeThreadAsync(
        string threadData,
        CancellationToken cancellationToken = default)
    {
        return innerAgent.DeserializeThreadAsync(threadData, cancellationToken);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        innerAgent.Dispose();
        base.Dispose();
    }
}
```

### 7. Create Chat Models Constants

```csharp
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Constants for <Provider> chat model identifiers.
/// </summary>
[PublicAPI]
public static class <Provider>ChatModels
{
    /// <summary>
    /// <Model description> - Fast and efficient model for most tasks.
    /// </summary>
    public const string FastModel = "provider-fast-model-id";

    /// <summary>
    /// <Model description> - Advanced model with enhanced capabilities.
    /// </summary>
    public const string AdvancedModel = "provider-advanced-model-id";

    /// <summary>
    /// <Model description> - Most capable model for complex tasks.
    /// </summary>
    public const string FlagshipModel = "provider-flagship-model-id";
}
```

### 8. Create Service Collection Extensions

```csharp
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Extension methods for configuring <Provider> services.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <Provider> agent services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The <Provider> API key.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection Add<Provider>Agent(
        this IServiceCollection services,
        string apiKey)
    {
        return services.Add<Provider>Agent(
            new <Provider>Connection { ApiKey = apiKey });
    }

    /// <summary>
    /// Adds <Provider> agent services to the service collection with a connection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connection">The <Provider> connection configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection Add<Provider>Agent(
        this IServiceCollection services,
        <Provider>Connection connection)
    {
        services.AddSingleton(connection);
        services.AddSingleton<<Provider>AgentFactory>();
        return services;
    }
}
```

### 9. Create README.md

```markdown
# AgentFrameworkToolkit.<Provider>

Agent provider for <Provider> using the AgentFrameworkToolkit.

## Installation

```bash
dotnet add package AgentFrameworkToolkit.<Provider>
```

## Quick Start

```csharp
using AgentFrameworkToolkit.<Provider>;

// Create factory with API key
var factory = new <Provider>AgentFactory("<your-api-key>");

// Create agent with options
var agent = factory.CreateAgent(new <Provider>AgentOptions
{
    Model = <Provider>ChatModels.FlagshipModel,
    MaxOutputTokens = 2000,
    Instructions = "You are a helpful assistant.",
    Temperature = 0.7f
});

// Run agent
var response = await agent.RunAsync("Hello!");
Console.WriteLine(response.Message.Text);
```

## Configuration

### Connection Options

- `ApiKey` (required): Your <Provider> API key
- `Endpoint`: Custom API endpoint (optional)
- `NetworkTimeout`: Network request timeout (optional)

### Agent Options

- `Model` (required): Model identifier (see `<Provider>ChatModels`)
- `MaxOutputTokens` (required): Maximum tokens in response
- `Name`: Agent name (optional)
- `Instructions`: System instructions (optional)
- `Tools`: Available tools for the agent (optional)
- `Temperature`: Response randomness 0-1 (optional)
- `TopP`: Nucleus sampling parameter (optional)

### Middleware Options

- `LoggingMiddleware`: Configure logging
- `OpenTelemetryMiddleware`: Configure telemetry
- `ToolCallingMiddleware`: Custom tool execution logic
- `RawToolCallDetails`: Inspect raw tool calls
- `RawHttpCallDetails`: Inspect HTTP requests/responses

## Dependency Injection

```csharp
services.Add<Provider>Agent("<your-api-key>");

// Or with custom connection
services.Add<Provider>Agent(new <Provider>Connection
{
    ApiKey = "<your-api-key>",
    NetworkTimeout = TimeSpan.FromSeconds(60)
});
```

## Advanced Usage

### Custom Middleware

```csharp
var agent = factory.CreateAgent(new <Provider>AgentOptions
{
    Model = <Provider>ChatModels.FlagshipModel,
    MaxOutputTokens = 2000,
    ToolCallingMiddleware = async (tools, cancellationToken, next) =>
    {
        // Custom logic before tool execution
        var result = await next(tools, cancellationToken);
        // Custom logic after tool execution
        return result;
    }
});
```

### Raw Call Inspection

```csharp
var agent = factory.CreateAgent(new <Provider>AgentOptions
{
    Model = <Provider>ChatModels.FlagshipModel,
    MaxOutputTokens = 2000,
    RawHttpCallDetails = details =>
    {
        Console.WriteLine($"Request: {details.RequestUri}");
        Console.WriteLine($"Response: {details.ResponseContent}");
    }
});
```

## License

MIT
```

## Update Repository Files

### 1. Add to Directory.Packages.props

```xml
<PackageVersion Include="<Provider>.SDK" Version="x.y.z" />
```

### 2. Add to AgentFrameworkToolkit.slnx

Add under the `/Packages/` folder group.

### 3. Update Main README.md

Add row to the provider table:

```markdown
| [<Provider>](./src/AgentFrameworkToolkit.<Provider>) | `dotnet add package AgentFrameworkToolkit.<Provider>` |
```

### 4. Update CHANGELOG.md

Add entry:

```markdown
### Added
- New `AgentFrameworkToolkit.<Provider>` package for <Provider> integration
```

## Testing

Create test file in `development/Sandbox/Providers/<Provider>.cs`:

```csharp
using AgentFrameworkToolkit.<Provider>;

public static class <Provider>Example
{
    public static async Task RunAsync()
    {
        var apiKey = Environment.GetEnvironmentVariable("<PROVIDER>_API_KEY")
            ?? throw new InvalidOperationException("API key not found");

        var factory = new <Provider>AgentFactory(apiKey);

        var agent = factory.CreateAgent(new <Provider>AgentOptions
        {
            Model = <Provider>ChatModels.FlagshipModel,
            MaxOutputTokens = 2000,
            Instructions = "You are a helpful assistant.",
            RawHttpCallDetails = details =>
            {
                Console.WriteLine($"API Call: {details.RequestUri}");
            }
        });

        var response = await agent.RunAsync("Hello! Tell me about yourself.");
        Console.WriteLine($"Response: {response.Message.Text}");
    }
}
```

## Common Implementation Challenges

### Challenge 1: SDK to IChatClient Conversion

Some SDKs provide `IChatClient` directly, others need adapters:

```csharp
// If SDK provides IChatClient:
IChatClient chatClient = client.AsChatClient(model);

// If SDK needs adapter (check Microsoft.Extensions.AI docs):
IChatClient chatClient = new <Provider>ChatClient(client, model);
```

### Challenge 2: Provider-Specific Parameters

Map provider-specific options in `CreateChatClient`:

```csharp
private IChatClient CreateChatClient(
    <Provider>Client client,
    <Provider>AgentOptions options)
{
    var chatOptions = new ChatOptions
    {
        ModelId = options.Model,
        Temperature = options.Temperature,
        TopP = options.TopP,
        // Map provider-specific options
        AdditionalProperties = new Dictionary<string, object>
        {
            ["custom_param"] = options.BudgetTokens ?? 0
        }
    };

    return client.AsChatClient(chatOptions);
}
```

## Validation Checklist

- [ ] All classes have XML documentation comments
- [ ] All public types use `[PublicAPI]` attribute
- [ ] Connection creates and configures SDK client
- [ ] AgentOptions includes all standard and provider-specific properties
- [ ] Factory applies middleware in correct order
- [ ] Agent delegates to inner agent
- [ ] Chat models constants provided
- [ ] Service extensions provided
- [ ] README.md with comprehensive usage examples
- [ ] Added to solution file
- [ ] SDK package version in `Directory.Packages.props`
- [ ] Updated main `README.md`
- [ ] Updated `CHANGELOG.md`
- [ ] Builds without warnings: `dotnet build --configuration Release`
- [ ] Tests pass: `dotnet test --configuration Release`
- [ ] Sandbox example works
