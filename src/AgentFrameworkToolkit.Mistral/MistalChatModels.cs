using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Mistral;

/// <summary>
/// The various Mistral Models (if a model is not on the list it might still work; these are just the most common models)
/// </summary>
[PublicAPI]
public class MistalChatModels
{
    /// <summary>
    /// Mistral (Small)
    /// </summary>
    public const string MistralSmall = "mistral-small-latest";

    /// <summary>
    /// Mistral (Medium)
    /// </summary>
    public const string MistralMedium = "mistral-medium-latest";

    /// <summary>
    /// Mistral (Large)
    /// </summary>
    public const string MistralLarge = "mistral-large-latest";
}