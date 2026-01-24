# OpenAI-Compatible Provider Template

Use this template when the LLM provider offers an OpenAI-compatible API endpoint. This approach reuses the existing OpenAI-based implementation in this repo.

## Examples in This Repo

- `src/AgentFrameworkToolkit.OpenRouter/`
- `src/AgentFrameworkToolkit.XAI/`
- `src/AgentFrameworkToolkit.Cohere/`

## 1. Create Project Structure

```
src/AgentFrameworkToolkit.<Provider>/
├── AgentFrameworkToolkit.<Provider>.csproj
├── <Provider>Connection.cs
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
    <ProjectReference Include="..\AgentFrameworkToolkit.OpenAI\AgentFrameworkToolkit.OpenAI.csproj" />
    <ProjectReference Include="..\AgentFrameworkToolkit\AgentFrameworkToolkit.csproj" />
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

Match the repo’s OpenAI-compatible providers: define a default endpoint constant and set it in the factory constructor(s).

```csharp
using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.<Provider>;

/// <summary>
/// Represents a connection for <Provider>.
/// </summary>
[PublicAPI]
public class <Provider>Connection : OpenAIConnection
{
    /// <summary>
    /// The default <Provider> endpoint.
    /// </summary>
    public const string DefaultEndpoint = "https://api.<provider>.com/v1";
}
```

## 4. Agent Factory (`<Provider>AgentFactory`)

Mirror `OpenRouterAgentFactory`, `CohereAgentFactory`, or `XAIAgentFactory`.

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
    /// Connection.
    /// </summary>
    public <Provider>Connection Connection { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="apiKey">Your <Provider> API key (for advanced configuration use the connection overload).</param>
    public <Provider>AgentFactory(string apiKey)
    {
        Connection = new <Provider>Connection
        {
            ApiKey = apiKey,
            Endpoint = <Provider>Connection.DefaultEndpoint
        };

        _openAIAgentFactory = new OpenAIAgentFactory(Connection);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="connection">Connection details.</param>
    public <Provider>AgentFactory(<Provider>Connection connection)
    {
        connection.Endpoint ??= <Provider>Connection.DefaultEndpoint;
        Connection = connection;
        _openAIAgentFactory = new OpenAIAgentFactory(connection);
    }

    /// <summary>
    /// Create a simple agent with default settings (for advanced scenarios use the options overload).
    /// </summary>
    public <Provider>Agent CreateAgent(string model, string? instructions = null, string? name = null, IList<AITool>? tools = null)
    {
        return CreateAgent(new AgentOptions
        {
            Model = model,
            Name = name,
            Instructions = instructions,
            Tools = tools
        });
    }

    /// <summary>
    /// Create a new agent.
    /// </summary>
    public <Provider>Agent CreateAgent(AgentOptions options)
    {
        return new <Provider>Agent(_openAIAgentFactory.CreateAgent(options));
    }
}
```

## 5. Agent Wrapper (`<Provider>Agent`)

Do not try to guess `AIAgent` override signatures; copy and rename an existing wrapper:
- `src/AgentFrameworkToolkit.OpenRouter/OpenRouterAgent.cs` (full delegation + structured output helpers)
- `src/AgentFrameworkToolkit.Cohere/CohereAgent.cs`
- `src/AgentFrameworkToolkit.XAI/XAIAgent.cs`

Minimum requirements:
- primary constructor `(AIAgent innerAgent)`
- `InnerAgent` property
- delegate `Id`, `Name`, `Description`, `GetService`, thread methods, `RunCoreAsync(...)`, `RunCoreStreamingAsync(...)`, and `Dispose()`

## 6. DI Extensions (`ServiceCollectionExtensions`)

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
    /// <summary>
    /// Register a <Provider>AgentFactory as a Singleton.
    /// </summary>
    public static IServiceCollection Add<Provider>AgentFactory(this IServiceCollection services, <Provider>Connection connection)
    {
        return services.AddSingleton(new <Provider>AgentFactory(connection));
    }

    /// <summary>
    /// Register a <Provider>AgentFactory as a Singleton.
    /// </summary>
    public static IServiceCollection Add<Provider>AgentFactory(this IServiceCollection services, string apiKey)
    {
        return services.AddSingleton(new <Provider>AgentFactory(apiKey));
    }
}
```

## 7. README Notes

In this repo, `AgentResponse` has `.Text` (see tests in `development/Tests/*`).

```csharp
var factory = new <Provider>AgentFactory("<api-key>");
var agent = factory.CreateAgent(new AgentOptions { Model = "<model>", MaxOutputTokens = 2000 });
var response = await agent.RunAsync("Hello");
Console.WriteLine(response.Text);
```

## 8. Repo Integration Checklist

- Add project to `AgentFrameworkToolkit.slnx` under `/Packages/`
- Add provider row to main `README.md` and create provider `README.md`
- Add any new SDK dependencies to `Directory.Packages.props`
- Update `CHANGELOG.md`
- Add `ProjectReference` to:
  - `development/Tests/Tests.csproj`
  - `development/Sandbox/Sandbox.csproj`
- Add tests and wire provider into `development/Tests/TestBase.cs` (see [Testing Guide](TestingGuide.md))
