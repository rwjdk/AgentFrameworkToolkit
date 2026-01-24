# Agent Framework Toolkit @ Model Context Protocol

> This package is aimed at making it easier to consume MCP Servers as AI Tools in [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)

Check out the [General README.md](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/README.md) for Agentfactory providers and other shared features in Agent Framework Toolkit.

## Samples

```cs
AIToolsFactory aiToolsFactory = new AIToolsFactory(); //Dependency Injection version: builder.Services.AddAIToolsFactory();

//Either remote MCP Server
await using McpClientTools remoteMcpClient = await aiToolsFactory.GetToolsFromRemoteMcpAsync("http://mcp.relewise.com");
IList<AITool> remoteMcpTools = remoteMcpClient.Tools;

//Or local MCP Server
await using McpClientTools localMcpClient = await aiToolsFactory.GetToolsFromLocalMcpAsync("npx", ["@playwright/mcp@latest"]);
IList<AITool> localMcpTools = remoteMcpClient.Tools;

//Ready to use in your Agent Framework Toolkit Agents or regular AIAgents
```
