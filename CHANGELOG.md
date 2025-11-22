# Changelog - Agent Framework Toolkit

## Version 1.0.0-preview.251121.3
- [BREAKING] Moved NetworkTimeout from Request to Connection as it makes more sense (might introduce a per agent override if needed in the future)
- Added Dependency Injection Methods for all the AgentFactories
- Added Extension Method for AIAgent to have .RunAsync<>(...) for Structured Output

## Version 1.0.0-preview.251121.2
- Added Mistral
- Gave each package its own Description

---

## Version 1.0.0-preview.251121.1
- Added Samples to README.md
- Moved to central nuget-props + adopted same versioning as Microsoft Agent Framework
- Added Simplified Agent Factory Constructors
- Added various simplified CreateAgent (XAI, OpenAI, AzureOpenAI)

---
