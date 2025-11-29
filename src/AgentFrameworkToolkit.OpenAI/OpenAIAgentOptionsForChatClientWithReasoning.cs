using OpenAI.Chat;

#pragma warning disable OPENAI001
namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Options for an OpenAI Agent (ChatClient with reasoning)
/// </summary>
public class OpenAIAgentOptionsForChatClientWithReasoning : OpenAIAgentOptions
{
    /// <summary>
    /// The Reasoning Effort
    /// </summary>
    public ChatReasoningEffortLevel? ReasoningEffort { get; set; }
}