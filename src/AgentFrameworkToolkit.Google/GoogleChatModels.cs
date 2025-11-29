using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// A List of the most common Google Models
/// </summary>
[PublicAPI]
public static class GoogleChatModels
{
    /// <summary>
    /// Gemini 2.5 Pro
    /// </summary>
    public const string Gemini25Pro = "gemini-2.5-pro";

    /// <summary>
    /// Gemini 2.5 Flash
    /// </summary>
    public const string Gemini25Flash = "gemini-2.5-flash";

    /// <summary>
    /// Gemini 2.5 Flash-Lite
    /// </summary>
    public const string Gemini25FlashLite = "gemini-2.5-flash-lite";
}