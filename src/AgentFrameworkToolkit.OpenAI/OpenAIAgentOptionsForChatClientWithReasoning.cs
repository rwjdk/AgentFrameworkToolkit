using OpenAI.Chat;

#pragma warning disable OPENAI001
namespace AgentFrameworkToolkit.OpenAI;

public class OpenAIAgentOptionsForChatClientWithReasoning : OpenAIAgentOptions
{
    public ChatReasoningEffortLevel? ReasoningEffort { get; set; }
}