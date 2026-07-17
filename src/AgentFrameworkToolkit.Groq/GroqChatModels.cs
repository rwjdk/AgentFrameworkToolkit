using JetBrains.Annotations;

namespace AgentFrameworkToolkit.Groq;

/// <summary>
/// A list of chat-compatible models and systems available on GroqCloud.
/// </summary>
[PublicAPI]
public static class GroqChatModels
{
    /// <summary>
    /// Llama 3.1 8B Instant.
    /// </summary>
    public const string Llama318BInstant = "llama-3.1-8b-instant";

    /// <summary>
    /// Llama 3.3 70B Versatile.
    /// </summary>
    public const string Llama3370BVersatile = "llama-3.3-70b-versatile";

    /// <summary>
    /// OpenAI GPT-OSS 120B.
    /// </summary>
    public const string GptOss120B = "openai/gpt-oss-120b";

    /// <summary>
    /// OpenAI GPT-OSS 20B.
    /// </summary>
    public const string GptOss20B = "openai/gpt-oss-20b";

    /// <summary>
    /// Groq Compound system.
    /// </summary>
    public const string Compound = "groq/compound";

    /// <summary>
    /// Groq Compound Mini system.
    /// </summary>
    public const string CompoundMini = "groq/compound-mini";

    /// <summary>
    /// Llama 4 Scout 17B 16E (preview).
    /// </summary>
    public const string Llama4Scout17B16EInstruct = "meta-llama/llama-4-scout-17b-16e-instruct";

    /// <summary>
    /// Llama Prompt Guard 2 22M (preview).
    /// </summary>
    public const string LlamaPromptGuard222M = "meta-llama/llama-prompt-guard-2-22m";

    /// <summary>
    /// Llama Prompt Guard 2 86M (preview).
    /// </summary>
    public const string LlamaPromptGuard286M = "meta-llama/llama-prompt-guard-2-86m";

    /// <summary>
    /// MiniMax M2.7 (preview and enterprise access).
    /// </summary>
    public const string MiniMaxM27 = "minimaxai/minimax-m2.7";

    /// <summary>
    /// OpenAI GPT-OSS Safeguard 20B (preview).
    /// </summary>
    public const string GptOssSafeguard20B = "openai/gpt-oss-safeguard-20b";

    /// <summary>
    /// Qwen3 32B (preview).
    /// </summary>
    public const string Qwen332B = "qwen/qwen3-32b";

    /// <summary>
    /// Qwen3.6 27B (preview).
    /// </summary>
    public const string Qwen3627B = "qwen/qwen3.6-27b";
}
