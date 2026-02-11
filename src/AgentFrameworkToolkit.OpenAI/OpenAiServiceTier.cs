namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// The different OpenAI Service Tiers
/// </summary>
public enum OpenAiServiceTier
{
    /// <summary>
    /// Automatic
    /// </summary>
    Auto,

    /// <summary>
    /// Flex Tier (Cheaper but higher latency) [Note: Not all models support this tier]
    /// </summary>
    Flex,

    /// <summary>
    /// Default Tier
    /// </summary>
    Default,

    /// <summary>
    /// Flex Tier (More Expensive but lover latency)
    /// </summary>
    Priority
}