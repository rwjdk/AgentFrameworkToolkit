using AgentFrameworkToolkit;
using AgentFrameworkToolkit.Anthropic;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.ModelContextProtocol;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ModelContextProtocol.Client;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Secrets;

#pragma warning disable OPENAI001

namespace Samples.Providers;

public static class AzureOpenAI
{
    [AITool]
    static string GetWeather()
    {
        return "Sunny";
    }

    public static async Task RunAsync()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();

        //Create your AgentFactory (using a connection object for more options)
        AzureOpenAIAgentFactory agentFactory = new AzureOpenAIAgentFactory(new AzureOpenAIConnection
        {
            Endpoint = "<endpoint>",
            ApiKey = "<apiKey>",
            NetworkTimeout = TimeSpan.FromMinutes(5), //Set call timeout
            Credentials = null, //Set RBAC Credentials
            DefaultClientType = ClientType.ResponsesApi, //Set default Client Type for each agent (ChatClient or ResponsesAPI)
            AdditionalAzureOpenAIClientOptions = options =>
            {
                //Set additional properties if needed
            }
        });

        //Create your Agent
        AzureOpenAIAgent agent = agentFactory.CreateAgent(new AgentOptions
        {
            //Mandatory
            Model = "gpt-5", //Model to use

            //Optional (Common)
            ClientType = ClientType.ChatClient, //Choose ClientType (ChatClient or Responses API)
            Name = "MyAgent", //Agent Name
            Temperature = 0, //The Temperature of the LLM Call (1 = Normal; 0 = Less creativity) [ONLY NON-REASONING MODELS]
            ReasoningEffort = OpenAIReasoningEffort.Low, //Set Reasoning Effort [ONLY REASONING MODELS]
            ReasoningSummaryVerbosity = OpenAIReasoningSummaryVerbosity.Detailed, //Only used in Responses API [ONLY REASONING MODELS]
            Instructions = "You are a nice AI", //The System Prompt for the Agent to Follow
            Tools = [], //Add your tools for Tool Calling here
            ToolCallingMiddleware = async (callingAgent, context, next, token) => //Tool Calling Middleware to Inspect, change, and cancel tool-calling
            {
                AIFunctionArguments arguments = context.Arguments; //Details on the tool-call that is about to happen
                return await next(context, token);
            },
            OpenTelemetryMiddleware = new OpenTelemetryMiddleware(source: "MyOpenTelemetrySource", telemetryAgent => telemetryAgent.EnableSensitiveData = true), //Configure OpenTelemetry Middleware

            //Optional (Rarely used)
            MaxOutputTokens = 2000, //Max allow token
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
            },
            ClientFactory = client =>
            {
                //Interact with the underlying Client-factory
                return client;
            }
        });

        AgentRunResponse response = await agent.RunAsync("Hello World");
        Console.WriteLine(response);
    }
}
