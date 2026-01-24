# Testing Guide for Agent Providers (This Repo)

Provider tests in this repo live under `development/Tests/` and are **integration tests** (they make real API calls). They are written with **xUnit v3**.

## Where the Test Harness Lives

- Shared test harness: `development/Tests/TestBase.cs` (`TestsBase` + `AgentProvider`/`AgentScenario`)
- Provider-specific test files: `development/Tests/<Provider>Tests.cs`

Provider test files typically:
- call shared scenario tests (e.g., `SimpleAgentTestsAsync(...)`)
- add DI coverage for `Add<Provider>AgentFactory(...)`

## Step-by-Step: Add Tests for a New Provider

### 1) Add the provider project reference

Add a `ProjectReference` to your provider in `development/Tests/Tests.csproj`.

### 2) Add provider secret(s) (user-secrets)

This repo uses .NET user-secrets via `development/Secrets/SecretsManager.cs`.

1. Add a new property to the `Secrets` record:
   - `development/Secrets/Secrets.cs`
2. Read it in `SecretsManager.GetSecrets()`:
   - `development/Secrets/SecretsManager.cs`
3. Set the secret for the `development/Secrets/Secrets.csproj` project using `dotnet user-secrets`.

### 3) Wire the provider into the shared harness

In `development/Tests/TestBase.cs`:

1. Add a new value to the `AgentProvider` enum.
2. Add a new `case` in `GetAgentForScenarioAsync(...)` that creates your provider agent for each scenario.

Patterns to follow:
- OpenAI-compatible providers (OpenRouter/Cohere/XAI) use the shared OpenAI options helper already in `TestBase.cs`.
- Custom providers (Anthropic/GitHub/Google/Mistral) define a provider-specific options helper (e.g., `GetMistralAgentOptions(...)`) and validate `RawHttpCallDetails` as needed.

### 4) Create `development/Tests/<Provider>Tests.cs`

Minimum recommended coverage (mirrors existing providers):

```csharp
public sealed class <Provider>Tests : TestsBase
{
    [Fact] public Task AgentFactory_Simple() => SimpleAgentTestsAsync(AgentProvider.<Provider>);
    [Fact] public Task AgentFactory_Normal() => NormalAgentTestsAsync(AgentProvider.<Provider>);
    [Fact] public Task AgentFactory_OpenTelemetryAndLoggingMiddleware() => OpenTelemetryAndLoggingMiddlewareTestsAsync(AgentProvider.<Provider>);
    [Fact] public Task AgentFactory_ToolCall() => ToolCallAgentTestsAsync(AgentProvider.<Provider>);
    [Fact] public Task AgentFactory_McpToolCall() => McpToolCallAgentTestsAsync(AgentProvider.<Provider>);
}
```

If the provider supports structured output with JSON schema, add:
- `StructuredOutputAgentTestsAsync(AgentProvider.<Provider>)` (see `OpenRouterTests` for an example)

### 5) Add DI tests (API key + connection)

Add these two tests (pattern: `AnthropicTests`, `OpenRouterTests`, etc.):

- `AgentFactory_DependencyInjection()`: `services.Add<Provider>AgentFactory(secrets.<Provider>ApiKey);`
- `AgentFactory_DependencyInjection_Connection()`: `services.Add<Provider>AgentFactory(new <Provider>Connection { ... });`

Then resolve the factory and run a real call (or, if the provider is known to hang in CI, at least verify DI resolution; see `GitHubTests` pattern).

## Running Tests

```bash
dotnet test --configuration Release
dotnet test --configuration Release --filter "FullyQualifiedName~<Provider>Tests"
```

## Notes / Gotchas

- These tests incur cost and require valid credentials.
- If your provider has multiple “client types” (like OpenAI-based providers), you may want separate enum values for each and separate test files.

## Reference Files (Good Examples)

- `development/Tests/TestBase.cs` (shared harness)
- `development/Tests/AnthropicTests.cs` (custom provider)
- `development/Tests/OpenRouterTests.cs` (OpenAI-compatible + structured output + embeddings)
- `development/Tests/XAITests.cs` (OpenAI-compatible)
- `development/Secrets/Secrets.cs` and `development/Secrets/SecretsManager.cs` (secrets)
