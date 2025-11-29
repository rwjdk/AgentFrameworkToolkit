namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// Options for an OpenAI Agent (ResponsesApi without reasoning)
/// </summary>
public class OpenAIAgentOptionsForResponseApiWithoutReasoning : OpenAIAgentOptions
{
    /// <summary>The temperature for generating chat responses.</summary>
    /// <remarks>
    /// This value controls the randomness of predictions made by the model. Use a lower value to decrease randomness in the response.
    /// </remarks>
    public float? Temperature { get; set; }
}