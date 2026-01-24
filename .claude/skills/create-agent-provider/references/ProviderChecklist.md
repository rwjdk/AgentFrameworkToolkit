# Provider Implementation Checklist

Use this checklist to ensure your provider implementation is complete and follows all AgentFrameworkToolkit standards.

## Pre-Implementation

- [ ] Determine if provider API is OpenAI-compatible or requires custom implementation
- [ ] Obtain provider SDK package name and version
- [ ] Identify available model IDs from provider documentation
- [ ] Understand provider-specific parameters and capabilities
- [ ] Check if provider supports streaming responses
- [ ] Check if provider supports tool/function calling

## Project Setup

- [ ] Create project directory: `src/AgentFrameworkToolkit.<Provider>/`
- [ ] Create `.csproj` file with correct references
  - [ ] OpenAI-compatible: Reference `AgentFrameworkToolkit.OpenAI` and `AgentFrameworkToolkit`
  - [ ] Custom: Reference `AgentFrameworkToolkit` and provider SDK
- [ ] Set `TargetFramework` to `net9.0`
- [ ] Set `ImplicitUsings` to `enable`
- [ ] Set `Nullable` to `enable`
- [ ] Set `IsPackable` to `true`
- [ ] Set correct `RootNamespace`: `AgentFrameworkToolkit.<Provider>`
- [ ] Add meaningful `Description`

## Core Components Implementation

### Connection Class (`<Provider>Connection`)

- [ ] Class name: `<Provider>Connection`
- [ ] XML documentation comment for class
- [ ] `[PublicAPI]` attribute on class
- [ ] `ApiKey` property (required string)
- [ ] `Endpoint` property (optional string)
- [ ] `NetworkTimeout` property (optional TimeSpan)
- [ ] `GetClient()` method that returns SDK client
- [ ] Support for `RawCallDetails` callback in `GetClient()`
- [ ] OpenAI-compatible: Inherit from `OpenAIConnection`
- [ ] Custom: Implement all properties and methods

### Agent Options (`<Provider>AgentOptions`)

**Note:** Only needed for custom providers (OpenAI-compatible reuses `AgentOptions`)

- [ ] Class name: `<Provider>AgentOptions`
- [ ] XML documentation comment for class
- [ ] `[PublicAPI]` attribute on class
- [ ] `Model` property (required string)
- [ ] `MaxOutputTokens` property (required int)
- [ ] `Name` property (optional string)
- [ ] `Instructions` property (optional string)
- [ ] `Tools` property (optional IList<AITool>)
- [ ] `Temperature` property (optional float)
- [ ] `TopP` property (optional float)
- [ ] Provider-specific properties (e.g., `BudgetTokens`)
- [ ] `Services` property (optional IServiceProvider)
- [ ] `LoggerFactory` property (optional ILoggerFactory)
- [ ] `LoggingMiddleware` property (optional)
- [ ] `OpenTelemetryMiddleware` property (optional)
- [ ] `ToolCallingMiddleware` property (optional)
- [ ] `RawToolCallDetails` property (optional)
- [ ] `RawHttpCallDetails` property (optional)
- [ ] `AdditionalChatClientAgentOptions` property (optional)
- [ ] All properties have XML documentation comments

### Agent Factory (`<Provider>AgentFactory`)

- [ ] Class name: `<Provider>AgentFactory`
- [ ] XML documentation comment for class
- [ ] `[PublicAPI]` attribute on class
- [ ] `Connection` property (type: `<Provider>Connection`)
- [ ] Constructor: `(string apiKey)` - simple API key only
- [ ] Constructor: `(<Provider>Connection connection)` - full configuration
- [ ] `CreateAgent()` method with correct parameter type
  - [ ] OpenAI-compatible: Parameter type `AgentOptions`
  - [ ] Custom: Parameter type `<Provider>AgentOptions`
- [ ] Returns `<Provider>Agent`
- [ ] Custom providers: Middleware applied in correct order:
  1. [ ] Raw tool details (innermost)
  2. [ ] Tool-calling middleware
  3. [ ] OpenTelemetry middleware
  4. [ ] Logging middleware (outermost)
