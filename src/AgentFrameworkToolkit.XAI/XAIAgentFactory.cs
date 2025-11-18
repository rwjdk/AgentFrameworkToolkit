using AgentFrameworkToolkit.OpenAI;

namespace AgentFrameworkToolkit.XAI;

public class XAIAgentFactory(XAIConnection connection)
{
    private readonly OpenAIAgentFactory _openAIAgentFactory = new(new OpenAIConnection
    {
        ApiKey = connection.ApiKey,
        AdditionalOpenAIClientOptions = connection.AdditionalOpenAIClientOptions,
        Endpoint = "https://api.x.ai/v1"
    });

    public XAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithoutReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    public XAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    public XAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithoutReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    public XAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }
}