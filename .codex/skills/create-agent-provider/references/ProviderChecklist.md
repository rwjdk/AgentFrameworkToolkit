# Provider Implementation Checklist

Use this checklist to ensure your provider implementation is complete and follows AgentFrameworkToolkit repo conventions.

## Pre-Implementation

- [ ] Determine if the provider API is OpenAI-compatible or requires a custom implementation
- [ ] Identify model IDs to expose (and whether you need a `<Provider>ChatModels` constants class)
- [ ] Identify provider-specific features you want to support (streaming, tool/function calling, embeddings)
- [ ] Identify how to inspect raw HTTP calls (does the SDK accept `HttpClient`?)

## Project Setup

- [ ] Create directory: `src/AgentFrameworkToolkit.<Provider>/`
- [ ] Create `AgentFrameworkToolkit.<Provider>.csproj`:
  - [ ] Has a `<Description>` (repo convention)
  - [ ] Imports `nuget-package.props`
  - [ ] Uses central versions in `Directory.Packages.props` (no versions in `.csproj`)
  - [ ] References the correct projects:
    - [ ] OpenAI-compatible: `AgentFrameworkToolkit.OpenAI` + `AgentFrameworkToolkit`
    - [ ] Custom: `AgentFrameworkToolkit` (+ provider SDK `PackageReference`)
  - [ ] Packs `README.md` into the NuGet package (repo convention)
- [ ] Add SDK package version(s) to `Directory.Packages.props` (if needed)

## Core Components

### Connection (`<Provider>Connection`)

- [ ] Has XML documentation comments (public APIs)
- [ ] Uses `[PublicAPI]` where consistent with nearby code
- [ ] Exposes required credentials (`ApiKey`, `AccessToken`, etc.)
- [ ] Exposes `NetworkTimeout` (optional)
- [ ] If applicable, supports raw HTTP inspection via `Action<RawCallDetails>?`
- [ ] OpenAI-compatible: inherits from `OpenAIConnection` and defines `DefaultEndpoint`
- [ ] Custom: constructs the provider SDK client and supports optional HTTP inspection

### Agent Options (`<Provider>AgentOptions`) (custom providers only)

- [ ] Starts from existing patterns (e.g., `MistralAgentOptions`)
- [ ] Includes common fields (`Model`, `Instructions`, `Tools`, `MaxOutputTokens`, `Temperature`)
- [ ] Includes middleware configuration:
  - [ ] `RawToolCallDetails`
  - [ ] `ToolCallingMiddleware`
  - [ ] `OpenTelemetryMiddleware`
  - [ ] `LoggingMiddleware`
  - [ ] `RawHttpCallDetails`
  - [ ] `Services`, `LoggerFactory`
  - [ ] `AdditionalChatClientAgentOptions` (optional)

### Agent Factory (`<Provider>AgentFactory`)

- [ ] Provides a simple constructor `(string apiKey)`
- [ ] Provides an advanced constructor `(<Provider>Connection connection)`
- [ ] OpenAI-compatible:
  - [ ] Wraps `OpenAIAgentFactory`
  - [ ] Ensures a default endpoint in the constructor (like OpenRouter/Cohere/XAI)
  - [ ] Uses `AgentFrameworkToolkit.OpenAI.AgentOptions`
- [ ] Custom:
  - [ ] Builds an `IChatClient` (or uses SDK-provided one)
  - [ ] Creates `ChatClientAgent`
  - [ ] Applies middleware via `MiddlewareHelper.ApplyMiddleware(...)` (do not re-implement ordering rules)
- [ ] All public methods have XML documentation comments

### Agent Wrapper (`<Provider>Agent`)

- [ ] Primary constructor `(AIAgent innerAgent)`
- [ ] `InnerAgent` property exposes inner agent
- [ ] Delegates to the inner agent (copy/rename an existing provider wrapper)

### Chat Models (`<Provider>ChatModels`) (recommended)

- [ ] Provides `public const string` model IDs (with XML docs)
- [ ] Model IDs match the provider’s official docs

### ServiceCollectionExtensions

- [ ] Uses the repo naming convention:
  - [ ] `Add<Provider>AgentFactory(IServiceCollection, string apiKey)`
  - [ ] `Add<Provider>AgentFactory(IServiceCollection, <Provider>Connection connection)`

## Tests (integration tests in this repo)

- [ ] Add provider project reference to `development/Tests/Tests.csproj`
- [ ] Add provider to `development/Tests/TestBase.cs`:
  - [ ] Add enum value to `AgentProvider`
  - [ ] Add `case` to `GetAgentForScenarioAsync(...)`
- [ ] Create `development/Tests/<Provider>Tests.cs`:
  - [ ] Covers shared scenarios (simple, normal, middleware, tool call, MCP tool call)
  - [ ] Adds DI tests (string API key + connection overload)
- [ ] Add provider API key to user-secrets:
  - [ ] Update `development/Secrets/Secrets.cs` + `development/Secrets/SecretsManager.cs`
  - [ ] Set user-secrets for `development/Secrets/Secrets.csproj`

## Sandbox

- [ ] Add provider project reference to `development/Sandbox/Sandbox.csproj`
- [ ] Create `development/Sandbox/Providers/<Provider>.cs` (use `SecretsManager.GetSecrets()`)
- [ ] Optional: wire it into `development/Sandbox/Program.cs` (use minimal agentoptions in sandbox)

## Repo Updates

- [ ] Add project to `AgentFrameworkToolkit.slnx` under `/Packages/`
- [ ] Update main `README.md` provider table
- [ ] Add provider `src/AgentFrameworkToolkit.<Provider>/README.md`

## Common Issues to Avoid

- [ ] Hardcoded credentials in code
- [ ] Versions in `.csproj` (use `Directory.Packages.props`)
- [ ] Missing XML documentation (warnings are errors)
- [ ] Forgetting to wire provider into `development/Tests/TestBase.cs`
- [ ] Forgetting to add provider project references to sandbox/tests projects
