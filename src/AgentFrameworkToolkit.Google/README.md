# Agent Framework Toolkit @ Google

> This package is aimed at Google as an LLM Provider. Check out the [General README.md](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/README.md) for other providers and shared features in Agent Framework Toolkit.

## What is Agent Framework Toolkit?
Agent Framework Toolkit and an opinionated C# wrapper on top of the [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) that makes various things easier to work with:
- Easier to set advanced Agent Options ([often only needing half or fewer lines of code to do the same things](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/README.md)) that normally would need the Breaking Glass approach.
- Easier [Tools / MCP Tools Definition](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/README.md)

### FAQ

**Q: If I use the Agent Framework Toolkit, does it limit or hinder what I can do with Microsoft Agent Framework?**

A: No, everything you can do with Microsoft Agent Framework can still be done with Agent Framework Toolkit. It is just a wrapper that enables options that are hard to use otherwise

**Q: What is the release frequency of Agent Framework Toolkit (can I always use the latest Microsoft Agent Framework release)?**

A: This NuGet package is released as often (or more) than the Microsoft Agent Framework. At a minimum, it will be bumped to the latest Microsoft Agent Framework Release within a day of official release. It follows the same versioning scheme as AF, so the same or higher version number will always be compatible with the latest release.

**Q: Why are the agents not AIAgent / ChatClientAgents? Are they compatible with the rest of the Microsoft Agent Framework?**

A: The specialized agents in Agent Framework Toolkit are all 100% compatible with AF as they simply inherit from AIAgent


## Getting Started

1. Install the 'AgentFrameworkToolkit.Google' NuGet Package (`dotnet add package AgentFrameworkToolkit.Google`)
2. Get your [Google API Key](https://aistudio.google.com/app/api-keys)
3. Create an `GoogleAgentFactory` instance (Namespace: AgentFrameworkToolkit.Google)
4. Use instance to create your `GoogleAgent` (which is a regular Microsoft Agent Framework `AIAgent` behind the scenes)

### Minimal Code Example
```cs
//Create your AgentFactory
GoogleAgentFactory agentFactory = new GoogleAgentFactory("<apiKey>");

//Create your Agent
AIAgent agent = agentFactory.CreateAgent(model: "gemini-2.5-flash");

AgentRunResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response);
```

### Normal Code Example
```cs
//Create your AgentFactory
GoogleAgentFactory agentFactory = new GoogleAgentFactory("<apiKey>");

//Create your Agent
GoogleAgent agent = agentFactory.CreateAgent(new GoogleAgentOptions //Use GoogleAgentOptions overload to access more options
{
    Model = "gemini-2.5-flash",
    Instructions = "You are a nice AI", //The System Prompt
    Tools = [], //Add your tools here
    BudgetTokens = 1024, //Set Thinking Budget
});

AgentRunResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response);
```

### Full Code example with ALL options
```cs
//Create your AgentFactory (using a connection object for more options)
GoogleAgentFactory agentFactory = new GoogleAgentFactory(new GoogleConnection
{
    ApiKey = "<apiKey>",
});

//Create your Agent
GoogleAgent agent = agentFactory.CreateAgent(new GoogleAgentOptions
{
    //Mandatory
    Model = "gemini-2.5-flash", //Model to use

    //Optional (Common)
    Name = "MyAgent", //Agent Name
    MaxOutputTokens = 2000, //Max allow token
    Temperature = 0, //The Temperature of the LLM Call (1 = Normal; 0 = Less creativity)
    ThinkingBudget = 5000, //Set Thinking Budget
    Instructions = "You are a nice AI", //The System Prompt for the Agent to Follow
    Tools = [], //Add your tools for Tool Calling here
    ToolCallingMiddleware = async (callingAgent, context, next, token) => //Tool Calling Middleware to Inspect, change, and cancel tool-calling
    {
        AIFunctionArguments arguments = context.Arguments; //Details on the tool-call that is about to happen
        return await next(context, token);
    },
    OpenTelemetryMiddleware = new OpenTelemetryMiddleware(source: "MyOpenTelemetrySource", telemetryAgent => telemetryAgent.EnableSensitiveData = true), //Configure OpenTelemetry Middleware

    //Optional (Rarely used)
    Id = "1234", //Set the ID of Agent (else a random GUID is assigned as ID)
    Description = "My Description", //Description of the Agent (not used by the LLM)
    LoggingMiddleware = new LoggingMiddleware( /* Configure custom logging */),
    Services = null, //Setup Tool Calling Service Injection (See https://youtu.be/EGs-Myf5MB4 for more details)
    LoggerFactory = null, //Setup logger Factory (Alternative to Middleware)
    AdditionalChatClientAgentOptions = options =>
    {
        //Option to set even more options if not covered by AgentFrameworkToolkit
    },
    RawToolCallDetails = Console.WriteLine, //Raw Tool calling Middleware (if you just wish to log what tools are being called. ToolCallingMiddleware is a more advanced version of this)
});

AgentRunResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response);
```
