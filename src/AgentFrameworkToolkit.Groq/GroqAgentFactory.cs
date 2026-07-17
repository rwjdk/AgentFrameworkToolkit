using AgentFrameworkToolkit.OpenAI;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Groq;

/// <summary>
/// Factory for creating Groq agents.
/// </summary>
[PublicAPI]
public class GroqAgentFactory
{
    private readonly OpenAIAgentFactory _openAIAgentFactory;

    /// <summary>
    /// Connection.
    /// </summary>
    public GroqConnection Connection { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="apiKey">Your Groq API key (for advanced configuration use the connection overload).</param>
    public GroqAgentFactory(string apiKey)
    {
        Connection = new GroqConnection
        {
            ApiKey = apiKey,
            Endpoint = GroqConnection.DefaultEndpoint
        };

        _openAIAgentFactory = new OpenAIAgentFactory(Connection);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="connection">Connection details.</param>
    public GroqAgentFactory(GroqConnection connection)
    {
        connection.Endpoint ??= GroqConnection.DefaultEndpoint;
        Connection = connection;
        _openAIAgentFactory = new OpenAIAgentFactory(connection);
    }

    /// <summary>
    /// Create a simple agent with default settings (for advanced agents use the options overload).
    /// </summary>
    /// <param name="model">Name of the model to use.</param>
    /// <param name="instructions">Instructions for the agent to follow (also known as the developer message).</param>
    /// <param name="name">Name of the agent.</param>
    /// <param name="tools">Tools for the agent.</param>
    /// <returns>An agent.</returns>
    public GroqAgent CreateAgent(string model, string? instructions = null, string? name = null, IList<AITool>? tools = null)
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
    /// Create a new agent.
    /// </summary>
    /// <param name="options">Options for the agent.</param>
    /// <returns>The agent.</returns>
    public GroqAgent CreateAgent(AgentOptions options)
    {
        return new GroqAgent(_openAIAgentFactory.CreateAgent(options));
    }
}
