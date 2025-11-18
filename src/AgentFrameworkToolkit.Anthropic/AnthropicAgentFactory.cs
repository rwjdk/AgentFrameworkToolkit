using Anthropic.SDK;
using Anthropic.SDK.Extensions;
using Anthropic.SDK.Messaging;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Anthropic;

public class AnthropicAgentFactory(AnthropicConnection connection)
{
    public AnthropicAgent CreateAgent(AnthropicAgentOptions options)
    {
        IChatClient client = GetClient(options);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new AnthropicAgent(options.ApplyMiddleware(innerAgent));
        }

        return new AnthropicAgent(innerAgent);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(AnthropicAgentOptions options)
    {
        ChatOptions chatOptions = new()
        {
            ModelId = options.DeploymentModelName
        };

        if (options.Tools != null)
        {
            chatOptions.Tools = options.Tools;
        }

        chatOptions.MaxOutputTokens = options.MaxOutputTokens;

        if (options.Temperature != null)
        {
            chatOptions.Temperature = options.Temperature;
        }

        if (options.BudgetTokens > 0 || options.UseInterleavedThinking)
        {
            chatOptions = chatOptions.WithThinking(new ThinkingParameters
            {
                BudgetTokens = options.BudgetTokens,
                UseInterleavedThinking = options.UseInterleavedThinking
            });
        }

        ChatClientAgentOptions chatClientAgentOptions = new()
        {
            Name = options.Name,
            Instructions = options.Instructions,
            Description = options.Description,
            Id = options.Id,
            ChatOptions = chatOptions
        };

        options.AdditionalChatClientAgentOptions?.Invoke(chatClientAgentOptions);

        return chatClientAgentOptions;
    }


    private IChatClient GetClient(AnthropicAgentOptions options)
    {
        HttpClient? httpClient = null;

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            httpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
        }

        if (options.NetworkTimeout.HasValue)
        {
            httpClient ??= new HttpClient();
            httpClient.Timeout = options.NetworkTimeout.Value;
        }

        AnthropicClient anthropicClient = new(new APIAuthentication(connection.ApiKey), httpClient);
        IChatClient client = anthropicClient.Messages.AsBuilder().Build();
        return client;
    }
}