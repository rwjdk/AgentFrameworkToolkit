# Contributing to AgentFramework Toolkit

Thank you for your interest in contributing! This document provides guidelines and instructions for contributors.

## Repository Structure

```
AgentFrameworkToolkit/
├── src/                      # Library packages and tests
│   ├── AgentFrameworkToolkit/              # Core library
│   ├── AgentFrameworkToolkit.*/            # Provider-specific packages
│   ├── AgentFrameworkToolkit.Tests/        # Unit tests
│   ├── Directory.Packages.props            # Central package version management
│   ├── Directory.Build.props               # Shared build properties
│   ├── Directory.Build.targets             # Shared build targets
│   └── nuget-package.props                 # NuGet package metadata
│
├── examples/Samples/         # Example console application
├── tools/                    # Development tools
│   ├── AppHost/             # Aspire orchestration host
│   ├── DevUI/               # Web UI for testing agents
│   └── ServiceDefaults/     # Shared service configuration
│
├── README.md                 # Package documentation (goes to NuGet)
├── CHANGELOG.md              # Version history
└── CONTRIBUTING.md           # This file
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for tools/DevUI and tools/AppHost)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/)
- Optional: [Node.js 24+](https://nodejs.org/) (for AppHost)

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

All package versions are centrally managed in `src/Directory.Packages.props`. When adding or updating a package:

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
- **`src/Directory.Build.props`**: Shared settings (TargetFramework, Nullable, TreatWarningsAsErrors, etc.)
- **`src/Directory.Build.targets`**: Packaging standards (SourceLink, symbols, package validation)
- **`src/nuget-package.props`**: NuGet metadata (authors, license, icon, README)

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

       <Import Project="..\nuget-package.props" />

       <ItemGroup>
           <PackageReference Include="NewProvider.SDK" />
       </ItemGroup>

       <ItemGroup>
           <ProjectReference Include="..\AgentFrameworkToolkit\AgentFrameworkToolkit.csproj" />
       </ItemGroup>
   </Project>
   ```

3. Add package version to `src/Directory.Packages.props`

4. Add the project to `AgentFrameworkToolkit.slnx` under the `/Packages/` folder

### Running Development Tools

#### DevUI (Web Interface for Testing Agents)

```bash
cd tools/DevUI
dotnet run
```

Configure API keys in `appsettings.Development.json` or use User Secrets:

```bash
dotnet user-secrets set "OpenAIApiKey" "your-key-here"
dotnet user-secrets set "AnthropicApiKey" "your-key-here"
```

#### AppHost (Aspire Orchestrator)

```bash
cd tools/AppHost
dotnet run
```

This starts the DevUI and orchestrates the development environment.

## Coding Standards

- **C# Version**: Latest (defined in `Directory.Build.props`)
- **Nullable Reference Types**: Enabled
- **Warnings as Errors**: Enabled (`TreatWarningsAsErrors=true`)
- **Code Analysis**: Enabled with `AnalysisLevel=latest`
- **EditorConfig**: Follow the rules defined in `.editorconfig`, `src/.editorconfig`, `examples/.editorconfig`, and `tools/.editorconfig`

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

All packages are versioned together (coordinated releases). Version is defined in `src/nuget-package.props`:

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

- **Unit Tests**: Located in `src/AgentFrameworkToolkit.Tests/`
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

1. Update version in `src/nuget-package.props`
2. Update `CHANGELOG.md` with changes
3. Create PR with version bump
4. After merge, CI/CD will:
   - Build all packages
   - Run tests
   - Push to NuGet (if configured)

## Questions or Issues?

- **Bugs**: [Open an issue](https://github.com/rwjdk/AgentFrameworkToolkit/issues/new)
- **Questions**: [Start a discussion](https://github.com/rwjdk/AgentFrameworkToolkit/discussions)

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
