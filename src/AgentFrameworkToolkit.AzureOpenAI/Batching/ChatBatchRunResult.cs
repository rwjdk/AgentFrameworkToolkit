using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.AzureOpenAI.Batching;

/// <summary>
/// A joined structured batch result item containing the original request and any matched response or error.
/// </summary>
/// <typeparam name="T">The structured output type returned for the line.</typeparam>
public class ChatBatchRunResult<T> : BatchRunResult
{
    /// <summary>
    /// Response Object
    /// </summary>
    public T? ResponseObject { get; init; }
}

/// <summary>
/// A joined batch result item containing the original request and any matched response or error.
/// </summary>
public class BatchRunResult
{
    /// <summary>
    /// Gets or sets the optional custom id for the line. If omitted, one is generated.
    /// </summary>
    public required string CustomId { get; set; }

    /// <summary>
    /// Gets or set the messages sent as the request payload.
    /// </summary>
    public required IList<ChatMessage> RequestMessages { get; set; }

    /// <summary>
    /// Gets or set the messages returned.
    /// </summary>
    public required IList<ChatMessage> ResponseMessages { get; set; }

    /// <summary>
    /// Gets or sets the matched error for the request when present.
    /// </summary>
    public ChatBatchRunError? Error { get; init; }
}