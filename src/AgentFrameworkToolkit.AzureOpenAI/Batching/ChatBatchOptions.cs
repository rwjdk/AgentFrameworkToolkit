using AgentFrameworkToolkit.OpenAI;

namespace AgentFrameworkToolkit.AzureOpenAI.Batching;

/// <summary>
/// Options for a batch run.
/// </summary>
public class ChatBatchOptions
{
    /// <summary>
    /// Gets or sets the model or deployment name used for every line in the batch.
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// If the run should wait until completed
    /// </summary>
    public bool WaitUntilCompleted { get; set; }

    /// <summary>
    /// Gets or sets the endpoint style to use for the batch.
    /// </summary>
    public ChatBatchClientType ClientType { get; set; } = ChatBatchClientType.ChatClient;

    /// <summary>
    /// Gets or sets instructions that are prepended to every batch line as a system message.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of output tokens per request.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Gets or sets the temperature used for generation.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the reasoning effort when using reasoning-capable models.
    /// </summary>
    public OpenAIReasoningEffort? ReasoningEffort { get; set; }

    /// <summary>
    /// Gets or sets the reasoning summary verbosity for the Responses API.
    /// </summary>
    public OpenAIReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }

    /// <summary>
    /// Gets or sets the service tier to use when supported by the model.
    /// </summary>
    public OpenAIServiceTier? ServiceTier { get; set; }

    /// <summary>
    /// Gets or sets an action for inspecting the raw HTTP calls made during upload and batch creation.
    /// </summary>
    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }
}