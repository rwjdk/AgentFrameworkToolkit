using AgentFrameworkToolkit;
using AgentFrameworkToolkit.GitHub;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Secrets;

#pragma warning disable OPENAI001

namespace Samples.Providers;

public static class GitHub
{
    public static async Task Run()
    {
        Secrets.Secrets secrets = SecretsManager.GetSecrets();
        GitHubAgentFactory factory = new(secrets.GitHubPatToken);

        //Create your AgentFactory (using a connection object for more options)
        GitHubAgentFactory agentFactory = new GitHubAgentFactory(new GitHubConnection
        {
            //Endpoint = "<endpoint>", //Optional: if targeting non-GitHub provider
            AccessToken = "<Access Token>",
            NetworkTimeout = TimeSpan.FromMinutes(5), //Set call timeout
            AdditionalAzureAIInferenceClientOptions = options =>
            {
                //Set additional properties if needed
            }
        });

        //Create your Agent
        GitHubAgent agent = agentFactory.CreateAgent(new GitHubAgentOptions
        {
            //Mandatory
            Model = "gpt-5", //Model to use

            //Optional (Common)
            Name = "MyAgent", //Agent Name
            Temperature = 0, //The Temperature of the LLM Call (1 = Normal; 0 = Less creativity) [ONLY NON-REASONING MODELS]
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
        });

        AgentRunResponse response = await agent.RunAsync("Hello World");
        Console.WriteLine(response);
    }
}
