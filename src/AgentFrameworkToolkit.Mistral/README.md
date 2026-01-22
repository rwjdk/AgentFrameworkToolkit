# Agent Framework Toolkit @ Mistral

> This package is aimed at Mistral as an LLM Provider. Check out the [General README.md](https://github.com/rwjdk/AgentFrameworkToolkit/blob/main/README.md) for other providers and shared features in Agent Framework Toolkit.

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

1. Install the 'AgentFrameworkToolkit.Mistral' NuGet Package (`dotnet add package AgentFrameworkToolkit.Mistral`)
2. Get your [Mistral API Key](https://admin.mistral.ai/organization/api-keys)
3. Create an `MistralAgentFactory` instance (Namespace: AgentFrameworkToolkit.Mistral)
4. Use instance to create your `MistralAgent` (which is a regular Microsoft Agent Framework `AIAgent` behind the scenes)

### Minimal Code Example
```cs
//Create your AgentFactory
MistralAgentFactory agentFactory = new MistralAgentFactory("<apiKey>");

//Create your Agent
AIAgent agent = agentFactory.CreateAgent("mistral-small-latest");

AgentResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response);
```

### Normal Code Example
```cs
//Create your AgentFactory
MistralAgentFactory agentFactory = new MistralAgentFactory("<apiKey>");

//Create your Agent
MistralAgent agent = agentFactory.CreateAgent(new MistralAgentOptions //Use MistralAgentOptions overload to access more options
{
    Model = "mistral-small-latest",
    Instructions = "You are a nice AI", //The System Prompt
    Tools = [], //Add your tools here
});

AgentResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response);
```

### Full Code example with ALL options
```cs
//Create your AgentFactory (using a connection object for more options)
MistralAgentFactory agentFactory = new MistralAgentFactory(new MistralConnection
{
    ApiKey = "<apiKey>",
    NetworkTimeout = TimeSpan.FromMinutes(5) //Set call timeout
});

//Create your Agent
MistralAgent agent = agentFactory.CreateAgent(new MistralAgentOptions
{
    //Mandatory
    Model = "mistral-small-latest", //Model to use
    
    //Optional (Common)
    Name = "MyAgent", //Agent Name
    MaxOutputTokens = 2000, //Max allow token
    Temperature = 0, //The Temperature of the LLM Call (1 = Normal; 0 = Less creativity)
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
    ChatMessageStoreFactory = context => new MyChatMessageStore(), //Set a custom message store
    AIContextProviderFactory = context => new MyAIContextProvider(), //Set a custom AI context provider
    AdditionalChatClientAgentOptions = options =>
    {
        //Option to set even more options if not covered by AgentFrameworkToolkit
    },
    RawToolCallDetails = Console.WriteLine, //Raw Tool calling Middleware (if you just wish to log what tools are being called. ToolCallingMiddleware is a more advanced version of this)
    RawHttpCallDetails = details => //Intercept the raw HTTP Call to the LLM (great for advanced debugging sessions)
    {
        Console.WriteLine(details.RequestUrl);
        Console.WriteLine(details.RequestData);
        Console.WriteLine(details.ResponseData);
    }
});

AgentResponse response = await agent.RunAsync("Hello World");
Console.WriteLine(response);
```
