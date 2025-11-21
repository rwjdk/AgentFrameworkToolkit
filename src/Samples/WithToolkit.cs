using AgentFrameworkToolkit.Anthropic;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.Mistral;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.XAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Shared;

#pragma warning disable OPENAI001

namespace Samples;

public class WithToolkit
{
    public static async Task Run()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();

        MistralAgentFactory mistralAgentFactory = new MistralAgentFactory(configuration.MistralApiKey);
        MistralAgent mistralAgent = mistralAgentFactory.CreateAgent(new MistralAgentOptions
        {
            DeploymentModelName = Mistral.SDK.ModelDefinitions.MistralSmall
        });

        AgentRunResponse runResponse = await mistralAgent.RunAsync("Hello");

        AzureOpenAIAgentFactory azureOpenAIAgentFactory = new(new AzureOpenAIConnection
        {
            Endpoint = configuration.AzureOpenAiEndpoint,
            ApiKey = configuration.AzureOpenAiKey
        });

        AzureOpenAIAgent commonAgent = azureOpenAIAgentFactory.CreateAgent(new OpenAIAgentOptionsForResponseApiWithReasoning
        {
            DeploymentModelName = "gpt-5-nano",
            ReasoningEffort = ResponseReasoningEffortLevel.Low,
            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
            Tools = [AIFunctionFactory.Create(GetWeather)],
            AdditionalChatClientAgentOptions = options => { options.Name = "NO!"; }
        });


        AgentRunResponse agentRunResponse = await commonAgent.RunAsync("What is the weather like in Paris?");

        UsageDetails usageDetails = agentRunResponse.Usage!;
        long? a = usageDetails.InputTokenCount;
        long? b = usageDetails.OutputTokenCount;


        ChatClientAgentRunResponse<Weather> commonResponse = await commonAgent.RunAsync<Weather>("What is the weather like in Paris?");
        Weather commonWeather = commonResponse.Result;


        bool addTool = false;

        AnthropicAgent anthropicAgent = GetAnthropicAgent();

        AIAgent[] agents =
        [
            GetGrokAgent(),
            GetAnthropicAgent(),
            GetGoogleAgent(),
            GetAzureOpenAIAgent(),
            GetOpenAIAgent()
        ];

        foreach (AIAgent agent in agents)
        {
            try
            {
                //Normal
                AgentRunResponse response1 = await agent.RunAsync("What is the capital of France?");
                Console.WriteLine(response1);
                /*
                //Streaming
                await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("Hello Again"))
                {
                    Console.Write(update);
                }

                Console.WriteLine();

                //Normal Tool Call
                AgentRunResponse response2 = await agent.RunAsync("What is the Weather like in Paris?");
                Console.WriteLine(response2);

                //Tool Call Streaming
                await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("What is the Weather like in Paris?"))
                {
                    Console.Write(update);
                }

                Console.WriteLine();

                //Structured output
                ChatClientAgentRunResponse<Weather> response3 = await agent.RunAsync<Weather>("What is the Weather like in Paris?");
                Console.WriteLine(response3.Result.City);*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        /*


        AgentRunResponse response = await agent.RunAsync("What is the weather like in paris?");
        */


        AzureOpenAIAgent fullBlownAgent = azureOpenAIAgentFactory.CreateAgent(new OpenAIAgentOptionsForResponseApiWithReasoning
        {
            Id = "1234",
            Name = "MyAgent",
            Description = "The description of my agent",
            Instructions = "Speak like a pirate",
            DeploymentModelName = "gpt-5-mini",
            ReasoningEffort = ResponseReasoningEffortLevel.Low,
            ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Detailed,
            Tools = [AIFunctionFactory.Create(GetWeather)],
            RawToolCallDetails = details => { Console.WriteLine(details.ToString()); },
            RawHttpCallDetails = details =>
            {
                Console.WriteLine($"URL: {details.RequestUrl}");
                Console.WriteLine($"Request: {details.RequestJson}");
                Console.WriteLine($"Response: {details.ResponseJson}");
            }
        });

        ChatClientAgentRunResponse<Weather> fullBlownResponse = await fullBlownAgent.RunAsync<Weather>("What is the weather like in Paris?");
        Weather fullBlownResponseWeather = fullBlownResponse.Result;

        AzureOpenAIAgent GetAzureOpenAIAgent()
        {
            AzureOpenAIAgentFactory factory = new(new AzureOpenAIConnection
            {
                Endpoint = configuration.AzureOpenAiEndpoint,
                ApiKey = configuration.AzureOpenAiKey,
            });

            AzureOpenAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
            {
                DeploymentModelName = "gpt-4.1-mini",
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        OpenAIAgent GetOpenAIAgent()
        {
            OpenAIAgentFactory factory = new(new OpenAIConnection
            {
                ApiKey = configuration.OpenAiApiKey
            });

            OpenAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
            {
                DeploymentModelName = "gpt-4.1-mini",
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        XAIAgent GetGrokAgent()
        {
            XAIAgentFactory factory = new(new XAIConnection
            {
                ApiKey = configuration.XAiGrokApiKey
            });

            XAIAgent agent = factory.CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
            {
                DeploymentModelName = "grok-4-fast-non-reasoning",
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        AnthropicAgent GetAnthropicAgent()
        {
            AnthropicAgentFactory factory = new(new AnthropicConnection
            {
                ApiKey = configuration.AnthropicApiKey
            });

            AnthropicAgent agent = factory.CreateAgent(new AnthropicAgentOptions
            {
                DeploymentModelName = "claude-sonnet-4-5-20250929",
                MaxOutputTokens = 1000,
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : []
            });
            return agent;
        }

        GoogleAgent GetGoogleAgent()
        {
            GoogleAgentFactory factory = new(new GoogleConnection
            {
                ApiKey = configuration.GoogleGeminiApiKey
            });

            GoogleAgent agent = factory.CreateAgent(new GoogleAgentOptions
            {
                DeploymentModelName = GenerativeAI.GoogleAIModels.Gemini25Pro,
                Tools = addTool ? [AIFunctionFactory.Create(GetWeather)] : [],
            });
            return agent;
        }
    }

    public static string GetWeather(string city)
    {
        return "It is sunny";
    }

    public class Weather
    {
        public required string City { get; set; }
        public required int DegreesCelsius { get; set; }
        public required int DegreesFahrenheit { get; set; }
    }
}