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

Every provider package requires these 5 components:

### 1. Connection Class (`<Provider>Connection`)
- Manages API credentials and configuration
- Creates and configures the SDK client
- Supports network timeout and custom endpoints
- Provides raw HTTP call inspection hooks

### 2. Agent Factory (`<Provider>AgentFactory`)
- Creates agent instances from options
- Applies middleware pipeline (logging, telemetry, tool-calling)
- Handles SDK client initialization
- Provides simple and advanced constructors

### 3. Agent Options (`<Provider>AgentOptions`)
- Configuration for agent creation
- Includes: model, instructions, tools, temperature
- Provider-specific settings (e.g., max tokens, budget tokens)
- Middleware configuration options

### 4. Agent Wrapper (`<Provider>Agent`)
- Inherits from `AIAgent`
- Delegates to inner generic agent
- Provides strongly-typed provider interface
- Exposes `InnerAgent` property

### 5. Chat Models Constants (`<Provider>ChatModels`)
- Constants for available model IDs
- Makes model selection discoverable
- Examples: `ClaudeHaiku45`, `Gpt4o`, `GeminiFlash2`

## Implementation Steps

### Step 1: Project Setup

```bash
# Create project directory
mkdir src/AgentFrameworkToolkit.<Provider>

# Create .csproj file (use references as template)
```

**For OpenAI-compatible providers:**
- Reference: `AgentFrameworkToolkit.OpenAI`
- Reference: `AgentFrameworkToolkit`

**For custom providers:**
- Reference: `AgentFrameworkToolkit`
- Add provider's official SDK package

### Step 2: Implement Core Components

Follow the pattern from existing providers:
- **OpenAI-compatible**: See `src/AgentFrameworkToolkit.OpenRouter/` or `src/AgentFrameworkToolkit.XAI/`
- **Custom**: See `src/AgentFrameworkToolkit.Anthropic/` or `src/AgentFrameworkToolkit.GitHub/`

### Step 3: Add Service Extensions

Create `ServiceCollectionExtensions.cs` for dependency injection:
- `AddSingleton<Provider>Connection`
- `AddSingleton<Provider>AgentFactory`
- Helper methods for common scenarios

### Step 4: Write Unit Tests

Unit tests are **required** for all providers. See [Testing Guide](references/TestingGuide.md) for detailed instructions.

**Quick checklist:**
1. Create test file: `development/Tests/<Provider>Tests.cs`
2. Inherit from `TestsBase`
3. Add provider to `AgentProvider` enum in `TestBase.cs`
4. Override `GetAgentAsync()` method
5. Implement 7 required test methods:
   - `AgentFactory_Simple()`
   - `AgentFactory_Normal()`
   - `AgentFactory_OpenTelemetryAndLoggingMiddleware()`
   - `AgentFactory_ToolCall()`
   - `AgentFactory_McpToolCall()`
   - `AgentFactory_DependencyInjection()`
   - `AgentFactory_DependencyInjection_Connection()`
6. Add API key to `Secrets.cs` and `secrets.json`
7. Run tests: `dotnet test --configuration Release`

### Step 5: Repository Integration

1. **Add to solution**:
   ```bash
   # Add project to AgentFrameworkToolkit.slnx under /Packages/
   ```

2. **Update package management**:
   ```xml
   <!-- Add SDK package version to Directory.Packages.props -->
   <PackageVersion Include="<Provider>.SDK" Version="x.y.z" />
   ```

3. **Update documentation**:
   - Add provider to main `README.md` provider table
   - Create provider-specific `README.md` with usage examples
   - Add entry to `CHANGELOG.md`

### Step 6: Validation

```bash
# Build the solution
dotnet build --configuration Release

# Run tests (must pass!)
dotnet test --configuration Release

# Test in Sandbox
dotnet run --project development/Sandbox/Sandbox.csproj
```

## Key Architectural Patterns

