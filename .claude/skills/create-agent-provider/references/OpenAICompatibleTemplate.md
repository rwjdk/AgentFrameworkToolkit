# OpenAI-Compatible Provider Template

Use this template when the LLM provider offers an OpenAI-compatible API endpoint. This approach reuses the existing OpenAI implementation and only requires minimal customization.

## Examples in Codebase

- `src/AgentFrameworkToolkit.OpenRouter/` - Multi-model routing service
- `src/AgentFrameworkToolkit.XAI/` - XAI (Grok) models
- `src/AgentFrameworkToolkit.Cohere/` - Cohere models

## Step-by-Step Implementation

### 1. Create Project Structure

```
src/AgentFrameworkToolkit.<Provider>/
├── <Provider>Connection.cs
├── <Provider>AgentFactory.cs
├── <Provider>Agent.cs
├── <Provider>ChatModels.cs (optional)
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
    <ProjectReference Include="..\AgentFrameworkToolkit.OpenAI\AgentFrameworkToolkit.OpenAI.csproj" />
    <ProjectReference Include="..\AgentFrameworkToolkit\AgentFrameworkToolkit.csproj" />
  </ItemGroup>

</Project>
```

### 3. Implement Connection Class

```csharp
using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Connection configuration for <Provider> API.
/// </summary>
[PublicAPI]
public class <Provider>Connection : OpenAIConnection
{
    /// <summary>
    /// Default <Provider> API endpoint.
    /// </summary>
    public const string DefaultEndpoint = "https://api.<provider>.com/v1";

    /// <summary>
    /// Initializes a new instance of the <see cref="<Provider>Connection"/> class.
    /// </summary>
    public <Provider>Connection()
    {
        Endpoint = DefaultEndpoint;
    }
}
```

### 4. Implement Agent Factory

```csharp
using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Factory for creating <Provider> agents.
/// </summary>
[PublicAPI]
public class <Provider>AgentFactory
{
    private readonly OpenAIAgentFactory _openAIAgentFactory;

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
        _openAIAgentFactory = new OpenAIAgentFactory(connection);
    }

    /// <summary>
    /// Creates a new <Provider> agent with the specified options.
    /// </summary>
    /// <param name="options">The agent configuration options.</param>
    /// <returns>A configured <see cref="<Provider>Agent"/>.</returns>
    public <Provider>Agent CreateAgent(AgentOptions options)
    {
        var openAIAgent = _openAIAgentFactory.CreateAgent(options);
        return new <Provider>Agent(openAIAgent.InnerAgent);
    }
}
```

### 5. Implement Agent Wrapper

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
    protected override Task<AgentThread> GetNewThreadAsync(CancellationToken cancellationToken = default)
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

### 6. (Optional) Create Chat Models Constants

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
    /// <Model description>
    /// </summary>
    public const string ModelName1 = "model-id-1";

    /// <summary>
    /// <Model description>
    /// </summary>
    public const string ModelName2 = "model-id-2";
}
```

### 7. Create Service Collection Extensions

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
        return services.Add<Provider>Agent(new <Provider>Connection { ApiKey = apiKey });
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
        services.AddSingleton<Provider>AgentFactory>();
        return services;
    }
}
```

### 8. Create README.md

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
var agent = factory.CreateAgent(new AgentOptions
{
    Model = <Provider>ChatModels.ModelName1,
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
- `Endpoint`: Custom API endpoint (defaults to `<Provider>Connection.DefaultEndpoint`)
- `NetworkTimeout`: Network request timeout

### Agent Options

Uses `AgentFrameworkToolkit.OpenAI.AgentOptions`:
- `Model`: Model identifier (see `<Provider>ChatModels`)
- `MaxOutputTokens`: Maximum tokens in response
- `Instructions`: System instructions
- `Tools`: Available tools for the agent
- `Temperature`: Response randomness (0-1)
- And more...

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

## License

MIT
```

## Update Repository Files

### 1. Add to Directory.Packages.props

If the provider has a specific SDK package, add it:

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
        var factory = new <Provider>AgentFactory(
            Environment.GetEnvironmentVariable("<PROVIDER>_API_KEY")
                ?? throw new InvalidOperationException("API key not found"));

        var agent = factory.CreateAgent(new AgentOptions
        {
            Model = <Provider>ChatModels.ModelName1,
            MaxOutputTokens = 2000,
            Instructions = "You are a helpful assistant."
        });

        var response = await agent.RunAsync("Hello!");
        Console.WriteLine(response.Message.Text);
    }
}
```

## Validation Checklist

- [ ] All classes have XML documentation comments
- [ ] All public types use `[PublicAPI]` attribute
- [ ] Connection inherits from `OpenAIConnection`
- [ ] Factory wraps `OpenAIAgentFactory`
- [ ] Agent delegates to inner agent
- [ ] Service extensions provided
- [ ] README.md with usage examples
- [ ] Added to solution file
- [ ] Package versions in `Directory.Packages.props`
- [ ] Updated main `README.md`
- [ ] Updated `CHANGELOG.md`
- [ ] Builds without warnings: `dotnet build --configuration Release`
- [ ] Tests pass: `dotnet test --configuration Release`
