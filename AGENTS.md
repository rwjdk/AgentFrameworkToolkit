# Conversation start rule (MANDATORY)
Whenever a **new conversation** begins, do **not** start coding immediately.

1. Investigate the codebase (read relevant files; search for existing patterns).
2. Identify what’s unclear, risky, or ambiguous about the requested change.
3. Ask the user **3–10 clarifying questions in a numbered list** (not bullets).
4. Only after those are answered, start implementing. (For follow-ups in the same conversation, skip this step.)

# Repo map (where things live)
- `src/AgentFrameworkToolkit/`: Core library (middleware + shared helpers).
- `src/AgentFrameworkToolkit.*`: Provider packages (one per provider).
- `src/AgentFrameworkToolkit.Tools/`: Tools integration.
- `src/AgentFrameworkToolkit.Tools.ModelContextProtocol/`: MCP tooling integration.
- `src/AgentSkillsDotNet/`: Skills integration for agents.
- `development/`: Sandbox + repo utilities (may include tests; do not run them).
- `Directory.Packages.props`: Central package version management (no versions in `.csproj`).
- `Directory.Build.props` / `Directory.Build.targets`: Shared build + analyzer configuration.

# New Provider Guidelines
Use the built-in skill `create-agent-provider` (see `.codex/skills/create-agent-provider/SKILL.md`).

# Code style and build rules
- Follow `.editorconfig` (root + `src/.editorconfig`).
- `src` enforces CRLF, braces, explicit types, collection expressions, and namespaces matching folders.
- Nullable is enabled; analyzers are enabled; warnings are errors (see `Directory.Build.props`).
- In `src`, prefer `new()` when the type is apparent and prefer collection expressions (`[]`) where applicable.
- Public members require XML documentation. Use `[PublicAPI]` (JetBrains.Annotations) where existing patterns do.
- Prefer primary constructors where the codebase already uses them.

## Repo update checklist (when user-facing behavior changes)
- If a new project/package is added: add it to `AgentFrameworkToolkit.slnx` (and keep versions centralized in `Directory.Packages.props`).
- If documentation/examples change: update `README.md` and provider READMEs as needed.
