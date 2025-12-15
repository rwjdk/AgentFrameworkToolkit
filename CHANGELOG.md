# Changelog - Agent Framework Toolkit

## Version 1.0.0-preview.251215.1
- Added option to get Raw Client from the various Connection Objects (Except for the Google Connection as switch to the new official Google Nuget is expected soon)
- OpenRouter and XAI Connection are now inherited from OpenAI Connections

---

## Version 1.0.0-preview.251204.1
- Bumped `Microsoft.Agents.AI` version to latest (1.0.0-preview.251204.1) to be compatible with [latest breaking change around Instructions](https://github.com/microsoft/agent-framework/pull/1517)

---

## Version 1.0.0-preview.251201.1
- AzureOpenAI: Added RBAC Support (TokenCredentials)
- Enabled "Treat Warnings as Errors"
- Added .editorconfig 
- Removed all todo's in the code.

---

## Version 1.0.0-preview.251129.1
- Added GitHub provider NuGet Package
- Everything now have XML Summaries
- [BREAKING] Renamed `DeploymentModelName` to `Model` to make it simpler to understand (Sorry to exiting users, but better now than later)
- [BREAKING] Renamed `RequestJson` and `ResponseJson` to `RequestData` and `ResponseData` as not all LLMs you JSON for communication (example Anthropic Data back is not JSON)
- OpenAI: Added `WithOpenAIResponsesApiReasoning` and `WithOpenAIChatClientReasoning` Extension Methods for `ChatOptions` (if you do not wish to use AgentFactory, but still wish to have an easier time to set OpenAI Reasoning)
- Fixed that `ServiceCollectionExtensions` for Google was in the wrong namespace
- Fixed that Mistral had an `AddAnthropicAgentFactory` method (wrong name)
- Fixed that OpenAI ResponseAPI without reasoning Agents did not get their Temperature set

---

## Version 1.0.0-preview.251126.2
- Anthropic: Replaced the unofficial `Anthropic.SDK` nuget package with the official `Microsoft.Agents.AI.Anthropic` nuget package instead.
- [BREAKING] Removed the `UseInterleavedThinking` option form the Anthropic Package, as it actually did not do anything.

---

## Version 1.0.0-preview.251126.1
- Bumped `Microsoft.Agents.AI` version to latest (1.0.0-preview.251125.1)
- Fixed that RawCallDetail would fail if input or output was not JSON (like the output of an Anthropic call)

---

## Version 1.0.0-preview.251123.0
- Added OpenRouter provider NuGet Package
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
- Added Mistral provider NuGet Package
- Gave each package its own Description

---

## Version 1.0.0-preview.251121.1
- Added Samples to README.md
- Moved to central nuget-props + adopted same versioning as Microsoft Agent Framework
- Added Simplified Agent Factory Constructors
- Added various simplified CreateAgent (XAI, OpenAI, AzureOpenAI)

---
