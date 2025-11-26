# Changelog - Agent Framework Toolkit

## Version 1.0.0-preview.251126.1
- Bumped `Microsoft.Agents.AI` version to latest (1.0.0-preview.251125.1)
- Fixed that RawCallDetail would fail if input or output was not JSON (like the output of an Anthropic call)

---

## Version 1.0.0-preview.251123.0
- Added OpenRouter NuGet Package
- [OpenAI] Added `OpenAIChatModels` of the most common models
- [Anthropic] Added `AnthropicChatModels` of the most common models
- [XAI] Added `XAIChatModels` of the most common models
- [Mistral] Added `MistralChatModels` of the most common models
- [Google] Added `GoogleChatModels` of the most common models
- [OpenRouter] Added `OpenRouterChatModels` of the most common models

---

## Version 1.0.0-preview.251121.3
- [BREAKING] Moved NetworkTimeout from Request to Connection as it makes more sense (might introduce a per agent override if needed in the future)
- Added Dependency Injection Methods for all the AgentFactories
- Added Extension Method for AIAgent to have .RunAsync<>(...) for Structured Output

---

## Version 1.0.0-preview.251121.2
- Added Mistral NuGet Package
- Gave each package its own Description

---

## Version 1.0.0-preview.251121.1
- Added Samples to README.md
- Moved to central nuget-props + adopted same versioning as Microsoft Agent Framework
- Added Simplified Agent Factory Constructors
- Added various simplified CreateAgent (XAI, OpenAI, AzureOpenAI)

---
