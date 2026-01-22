# Agent Framework Toolkit @ Tools

> This package is aimed at making it eaiser to consume AI Tools in [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)

Check out the [General README.md](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/README.md) for Agentfactory providers and other shared features in Agent Framework Toolkit.

## Samples
```cs
//1. Make your tool-class and add [AITool] attributes

public class MyTools
{
    [AITool]
    public string MyTool1()
    {
        return "hello";
    }

    [AITool]
    public string MyTool2()
    {
        return "world";
    }
}

//2. Get your tool by either instance or Type (if not contructor dependencies)

IList<AITool> tools = aiToolsFactory.GetTools(typeof(MyTools));
//or
IList<AITool> tools = aiToolsFactory.GetTools(new MyTools());
```
```
