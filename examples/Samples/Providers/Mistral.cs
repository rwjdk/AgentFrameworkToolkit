using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Mistral;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

namespace Samples.Providers;

public static class Mistral
{
    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
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

        AgentRunResponse response = await agent.RunAsync("Hello World");
        Console.WriteLine(response);
    }
}
