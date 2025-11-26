using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Anthropic;

public class AnthropicAgentOptions
{
    public required string DeploymentModelName { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public IList<AITool>? Tools { get; set; }
    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }
    public Action<ChatClientAgentOptions>? AdditionalChatClientAgentOptions { get; set; }
    public required int MaxOutputTokens { get; set; }
    public float? Temperature { get; set; }

    public AIAgent ApplyMiddleware(AIAgent innerAgent)
    {
        //todo - more middleware options
        if (RawToolCallDetails != null)
        {
            innerAgent = innerAgent.AsBuilder().Use(new ToolCallsHandler(RawToolCallDetails).ToolCallingMiddleware).Build();
        }

        return innerAgent;
    }

    /// <summary>
    /// Reasoning effort knob, in tokens. Higher value --> more internal reasoning.
    /// </summary>
    public int BudgetTokens { get; set; }
}