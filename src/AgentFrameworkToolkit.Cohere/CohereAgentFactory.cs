using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Cohere;

/// <summary>
/// Factory for creating Cohere Agents
/// </summary>
[PublicAPI]
public class CohereAgentFactory
{
    private readonly OpenAIAgentFactory _openAIAgentFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your Cohere API Key (if you need a more advanced connection use the constructor overload)</param>
    public CohereAgentFactory(string apiKey)
    {
        _openAIAgentFactory = new OpenAIAgentFactory(new OpenAIConnection
        {
            ApiKey = apiKey,
            Endpoint = CohereConnection.DefaultEndpoint
        });
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public CohereAgentFactory(CohereConnection connection)
    {
        connection.Endpoint ??= CohereConnection.DefaultEndpoint;
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
    public CohereAgent CreateAgent(string model, string? instructions = null, string? name = null, IList<AITool>? tools = null)
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
    public CohereAgent CreateAgent(AgentOptions options)
    {
        return new CohereAgent(_openAIAgentFactory.CreateAgent(options));
    }
}