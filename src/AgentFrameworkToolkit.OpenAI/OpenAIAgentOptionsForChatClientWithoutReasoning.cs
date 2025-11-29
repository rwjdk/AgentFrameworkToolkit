namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Options for an OpenAI Agent (ChatClient without reasoning)
/// </summary>
public class OpenAIAgentOptionsForChatClientWithoutReasoning : OpenAIAgentOptions
{
    /// <summary>The temperature for generating chat responses.</summary>
    /// <remarks>
    /// This value controls the randomness of predictions made by the model. Use a lower value to decrease randomness in the response.
    /// </remarks>
    public float? Temperature { get; set; }
}