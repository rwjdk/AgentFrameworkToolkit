using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

#pragma warning disable OPENAI001

namespace Samples.Providers;

public static class OpenAI
{
    public static async Task RunAsync()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration();
        OpenAIAgentFactory factory = new(configuration.OpenAiApiKey);

        OpenAIAgent agent = factory.CreateAgent(new AgentOptions()
        {
            Model = OpenAIChatModels.Gpt5Mini,
            MaxOutputTokens = 2000,
            RawHttpCallDetails = details => { Console.WriteLine(details.RequestData); },
            AdditionalChatClientAgentOptions = options =>
            {
                options.ChatOptions ??= new ChatOptions();
                options.ChatOptions.WithOpenAIChatClientReasoning(ChatReasoningEffortLevel.High);
            }
        });

        AgentRunResponse response = await agent.RunAsync("Hello");
        Console.WriteLine(response);
    }
}