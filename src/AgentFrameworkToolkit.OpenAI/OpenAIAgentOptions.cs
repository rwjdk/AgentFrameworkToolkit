using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Options for an OpenAI Agent
/// </summary>
public abstract class OpenAIAgentOptions
{
    /// <summary>
    /// Model to use
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Id of the Agent
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The Name of the Agent (Optional in most cases, but some scenarios to require one)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The Description of the Agent (Information only and not used by the LLM)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Instruction for the Agent to be fed to the LLM as System/Developer Message
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// A set of Tools that the Agent are allowed to call
    /// </summary>
    public IList<AITool>? Tools { get; set; }

    /// <summary>
    /// An Action, if set, will attach an HTTP Message Handler so you can see the raw HTTP Calls that are sent to the LLM
    /// </summary>
    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }

    /// <summary>
    /// An Action, if set, will apply Tool Calling Middleware so you can inspect Tool Call Details
    /// </summary>
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }

    /// <summary>
    /// The maximum number of tokens in the generated chat response.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// An Action that allow you to inject additional ChatClientAgentOptions settings beyond what these options can do
    /// </summary>
    public Action<ChatClientAgentOptions>? AdditionalChatClientAgentOptions { get; set; }

    /// <summary>
    /// Apply Middleware to the Agent, if needed
    /// </summary>
    /// <param name="innerAgent">The inner Agent</param>
    /// <returns>The Agent back with applied middleware</returns>
    public AIAgent ApplyMiddleware(AIAgent innerAgent)
    {
        if (RawToolCallDetails != null)
        {
            innerAgent = innerAgent.AsBuilder().Use(new ToolCallsHandler(RawToolCallDetails).ToolCallingMiddleware).Build();
        }

        return innerAgent;
    }
}