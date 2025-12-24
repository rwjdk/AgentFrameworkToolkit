# Contributing to AgentFramework Toolkit

Thank you for your interest in contributing! This document provides guidelines and instructions for contributors.

## Repository Structure

```
AgentFrameworkToolkit/
├── src/                                # Library packages
│   ├── AgentFrameworkToolkit/          # Core library
│   └── AgentFrameworkToolkit.*/        # Provider-specific packages
├── development/                        # Development resoures and tests
│   ├── Sandbox/                        # Sandbox area for various testing while coding
│   ├── Secrets/                        # UserSecrets LLM API Keys etc.
│   └── Tests/                          # Unit tests
├-─ nuget-package.props                 # NuGet package metadata
├── Directory.Packages.props            # Central package version management
├── Directory.Build.props               # Shared build properties
├── Directory.Build.targets             # Shared build targets
├── README.md                           # Package documentation (goes to NuGet)
├── CHANGELOG.md                        # Version history
└── CONTRIBUTING.md                     # This file
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later (core libraries)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (development utilities in the `development/` folder target net10.0)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/rwjdk/AgentFrameworkToolkit.git
cd AgentFrameworkToolkit
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build --configuration Release
```

### 4. Run Tests

```bash
dotnet test --configuration Release
```

## Development Workflow

### Central Package Management

All package versions are centrally managed in `Directory.Packages.props`. When adding or updating a package:

1. Add/update the version in `Directory.Packages.props`:
   ```xml
   <PackageVersion Include="PackageName" Version="1.2.3" />
   ```

2. Reference it in project files **without** version:
   ```xml
   <PackageReference Include="PackageName" />
   ```

### Build Configuration

Common build properties are defined in:
- **`Directory.Build.props`**: Shared settings (TargetFramework, Nullable, TreatWarningsAsErrors, etc.)
- **`Directory.Build.targets`**: Packaging standards (SourceLink, symbols, package validation)
- **`nuget-package.props`**: NuGet metadata (authors, license, icon, README)

### Adding a New Provider Package

1. Create a new project under `src/`:
   ```bash
   dotnet new classlib -n AgentFrameworkToolkit.NewProvider -o src/AgentFrameworkToolkit.NewProvider
   ```

2. Update the `.csproj` file:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
       <PropertyGroup>
           <Description>An opinionated C# Toolkit targeting NewProvider for Microsoft Agent Framework that makes life easier</Description>
       </PropertyGroup>

       <Import Project="..\..\nuget-package.props" />

       <ItemGroup>
           <PackageReference Include="NewProvider.SDK" />
       </ItemGroup>

       <ItemGroup>
           <ProjectReference Include="..\AgentFrameworkToolkit\AgentFrameworkToolkit.csproj" />
       </ItemGroup>
   </Project>
   ```

3. Add the package version to `Directory.Packages.props`

4. Add the project to `AgentFrameworkToolkit.slnx` under the `/Packages/` folder

### Running Development Tools

#### Sandbox (Console Playground)

```bash
dotnet run --project development/Sandbox/Sandbox.csproj
```

The sandbox references every provider package so you can experiment with agent flows locally. Configure API keys via `dotnet user-secrets` or the `development/Secrets` utility below before running it.

#### Secrets Utility

```bash
dotnet run --project development/Secrets/Secrets.csproj
```

Use this helper to manage local secrets (API keys, endpoints, etc.) that both the sandbox and tests consume.

## Coding Standards

- **C# Version**: Latest (defined in `Directory.Build.props`)
- **Nullable Reference Types**: Enabled
- **Warnings as Errors**: Enabled (`TreatWarningsAsErrors=true`)
- **Code Analysis**: Enabled with `AnalysisLevel=latest`
- **EditorConfig**: Follow the rules defined in `.editorconfig` (repo root) and `src/.editorconfig`

### Naming Conventions

- Classes: `PascalCase`
- Methods: `PascalCase`
- Parameters: `camelCase`
- Private fields: `_camelCase` (with underscore)
- Constants: `PascalCase`

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Creates a new agent with the specified options.
/// </summary>
/// <param name="options">The agent configuration options.</param>
/// <returns>A configured AI agent instance.</returns>
public OpenAIAgent CreateAgent(OpenAIAgentOptions options)
{
    // ...
}
```

## Pull Request Guidelines

1. **Branch Naming**:
   - Features: `feature/description`
   - Bug fixes: `fix/description`
   - Chores: `chore/description`

2. **Commit Messages**:
   - Use conventional commits: `type: description`
   - Types: `feat`, `fix`, `docs`, `chore`, `refactor`, `test`, `style`
   - Example: `feat: add support for streaming responses`

3. **PR Description**:
   - Clearly describe what the PR does
   - Reference related issues (e.g., `Fixes #123`)
   - Include examples if adding new features

4. **Before Submitting**:
   - Ensure all tests pass: `dotnet test`
   - Build succeeds: `dotnet build --configuration Release`
   - No warnings or errors
   - Code follows existing patterns

## Versioning

All packages are versioned together (coordinated releases). Version is defined in `nuget-package.props`:

```xml
<PackageVersion>1.0.0-preview.251217.1</PackageVersion>
```

Version format: `MAJOR.MINOR.PATCH-preview.YYMMDD.BUILD`

## Changelog

When making changes that affect users, update `CHANGELOG.md`:

```markdown
## Version X.Y.Z-preview.YYMMDD.BUILD
- Added: New feature description
- Fixed: Bug fix description
- [BREAKING]: Breaking change description
```

## Testing

- **Unit Tests**: Located in `development/Tests/`
- **How to run**: `dotnet test development/Tests/Tests.csproj --configuration Release` (or run `dotnet test AgentFrameworkToolkit.sln`)
- **Test Framework**: xUnit v3
- **Coverage**: Aim for high coverage on public APIs
- **Naming**: `MethodName_Scenario_ExpectedBehavior`

Example:
```csharp
[Fact]
public void CreateAgent_WithValidOptions_ReturnsAgent()
{
    // Arrange
    var factory = new OpenAIAgentFactory("api-key");
    var options = new OpenAIAgentOptions { Model = "gpt-4" };

    // Act
    var agent = factory.CreateAgent(options);

    // Assert
    Assert.NotNull(agent);
}
```

## Release Process

1. Update version in `nuget-package.props`
2. Update `CHANGELOG.md` with changes
3. Create PR with version bump
4. After merge, CI/CD will:
   - Build all packages
   - Run tests
   - Push to NuGet (if configured)

## Continuous Integration

- The GitHub Actions workflow defined in `.github/workflows/Build.yml` executes the restore/build/test/pack sequence for every PR and push.
- Keep your local verification steps aligned with that workflow so CI stays green.

## Questions or Issues?

- **Bugs**: [Open an issue](https://github.com/rwjdk/AgentFrameworkToolkit/issues/new)
- **Questions**: [Start a discussion](https://github.com/rwjdk/AgentFrameworkToolkit/discussions)

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
