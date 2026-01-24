# Custom Provider Template

Use this template when the LLM provider has a unique API that is NOT OpenAI-compatible. This approach follows the patterns already used by custom providers in this repo.

## Examples in This Repo

- `src/AgentFrameworkToolkit.Anthropic/`
- `src/AgentFrameworkToolkit.GitHub/`
- `src/AgentFrameworkToolkit.Google/`
- `src/AgentFrameworkToolkit.Mistral/`

## 1. Create Project Structure

```
src/AgentFrameworkToolkit.<Provider>/
├── AgentFrameworkToolkit.<Provider>.csproj
├── <Provider>Connection.cs
├── <Provider>AgentOptions.cs
├── <Provider>AgentFactory.cs
├── <Provider>Agent.cs
├── <Provider>ChatModels.cs (optional)
├── ServiceCollectionExtensions.cs
└── README.md
```

## 2. Create `.csproj`

In this repo, `src/*` projects:
- inherit `TargetFramework`, `Nullable`, `ImplicitUsings`, analyzers, etc. from `Directory.Build.props`
- import packaging defaults from `nuget-package.props`
- use central package versions from `Directory.Packages.props`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <Description>Agent provider for <Provider> using AgentFrameworkToolkit</Description>
  </PropertyGroup>

  <Import Project="..\..\nuget-package.props" />

  <ItemGroup>
    <ProjectReference Include="..\AgentFrameworkToolkit\AgentFrameworkToolkit.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Provider SDK package; version goes in Directory.Packages.props -->
    <PackageReference Include="<Provider>.SDK" />
  </ItemGroup>

  <ItemGroup>
    <None Include="README.md">
      <Pack>True</Pack>
      <PackagePath>\</PackagePath>
    </None>
  </ItemGroup>

</Project>
```

## 3. Connection (`<Provider>Connection`)

Match the “connection creates SDK client” pattern (see `MistralConnection`, `GitHubConnection`).
If the SDK supports injecting `HttpClient`, you can support request/response inspection via `RawCallDetailsHttpHandler`.

```csharp
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Represents a connection for <Provider>.
/// </summary>
[PublicAPI]
public class <Provider>Connection
{
    /// <summary>
    /// The API key to be used.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Optional base endpoint override (show it if the SDK supports it).
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The timeout value of the LLM call.
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Get a raw SDK client.
    /// </summary>
    /// <param name="rawHttpCallDetails">
    /// An Action, if set, will attach an HTTP message handler so you can see the raw HTTP calls sent to the LLM.
    /// </param>
    public <Provider>SdkClient GetClient(Action<RawCallDetails>? rawHttpCallDetails = null)
    {
        HttpClient? httpClient = null;
        if (rawHttpCallDetails != null)
        {
            httpClient = new HttpClient(new RawCallDetailsHttpHandler(rawHttpCallDetails));
        }

        if (NetworkTimeout.HasValue)
        {
            httpClient ??= new HttpClient();
            httpClient.Timeout = NetworkTimeout.Value;
        }

        // Construct and return the SDK client using ApiKey / Endpoint / httpClient
        throw new NotImplementedException("Create the provider SDK client here.");
    }
}
```

## 4. Agent Options (`<Provider>AgentOptions`)

Start from `MistralAgentOptions` and add provider-specific knobs only where needed.

```csharp
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using static Microsoft.Agents.AI.ChatClientAgentOptions;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Options for a <Provider> agent.
/// </summary>
public class <Provider>AgentOptions
{
    public required string Model { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public IList<AITool>? Tools { get; set; }

    public int? MaxOutputTokens { get; set; }
    public float? Temperature { get; set; }

    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }
    public Action<ChatClientAgentOptions>? AdditionalChatClientAgentOptions { get; set; }

    public IServiceProvider? Services { get; set; }
    public ILoggerFactory? LoggerFactory { get; set; }

    public MiddlewareDelegates.ToolCallingMiddlewareDelegate? ToolCallingMiddleware { get; set; }
    public OpenTelemetryMiddleware? OpenTelemetryMiddleware { get; set; }
    public LoggingMiddleware? LoggingMiddleware { get; set; }

    public Func<ChatMessageStoreFactoryContext, CancellationToken, ValueTask<ChatMessageStore>>? ChatMessageStoreFactory { get; set; }
    public Func<AIContextProviderFactoryContext, CancellationToken, ValueTask<AIContextProvider>>? AIContextProviderFactory { get; set; }

