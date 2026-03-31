using System.Text.Json.Nodes;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// A failed parsed line from the batch error file.
/// </summary>
[PublicAPI]
public class ChatBatchRunError
{
    /// <summary>
    /// Gets or sets the custom id for the line.
    /// </summary>
    public required string CustomId { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code for the line, if present.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Gets or sets the request id returned by the service, if present.
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets the raw error object.
    /// </summary>
    public JsonObject? RawError { get; init; }

    /// <summary>
    /// Gets or sets the raw response body when present.
    /// </summary>
    public JsonObject? RawBody { get; init; }
}