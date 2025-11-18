using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.OpenAI;

public class OpenAIAgentOptionsForResponseApiWithReasoning : OpenAIAgentOptions
{
    public ResponseReasoningEffortLevel? ReasoningEffort { get; set; }
    public ResponseReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }
}