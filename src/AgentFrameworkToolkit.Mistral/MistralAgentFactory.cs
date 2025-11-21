using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mistral.SDK;

namespace AgentFrameworkToolkit.Mistral;

public class MistralAgentFactory
{
    private readonly MistralConnection _connection;

    public MistralAgentFactory(string apiKey)
    {
        _connection = new MistralConnection
        {
            ApiKey = apiKey
        };
    }

    public MistralAgentFactory(MistralConnection connection)
    {
        _connection = connection;
    }

    public MistralAgent CreateAgent(MistralAgentOptions options)
    {
        IChatClient client = GetClient(options);

        AIAgent innerAgent = new ChatClientAgent(client, CreateChatClientAgentOptions(options));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (options.RawToolCallDetails != null)
        {
            return new MistralAgent(options.ApplyMiddleware(innerAgent));
        }

        return new MistralAgent(innerAgent);
    }

    private static ChatClientAgentOptions CreateChatClientAgentOptions(MistralAgentOptions options)
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


    private IChatClient GetClient(MistralAgentOptions options)
    {
        HttpClient? httpClient = null;

        // ReSharper disable once InvertIf
        if (options.RawHttpCallDetails != null)
        {
            httpClient = new(new RawCallDetailsHttpHandler(options.RawHttpCallDetails)); //todo - antipattern to new up a new httpClient Here
        }

        if (_connection.NetworkTimeout.HasValue)
        {
            httpClient ??= new HttpClient();
            httpClient.Timeout = _connection.NetworkTimeout.Value;
        }

        MistralClient mistralClient = new(new APIAuthentication(_connection.ApiKey), httpClient);
        return mistralClient.Completions;
    }
}