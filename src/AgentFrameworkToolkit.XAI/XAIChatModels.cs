using JetBrains.Annotations;

namespace AgentFrameworkToolkit.XAI;

/// <summary>
/// A List of the most common XAI Models
/// </summary>
[PublicAPI]
public static class XAIChatModels
{
    /// <summary>
    /// Grok 4.1 Fast (Reasoning)
    /// </summary>
    public const string Grok41FastReasoning = "grok-4-1-fast-reasoning";

    /// <summary>
    /// Grok 4.1 Fast (Non-Reasoning)
    /// </summary>
    public const string Grok41FastNonReasoning = "grok-4-1-fast-non-reasoning";

    /// <summary>
    /// Grok Code Fast 1
    /// </summary>
    public const string GrokCodeFast1 = "grok-code-fast-1";

    /// <summary>
    /// Grok 4 Fast (Reasoning)
    /// </summary>
    public const string Grok4FastReasoning = "grok-4-fast-reasoning";

    /// <summary>
    /// Grok 4 Fast (Non-Reasoning)
    /// </summary>
    public const string Grok4FastNonReasoning = "grok-4-fast-non-reasoning";

    /// <summary>
    /// Grok 4
    /// </summary>
    public const string Grok4 = "grok-4";

    /// <summary>
    /// Grok 3 Mini
    /// </summary>
    public const string Grok3Mini = "grok-3-mini";

    /// <summary>
    /// Grok 3
    /// </summary>
    public const string Grok3 = "grok-3";

    /// <summary>
    /// Grok 2 Vision
    /// </summary>
    public const string Grok2Vision = "grok-2-vision";
}