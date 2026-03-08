using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Cerebras;

/// <summary>
/// A List of Cerebras Production Models
/// </summary>
[PublicAPI]
public static class CerebrasChatModels
{
    /// <summary>
    /// Llama 3.1 8B
    /// </summary>
    public const string Llama318B = "llama3.1-8b";

    /// <summary>
    /// GPT OSS 120B
    /// </summary>
    public const string GptOss120B = "gpt-oss-120b";
}
