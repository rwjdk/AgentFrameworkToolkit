using JetBrains.Annotations;

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// A List of the most common OpenAI Models
/// </summary>
[PublicAPI]
public static class OpenAIChatModels
{
    /// <summary>
    /// GPT-5.1 (Reasoning)
    /// </summary>
    public const string Gpt51 = "gpt-5.1";

    /// <summary>
    /// GPT-5.1 Codex (Reasoning) [NB: Only work with Responses API]
    /// </summary>
    public const string Gpt51Codex = "gpt-5.1-codex";

    /// <summary>
    /// GPT-5 Codex (Reasoning) [NB: Only work with Responses API]
    /// </summary>
    public const string Gpt5Codex = "gpt-5-codex";

    /// <summary>
    /// GPT-5 pro (Reasoning)
    /// </summary>
    public const string Gpt5Pro = "gpt-5-pro";

    /// <summary>
    /// GPT-5 (Reasoning)
    /// </summary>
    public const string Gpt5 = "gpt-5";

    /// <summary>
    /// GPT-5-Mini (Reasoning)
    /// </summary>
    public const string Gpt5Mini = "gpt-5-mini";

    /// <summary>
    /// GPT-5-Nano (Reasoning)
    /// </summary>
    public const string Gpt5Nano = "gpt-5-nano";

    /// <summary>
    /// GPT-4.1 (Non-Reasoning)
    /// </summary>
    public const string Gpt41 = "gpt-4.1";

    /// <summary>
    /// GPT-4.1 Mini (Non-Reasoning)
    /// </summary>
    public const string Gpt41Mini = "gpt-4.1-mini";

    /// <summary>
    /// GPT-4.1 Nano (Non-Reasoning)
    /// </summary>
    public const string Gpt41Nano = "gpt-4.1-nano";
}