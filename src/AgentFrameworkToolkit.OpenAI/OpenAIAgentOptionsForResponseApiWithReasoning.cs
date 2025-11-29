using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Options for an OpenAI Agent (ResponsesApi with reasoning)
/// </summary>
public class OpenAIAgentOptionsForResponseApiWithReasoning : OpenAIAgentOptions
{
    /// <summary>
    /// The Reasoning Effort
    /// </summary>
    public ResponseReasoningEffortLevel? ReasoningEffort { get; set; }

    /// <summary>
    /// The Reasoning Summary Verbosity
    /// </summary>
    public ResponseReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }
}