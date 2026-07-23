---
name: create-agent-provider
description: Instructions for creating a new Agent Provider NuGet package for AgentFrameworkToolkit
---

# Create Agent Provider Package

This skill guides you through creating a new Agent Provider NuGet package for the AgentFrameworkToolkit. Agent Providers enable integration with different LLM services (e.g., Anthropic, OpenAI, Google, Mistral).

## Quick Decision Guide

**Choose implementation approach:**

1. **OpenAI-Compatible Provider** - If the LLM service has an OpenAI-compatible API:
   - Examples: OpenRouter, XAI (Grok), Cohere
   - See: [OpenAI-Compatible Template](references/OpenAICompatibleTemplate.md)
   - Faster implementation (reuses existing OpenAI code)

2. **Custom Provider** - If the LLM service has a unique API:
   - Examples: Anthropic (Claude), Google (Gemini), GitHub Models, Mistral
   - See: [Custom Provider Template](references/CustomProviderTemplate.md)
   - Full control, follows standard patterns

## Core Provider Components

Most provider packages include these components (OpenAI-compatible providers reuse `AgentFrameworkToolkit.OpenAI.AgentOptions`):

### 1. Connection Class (`<Provider>Connection`)
- Manages API credentials and configuration
- Creates and configures the SDK client
- Supports network timeout and custom endpoints (if applicable)
- Provides raw HTTP call inspection hooks (if possible)

### 2. Agent Factory (`<Provider>AgentFactory`)
- Creates agent instances from options (or from simplified overloads)
- Builds the inner `ChatClientAgent` (custom providers)
- Applies middleware via `AgentFrameworkToolkit.MiddlewareHelper`

### 3. Agent Options (`<Provider>AgentOptions`) (custom providers only)
- Configuration for agent creation (model, instructions, tools, max tokens, etc.)
- Provider-specific settings (e.g., thinking budget)
- Middleware configuration (raw HTTP/tool inspection, tool-calling middleware, OpenTelemetry, logging)

### 4. Agent Wrapper (`<Provider>Agent`)
- Inherits from `AIAgent`
- Delegates to an inner agent instance
- Exposes an `InnerAgent` property

### 5. Chat Models Constants (`<Provider>ChatModels`) (only if provider is specific. if it offers multiple LLMs don't include such)
- Constants for available model IDs
- Makes model selection discoverable

## Implementation Steps

### Step 1: Project Setup

```bash
mkdir src/AgentFrameworkToolkit.<Provider>
```

**Important repo conventions (don’t fight the build system):**
- `src/*` projects inherit defaults from `Directory.Build.props` (currently `net8.0`).
- NuGet packaging defaults are imported via `nuget-package.props` in each `src/*` `.csproj`.
- Central package versions are in `Directory.Packages.props` (no versions in `.csproj`).

### Step 2: Implement Core Components

Follow existing providers:
- **OpenAI-compatible**: `src/AgentFrameworkToolkit.OpenRouter/`, `src/AgentFrameworkToolkit.XAI/`, `src/AgentFrameworkToolkit.Cohere/`
- **Custom**: `src/AgentFrameworkToolkit.Anthropic/`, `src/AgentFrameworkToolkit.GitHub/`, `src/AgentFrameworkToolkit.Google/`, `src/AgentFrameworkToolkit.Mistral/`

### Step 3: Add Service Extensions

Create `ServiceCollectionExtensions.cs` for dependency injection using the repo naming convention:
- `Add<Provider>AgentFactory(this IServiceCollection services, string apiKey)`
- `Add<Provider>AgentFactory(this IServiceCollection services, <Provider>Connection connection)`

### Step 4: Write Tests (Integration Tests)

Provider tests live in `development/Tests/` and make real API calls.
See [Testing Guide](references/TestingGuide.md) for the repo’s concrete pattern.

**Quick checklist:**
1. Add provider project references:
   - `development/Tests/Tests.csproj`
   - `development/Sandbox/Sandbox.csproj`
2. Add provider to the shared test harness:
   - Add a value to `AgentProvider` enum in `development/Tests/TestBase.cs`
   - Add a `case` to `GetAgentForScenarioAsync(...)` in `development/Tests/TestBase.cs`
3. Create `development/Tests/<Provider>Tests.cs`:
   - Call the shared scenario tests (`SimpleAgentTestsAsync`, `NormalAgentTestsAsync`, etc.)
   - Add DI tests for `Add<Provider>AgentFactory(...)`
4. Add a sandbox runner in `development/Sandbox/Providers/<Provider>.cs` and optionally wire it in `development/Sandbox/Program.cs`
5. Add your API key to user-secrets:
   - Update `development/Secrets/Secrets.cs` and `development/Secrets/SecretsManager.cs`
   - Set the secret using `dotnet user-secrets` for the `development/Secrets/Secrets.csproj` project

### Step 5: Repository Integration

1. Add the project to `AgentFrameworkToolkit.slnx` under `/Packages/`
2. Add any new SDK package versions to `Directory.Packages.props`
3. Update documentation:
   - Main `README.md` provider table
   - Provider-specific `src/AgentFrameworkToolkit.<Provider>/README.md`
   - `CHANGELOG.md`

### Step 6: Validation

```bash
dotnet build --configuration Release
```

## Key Architectural Patterns

### Middleware Configuration (custom providers)

Use the shared helper instead of re-implementing ordering rules:
- `AgentFrameworkToolkit.MiddlewareHelper.ApplyMiddleware(...)`

### Agent Factory Pattern (custom providers)

```csharp
public class <Provider>AgentFactory
{
    public <Provider>Connection Connection { get; }

    public <Provider>AgentFactory(string apiKey);
    public <Provider>AgentFactory(<Provider>Connection connection);

    public <Provider>Agent CreateAgent(<Provider>AgentOptions options)
    {
        // 1. Get SDK client from connection
        // 2. Build IChatClient (or use SDK-provided one)
        // 3. Create ChatClientAgent
        // 4. Apply middleware via MiddlewareHelper
        // 5. Wrap in provider-specific agent
    }
}
```

## Common Pitfalls

1. Missing XML documentation (warnings are errors in this repo)
2. Putting versions in `.csproj` instead of `Directory.Packages.props`
3. Forgetting to update `AgentFrameworkToolkit.slnx` and `README.md`
4. Forgetting to add provider to `development/Tests/TestBase.cs` test harness
5. Hardcoding API keys instead of using user-secrets (`development/Secrets/SecretsManager.cs`)

## Reference Documentation

- [OpenAI-Compatible Provider Template](references/OpenAICompatibleTemplate.md)
- [Custom Provider Template](references/CustomProviderTemplate.md)
- [Unit Testing Guide](references/TestingGuide.md)
- [Provider Implementation Checklist](references/ProviderChecklist.md)
