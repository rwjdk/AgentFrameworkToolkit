# AgentFrameworkToolkit AI Agent Notes

Use this file as a quick map for where code lives, how it is styled, and how to add new providers.

## Repo layout
- `src/AgentFrameworkToolkit/`: Core library (middleware, tooling, shared extensions).
- `src/AgentFrameworkToolkit.*`: Provider packages (one per provider).
- `src/AgentFrameworkToolkit.Tools.ModelContextProtocol/`: MCP tooling integration.
- `development/`: Sandbox, Secrets utility, and Tests.
- `Directory.Packages.props`: Central package versions.
- `Directory.Build.props`/`Directory.Build.targets`: Shared build and analyzer settings.

## Code style and build rules
- Follow `.editorconfig` (root + `src/.editorconfig`); `src` enforces CRLF, braces, explicit types, collection expressions, and namespace matches folder.
- Nullable is enabled, analyzers are on, and warnings are errors (see `Directory.Build.props`).
- All public APIs require XML documentation; most public types use `[PublicAPI]` (JetBrains.Annotations).
- Prefer primary constructors where the codebase already uses them.

## Adding a new provider package
Pick the smallest implementation path that matches the provider API.

### OpenAI-compatible providers
Use the OpenAI package as the base (see `src/AgentFrameworkToolkit.OpenRouter/` and `src/AgentFrameworkToolkit.XAI/`).
1. Add `src/AgentFrameworkToolkit.<Provider>/` with a `.csproj` that references `AgentFrameworkToolkit.OpenAI` and `AgentFrameworkToolkit`.
2. Create `<Provider>Connection : OpenAIConnection` with a `DefaultEndpoint`.
3. Implement `<Provider>AgentFactory` that wraps `OpenAIAgentFactory` and defaults the endpoint.
4. Implement `<Provider>Agent` that wraps `OpenAIAgent`.
5. Add DI helpers in `ServiceCollectionExtensions`.
6. Reuse `AgentFrameworkToolkit.OpenAI.AgentOptions` and `OpenAIEmbeddingFactory` if applicable.

### Custom providers
Follow patterns in `src/AgentFrameworkToolkit.GitHub/`, `src/AgentFrameworkToolkit.Google/`, or `src/AgentFrameworkToolkit.Mistral/`.
1. Create `src/AgentFrameworkToolkit.<Provider>/` and a `.csproj` that references `AgentFrameworkToolkit`.
2. Add a `<Provider>Connection` that builds the SDK client and supports raw HTTP inspection.
3. Add `<Provider>AgentOptions` (model, instructions, tools, middleware, telemetry, logging, etc.).
4. Add `<Provider>AgentFactory` that creates a `ChatClientAgent` and applies middleware (copy the `ApplyMiddleware` pattern).
5. Add `<Provider>Agent` that subclasses `AIAgent` and delegates to an inner agent.
6. Add `<Provider>ChatModels` constants for model IDs.
7. Add DI helpers in `ServiceCollectionExtensions` and optional embedding factory if the provider supports it.
8. Add a provider `README.md` with install + usage samples.

### Repo updates for any provider
- Add package versions to `Directory.Packages.props` (no versions in `.csproj`).
- Add the project to `AgentFrameworkToolkit.slnx` under `/Packages/`.
- Update `README.md` provider table and `CHANGELOG.md` for user-facing changes.
- Add or extend tests in `development/Tests/` when behavior changes.

## Local validation
- Build: `dotnet build --configuration Release`
- Tests: `dotnet test --configuration Release`
- Sandbox: `dotnet run --project development/Sandbox/Sandbox.csproj`
