using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Cerebras;

/// <summary>
/// Factory for creating Cerebras Agents
/// </summary>
[PublicAPI]
public class CerebrasAgentFactory
{
    private readonly OpenAIAgentFactory _openAIAgentFactory;

    /// <summary>
    /// Connection
    /// </summary>
    public CerebrasConnection Connection { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Your Cerebras API Key (if you need a more advanced connection use the constructor overload)</param>
    public CerebrasAgentFactory(string apiKey)
    {
        Connection = new CerebrasConnection
        {
            ApiKey = apiKey,
            Endpoint = CerebrasConnection.DefaultEndpoint
        };

        _openAIAgentFactory = new OpenAIAgentFactory(Connection);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connection">Connection Details</param>
    public CerebrasAgentFactory(CerebrasConnection connection)
    {
        connection.Endpoint ??= CerebrasConnection.DefaultEndpoint;
        Connection = connection;
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
    public CerebrasAgent CreateAgent(string model, string? instructions = null, string? name = null, IList<AITool>? tools = null)
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
    public CerebrasAgent CreateAgent(AgentOptions options)
    {
        return new CerebrasAgent(_openAIAgentFactory.CreateAgent(options));
    }
}
