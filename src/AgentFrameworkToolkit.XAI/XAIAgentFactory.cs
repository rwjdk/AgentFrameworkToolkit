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
            Endpoint = XAIConnection.DefaultEndpoint
        });
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public XAIAgentFactory(XAIConnection connection)
    {
        connection.Endpoint ??= XAIConnection.DefaultEndpoint;
        _openAIAgentFactory = new OpenAIAgentFactory(connection);
    }


    /// <summary>
    /// Create a simple Agent (using the ChatClient) with default settings (For more advanced agents use the options overloads)
    /// </summary>
    /// <param name="model">Name of the Model to use</param>
    /// <param name="instructions">Instructions for the Agent to follow (aka Developer Message)</param>
    /// <param name="name">Name of the Agent</param>
    /// <param name="tools">Tools for the Agent</param>
    /// <returns>An Agent</returns>
    public XAIAgent CreateAgent(string model, string? instructions = null, string? name = null, IList<AITool>? tools = null)
    {
        return CreateAgent(new AgentOptions
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
    public XAIAgent CreateAgent(AgentOptions options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public XAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithoutReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public XAIAgent CreateAgent(OpenAIAgentOptionsForResponseApiWithReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public XAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithoutReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }

    /// <summary>
    /// Create a new Agent
    /// </summary>
    /// <param name="options">Options for the agent</param>
    /// <returns>The Agent</returns>
    [Obsolete("Use 'AgentOptions' variant instead (This method will be remove 1st of January 2026)")]
    public XAIAgent CreateAgent(OpenAIAgentOptionsForChatClientWithReasoning options)
    {
        return new XAIAgent(_openAIAgentFactory.CreateAgent(options));
    }
}
