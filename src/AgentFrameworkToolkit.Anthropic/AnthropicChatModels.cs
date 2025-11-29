using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Anthropic;

/// <summary>
/// A List of the most common Anthropic Models
/// </summary>
[PublicAPI]
public static class AnthropicChatModels
{
    /// <summary>
    /// Claude Sonnet 3.5
    /// </summary>
    public const string ClaudeSonnet35 = "claude-3-5-sonnet";

    /// <summary>
    /// Claude Sonnet 3.7
    /// </summary>
    public const string ClaudeSonnet37 = "claude-3-7-sonnet";

    /// <summary>
    /// Claude Sonnet 4
    /// </summary>
    public const string ClaudeSonnet4 = "claude-sonnet-4";

    /// <summary>
    /// Claude Sonnet 4.5
    /// </summary>
    public const string ClaudeSonnet45 = "claude-sonnet-4-5";

    /// <summary>
    /// Claude Opus 4
    /// </summary>
    public const string ClaudeOpus4 = "claude-opus-4";

    /// <summary>
    /// Claude Opus 4.1
    /// </summary>
    public const string ClaudeOpus41 = "claude-opus-4-1";

    /// <summary>
    /// Claude Haiku 3.5
    /// </summary>
    public const string ClaudeHaiku35 = "claude-3-5-haiku";

    /// <summary>
    /// Claude Haiku 4.5
    /// </summary>
    public const string ClaudeHaiku45 = "claude-haiku-4-5";

    /// <summary>
    /// Claude Haiku 3
    /// </summary>
    public const string ClaudeHaiku3 = "claude-3-haiku";
}