### Middleware Pipeline Order
Apply middleware in this exact order:
1. **Logging Middleware** (outermost)
2. **OpenTelemetry Middleware**
3. **Tool-Calling Middleware**
4. **Raw Tool Details Hook** (innermost)

### Agent Factory Pattern
```csharp
public class <Provider>AgentFactory
{
    public <Provider>Connection Connection { get; }

    // Simple: API key only
    public <Provider>AgentFactory(string apiKey)

    // Advanced: full connection control
    public <Provider>AgentFactory(<Provider>Connection connection)

    public <Provider>Agent CreateAgent(<Provider>AgentOptions options)
    {
        // 1. Get SDK client from connection
        // 2. Create ChatClientAgent
        // 3. Apply middleware pipeline
        // 4. Wrap in provider-specific agent
    }
}
```

### Agent Wrapper Pattern
```csharp
public class <Provider>Agent(AIAgent innerAgent) : AIAgent
{
    public AIAgent InnerAgent => innerAgent;

    // Delegate all methods to innerAgent
}
```

## Code Style Requirements

From `AGENTS.md` and `.editorconfig`:
- Follow CRLF line endings in `src/`
- Enable nullable reference types
- All public APIs require XML documentation comments
- Use `[PublicAPI]` attribute (JetBrains.Annotations)
- Prefer primary constructors (where codebase uses them)
- Warnings as errors (strict)
- Namespace must match folder structure

## Reference Documentation

- [OpenAI-Compatible Provider Template](references/OpenAICompatibleTemplate.md)
- [Custom Provider Template](references/CustomProviderTemplate.md)
- [Unit Testing Guide](references/TestingGuide.md)
- [Provider Implementation Checklist](references/ProviderChecklist.md)

## Common Pitfalls

1. **Middleware Order**: Wrong order breaks functionality
2. **Missing Documentation**: All public APIs need XML docs
3. **Package Versions**: Must be in `Directory.Packages.props`, NOT in `.csproj`
4. **Changelog**: Always update `CHANGELOG.md`
5. **Namespace Mismatch**: Namespace must match folder structure exactly
6. **Missing Unit Tests**: All providers require comprehensive unit tests (7 minimum)
7. **Forgetting to Add Provider to Enum**: Must add to `AgentProvider` enum in `TestBase.cs`
8. **Hardcoded API Keys**: Never commit API keys; use `SecretsManager` in tests

## Testing Your Provider

### Unit Tests (Required)

All providers **must** have comprehensive unit tests. See [Unit Testing Guide](references/TestingGuide.md).

**Required tests:**
1. Simple agent creation
2. Agent with logging and middleware
3. Tool calling
4. MCP tool integration
5. Dependency injection (2 variants)

```bash
# Run your provider tests
dotnet test --configuration Release --filter "FullyQualifiedName~<Provider>Tests"
```

### Sandbox Testing (Required)

Create a test file in `development/Sandbox/Providers/<Provider>.cs`:

```csharp
public static class <Provider>Example
{
    public static async Task RunAsync()
    {
        var apiKey = Environment.GetEnvironmentVariable("<PROVIDER>_API_KEY")
            ?? throw new InvalidOperationException("API key not found");

        var factory = new <Provider>AgentFactory(apiKey);
        var agent = factory.CreateAgent(new <Provider>AgentOptions
        {
            Model = <Provider>ChatModels.DefaultModel,
            MaxOutputTokens = 2000,
            Instructions = "You are a helpful assistant."
        });

        var response = await agent.RunAsync("Hello!");
        Console.WriteLine(response.Text);
    }
}
```

## Next Steps

1. Choose your implementation approach (OpenAI-compatible vs Custom)
2. Review the appropriate template in references/
3. Follow the [Provider Implementation Checklist](references/ProviderChecklist.md)
4. Implement core components
5. **Write unit tests** (see [Testing Guide](references/TestingGuide.md))
6. Validate and test (all tests must pass)
7. Submit PR with updated documentation