- [ ] All methods have XML documentation comments

### Agent Wrapper (`<Provider>Agent`)

- [ ] Class name: `<Provider>Agent`
- [ ] XML documentation comment for class
- [ ] `[PublicAPI]` attribute on class
- [ ] Inherits from `AIAgent`
- [ ] Primary constructor: `(AIAgent innerAgent)`
- [ ] `InnerAgent` property exposes inner agent
- [ ] Override `RunCoreAsync()` - delegates to inner agent
- [ ] Override `RunCoreStreamingAsync()` - delegates to inner agent
- [ ] Override `GetNewThreadAsync()` - delegates to inner agent
- [ ] Override `DeserializeThreadAsync()` - delegates to inner agent
- [ ] Override `Dispose()` - disposes inner agent and calls base
- [ ] All methods have XML documentation comments

### Chat Models (`<Provider>ChatModels`)

- [ ] Class name: `<Provider>ChatModels`
- [ ] XML documentation comment for class
- [ ] `[PublicAPI]` attribute on class
- [ ] Static class
- [ ] Public const string for each model
- [ ] Each constant has XML documentation comment
- [ ] Model IDs match provider's official documentation
- [ ] Naming follows pattern: descriptive name (e.g., `GeminiFlash2`, `ClaudeHaiku45`)

### Service Collection Extensions

- [ ] Class name: `ServiceCollectionExtensions`
- [ ] XML documentation comment for class
- [ ] `[PublicAPI]` attribute on class
- [ ] Static class
- [ ] Extension method: `Add<Provider>Agent(IServiceCollection, string apiKey)`
- [ ] Extension method: `Add<Provider>Agent(IServiceCollection, <Provider>Connection)`
- [ ] Methods register `<Provider>Connection` as singleton
- [ ] Methods register `<Provider>AgentFactory` as singleton
- [ ] Methods return `IServiceCollection` for chaining
- [ ] All methods have XML documentation comments

## Documentation

### Provider README.md

- [ ] File created: `src/AgentFrameworkToolkit.<Provider>/README.md`
- [ ] Title: `# AgentFrameworkToolkit.<Provider>`
- [ ] Brief description of provider
- [ ] Installation section with `dotnet add package` command
- [ ] Quick Start section with minimal example
- [ ] Configuration section documenting all options
- [ ] Advanced usage examples (middleware, streaming, etc.)
- [ ] Dependency injection examples
- [ ] License section

### Code Documentation

- [ ] All public classes have XML documentation comments
- [ ] All public methods have XML documentation comments
- [ ] All public properties have XML documentation comments
- [ ] All public constants have XML documentation comments
- [ ] All documentation follows C# XML comment standards
- [ ] All public types use `[PublicAPI]` attribute

## Repository Integration

### Package Management

- [ ] SDK package version added to `Directory.Packages.props`
- [ ] No package versions in `.csproj` file (central management only)
- [ ] PackageReference uses `<PackageReference Include="..." />` without Version

### Solution Configuration

- [ ] Project added to `AgentFrameworkToolkit.slnx`
- [ ] Project placed under `/Packages/` folder group
- [ ] Solution file is valid XML

### Main README.md

- [ ] Provider added to provider table with link
- [ ] Installation command included
- [ ] Table row format matches existing rows

### CHANGELOG.md

- [ ] New entry added under `### Added` section
- [ ] Entry format: `- New AgentFrameworkToolkit.<Provider> package for <Provider> integration`
- [ ] Entry placed in appropriate version section
- [ ] Follows existing changelog format

## Code Quality

### Style Compliance

- [ ] Follows `.editorconfig` rules
- [ ] CRLF line endings in `src/` directory
- [ ] Braces on new lines
- [ ] Explicit types (no `var` unless obvious)
- [ ] Collection expressions where appropriate
- [ ] Namespace matches folder structure exactly

### Nullability