    // Add provider-specific properties (e.g., thinking budget) here.
}
```

## 5. Agent Factory (`<Provider>AgentFactory`)

Pattern (see `MistralAgentFactory`):
1. Build SDK client via connection
2. Convert to an `IChatClient`
3. Create `ChatClientAgent`
4. Apply middleware via `MiddlewareHelper.ApplyMiddleware`
5. Wrap in a provider-specific `AIAgent`

```csharp
using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Factory for creating <Provider> agents.
/// </summary>
[PublicAPI]
public class <Provider>AgentFactory
{
    /// <summary>
    /// Connection.
    /// </summary>
    public <Provider>Connection Connection { get; }

    public <Provider>AgentFactory(string apiKey)
    {
        Connection = new <Provider>Connection { ApiKey = apiKey };
    }

    public <Provider>AgentFactory(<Provider>Connection connection)
    {
        Connection = connection;
    }

    public <Provider>Agent CreateAgent(<Provider>AgentOptions options)
    {
        // 1) SDK client
        var sdkClient = Connection.GetClient(options.RawHttpCallDetails);

        // 2) Convert SDK client to IChatClient (provider-specific)
        IChatClient chatClient = CreateChatClient(sdkClient);

        // 3) Build ChatClientAgentOptions
        ChatOptions chatOptions = new()
        {
            ModelId = options.Model,
            Instructions = options.Instructions,
            MaxOutputTokens = options.MaxOutputTokens,
            Temperature = options.Temperature,
            Tools = options.Tools
        };

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Id = options.Id,
            Name = options.Name,
            Description = options.Description,
            ChatOptions = chatOptions,
            AIContextProviderFactory = options.AIContextProviderFactory,
            ChatMessageStoreFactory = options.ChatMessageStoreFactory
        };

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        AIAgent innerAgent = new ChatClientAgent(chatClient, chatClientAgentOptions, options.LoggerFactory, options.Services);

        // 4) Apply middleware (shared helper)
        innerAgent = MiddlewareHelper.ApplyMiddleware(
            innerAgent,
            options.RawToolCallDetails,
            options.ToolCallingMiddleware,
            options.OpenTelemetryMiddleware,
            options.LoggingMiddleware,
            options.Services);

        // 5) Wrap
        return new <Provider>Agent(innerAgent);
    }

    private static IChatClient CreateChatClient(<Provider>SdkClient sdkClient)
    {
        throw new NotImplementedException("Adapt the provider SDK client to IChatClient.");
    }
}
```

## 6. Agent Wrapper (`<Provider>Agent`)

Copy and rename an existing wrapper:
- `src/AgentFrameworkToolkit.Mistral/MistralAgent.cs`
- `src/AgentFrameworkToolkit.GitHub/GitHubAgent.cs`
- `src/AgentFrameworkToolkit.Anthropic/AnthropicAgent.cs`

Minimum requirements:
- primary constructor `(AIAgent innerAgent)`
- `InnerAgent` property
- delegate `Id`, `Name`, `Description`, `GetService`, thread methods, `RunCoreAsync(...)`, `RunCoreStreamingAsync(...)`, and `Dispose()`

## 7. DI Extensions (`ServiceCollectionExtensions`)

Follow the repo naming convention (`Add<Provider>AgentFactory`):

```csharp
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Extension methods for IServiceCollection.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add<Provider>AgentFactory(this IServiceCollection services, <Provider>Connection connection)
    {
        return services.AddSingleton(new <Provider>AgentFactory(connection));
    }

    public static IServiceCollection Add<Provider>AgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new <Provider>AgentFactory(apiKey));
    }
}
```

## 8. Tests, Sandbox, and Repo Wiring

- Add project to `AgentFrameworkToolkit.slnx` under `/Packages/`
- Add `ProjectReference` to:
  - `development/Tests/Tests.csproj`
  - `development/Sandbox/Sandbox.csproj`
- Wire the provider into the shared test harness:
  - `development/Tests/TestBase.cs` (`AgentProvider` enum + `GetAgentForScenarioAsync(...)` switch)
- Add a sandbox runner in `development/Sandbox/Providers/<Provider>.cs`
- Add the provider’s API key to user-secrets via `development/Secrets/SecretsManager.cs`

See [Testing Guide](TestingGuide.md) for the concrete `TestBase.cs` pattern.
