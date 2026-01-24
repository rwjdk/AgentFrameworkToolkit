# AgentFrameworkToolkit — AI Agent Notes

Use this file as a quick map for where code lives and how to make changes in a way that fits this repo.

## Conversation start rule (MANDATORY)
Whenever a **new conversation** begins, do **not** start coding immediately.

1. Investigate the codebase (read relevant files; search for existing patterns).
2. Identify what’s unclear, risky, or ambiguous about the requested change.
3. Ask the user **3–10 clarifying questions in a numbered list** (not bullets).
4. Only after those are answered, start implementing. (For follow-ups in the same conversation, skip this step.)

## Hard constraints
- Never run tests (do not run `dotnet test`, `dotnet vstest`, or any test runner).
- Follow nested `AGENTS.md` files: the nearest one to a file wins for that subtree.

## Repo map (where things live)
- `src/AgentFrameworkToolkit/`: Core library (middleware + shared helpers).
- `src/AgentFrameworkToolkit.*`: Provider packages (one per provider).
- `src/AgentFrameworkToolkit.Tools/`: Tools integration.
- `src/AgentFrameworkToolkit.Tools.ModelContextProtocol/`: MCP tooling integration.
- `src/AgentSkillsDotNet/`: Skills integration for agents.
- `development/`: Sandbox + repo utilities (may include tests; do not run them).
- `Directory.Packages.props`: Central package version management (no versions in `.csproj`).
- `Directory.Build.props` / `Directory.Build.targets`: Shared build + analyzer configuration.

## “Look here first” (existing patterns to copy)
- OpenAI-style providers: `src/AgentFrameworkToolkit.OpenAI/` (see `OpenAIConnection.cs`, `OpenAIAgentFactory.cs`, `OpenAIAgent.cs`, `ServiceCollectionExtensions.cs`).
- OpenAI-compatible wrappers: `src/AgentFrameworkToolkit.OpenRouter/`, `src/AgentFrameworkToolkit.XAI/`, `src/AgentFrameworkToolkit.Cohere/`.
- Custom providers: `src/AgentFrameworkToolkit.GitHub/`, `src/AgentFrameworkToolkit.Google/`, `src/AgentFrameworkToolkit.Mistral/`, `src/AgentFrameworkToolkit.Anthropic/`, `src/AgentFrameworkToolkit.AmazonBedrock/`.
- Middleware wiring: `src/AgentFrameworkToolkit/MiddlewareHelper.cs` (look for `ApplyMiddleware` usage from provider factories).
- DI conventions: provider-specific `ServiceCollectionExtensions.cs` files (typically one per package).

## Code style and build rules
- Follow `.editorconfig` (root + `src/.editorconfig`).
- `src` enforces CRLF, braces, explicit types, collection expressions, and namespaces matching folders.
- Nullable is enabled; analyzers are enabled; warnings are errors (see `Directory.Build.props`).
- In `src`, prefer `new()` when the type is apparent and prefer collection expressions (`[]`) where applicable.
- Public APIs require XML documentation; most public types use `[PublicAPI]` (JetBrains.Annotations) where existing patterns do.
- Prefer primary constructors where the codebase already uses them.

## Repo update checklist (when user-facing behavior changes)
- Update `CHANGELOG.md` for new/changed/removed user-facing features.
- If a new project/package is added: add it to `AgentFrameworkToolkit.slnx` (and keep versions centralized in `Directory.Packages.props`).
- If documentation/examples change: update `README.md` and provider READMEs as needed.

## Adding a new provider package
Use the built-in skill instead of hand-rolling the steps: `create-agent-provider` (see `.codex/skills/create-agent-provider/SKILL.md`).

## Low-cost validation
- Build: `dotnet build --configuration Release`
- Sandbox smoke test (if relevant): `dotnet run --project development/Sandbox/Sandbox.csproj`