- [ ] Nullable reference types enabled
- [ ] Required properties marked with `required`
- [ ] Optional properties marked with `?`
- [ ] No nullable warnings

### Build Configuration

- [ ] Builds without errors: `dotnet build --configuration Release`
- [ ] No build warnings (warnings treated as errors)
- [ ] Analyzers pass without issues

## Testing

### Sandbox Example

- [ ] File created: `development/Sandbox/Providers/<Provider>.cs`
- [ ] Contains `<Provider>Example` static class
- [ ] Contains `RunAsync()` static method
- [ ] Reads API key from environment variable
- [ ] Creates factory and agent
- [ ] Makes at least one API call
- [ ] Outputs response to console
- [ ] Includes error handling
- [ ] Demonstrates key features

### Manual Testing

- [ ] Sandbox example runs successfully
- [ ] Basic agent creation works
- [ ] Simple message produces response
- [ ] Error messages are clear and helpful
- [ ] API authentication works correctly

### Optional: Unit Tests

- [ ] Tests added to `development/Tests/`
- [ ] Tests cover core functionality
- [ ] Tests pass: `dotnet test --configuration Release`

## Validation

### Pre-Release Checks

- [ ] Full build succeeds: `dotnet build --configuration Release`
- [ ] No compiler warnings or errors
- [ ] All tests pass: `dotnet test --configuration Release`
- [ ] Sandbox example runs successfully
- [ ] Documentation is complete and accurate
- [ ] All files use correct line endings (CRLF in src/)
- [ ] No hardcoded credentials or secrets

### Code Review Ready

- [ ] All checklist items completed
- [ ] Git commit follows repository conventions
- [ ] Commit message is descriptive
- [ ] All new/changed features documented in CHANGELOG.md
- [ ] PR description explains provider purpose and usage
- [ ] No unnecessary files included (bin/, obj/, .vs/, etc.)

## Common Issues to Avoid

### Critical Errors

- [ ] Middleware applied in wrong order (breaks functionality)
- [ ] Missing XML documentation (build error)
- [ ] Missing `[PublicAPI]` attribute (inconsistent with codebase)
- [ ] Package versions in `.csproj` (should be in Directory.Packages.props)
- [ ] Namespace doesn't match folder (build error)
- [ ] Wrong line endings (LF instead of CRLF in src/)

### Common Mistakes

- [ ] Forgetting to update CHANGELOG.md
- [ ] Forgetting to add provider to main README.md
- [ ] Incomplete XML documentation comments
- [ ] Not testing in Sandbox before submitting
- [ ] Hardcoded API keys or credentials
- [ ] Missing disposal of resources
- [ ] Not handling nullable types correctly

## OpenAI-Compatible Specific

If implementing an OpenAI-compatible provider:

- [ ] Connection inherits from `OpenAIConnection`
- [ ] Factory wraps `OpenAIAgentFactory`
- [ ] Reuses `AgentFrameworkToolkit.OpenAI.AgentOptions`
- [ ] Default endpoint set correctly in connection
- [ ] No custom AgentOptions class needed
- [ ] Agent delegates to OpenAI agent's inner agent

## Custom Provider Specific

If implementing a custom provider:

- [ ] Connection implements full client creation logic
- [ ] Custom `<Provider>AgentOptions` class created
- [ ] Factory implements full middleware pipeline
- [ ] SDK client to `IChatClient` conversion implemented
- [ ] Provider-specific options mapped correctly
- [ ] Middleware applied in correct order

## Final Verification

- [ ] Review this entire checklist one more time
- [ ] Compare implementation with similar existing provider
- [ ] Test all documented examples
- [ ] Verify all links in documentation work
- [ ] Ensure no placeholder text remains (e.g., `<Provider>`)
- [ ] Ready for PR submission

## Post-Implementation

After PR is merged:

- [ ] Verify package builds successfully in CI/CD
- [ ] Test package installation from NuGet
- [ ] Update personal documentation/notes if needed
- [ ] Celebrate successful contribution! 🎉
