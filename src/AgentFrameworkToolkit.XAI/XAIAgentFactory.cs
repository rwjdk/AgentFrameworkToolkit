using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.XAI;

/// <summary>
/// Factory for creating X AI Agents
/// </summary>
[PublicAPI]
public class XAIAgentFactory
{
    private const string Endpoint = "https://api.x.ai/v1";
    private readonly OpenAIAgentFactory _openAIAgentFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your XAI API Key (if you need a more advanced connection use the constructor overload)</param>
    public XAIAgentFactory(string apiKey)
    {
        _openAIAgentFactory = new OpenAIAgentFactory(new OpenAIConnection
        {
            ApiKey = apiKey,
            Endpoint = Endpoint
        });
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public XAIAgentFactory(XAIConnection connection)
    {
        _openAIAgentFactory = new OpenAIAgentFactory(new OpenAIConnection
        {
            ApiKey = connection.ApiKey,
            AdditionalOpenAIClientOptions = connection.AdditionalOpenAIClientOptions,
            Endpoint = Endpoint
        });
    }


    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the Model to use</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public XAIAgent CreateAgent(string model, string? instructions = null, string? name = null, AITool[]? tools = null)
    {
        return CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
        {
            Model = model,
            Name = name,
            Instructions = instructions,
            Tools = tools
        });
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public XAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithoutReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public XAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public XAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithoutReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    public XAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }
}