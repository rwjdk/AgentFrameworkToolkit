using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// A successful parsed line from the batch output file with structured output.
/// </summary>
/// <typeparam name="T">The structured output type returned for the line.</typeparam>
internal class ChatBatchRunResponse<T> : ChatBatchRunResponse
{
    /// <summary>
    /// Gets or sets the structured result for the line.
    /// </summary>
    public required T? Result { get; init; }
}

/// <summary>
/// A successful parsed line from the batch output file.
/// </summary>
internal class ChatBatchRunResponse
{
    /// <summary>
    /// Gets or sets the custom id for the line.
    /// </summary>
    public required string CustomId { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code for the line.
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// Gets or sets the request id returned by the service.
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Gets or sets the parsed chat messages.
    /// </summary>
    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    /// <summary>
    /// Gets or sets the raw JSON response body.
    /// </summary>
    public JsonObject? RawBody { get; init; }
}