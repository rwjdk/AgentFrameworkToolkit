using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.AzureOpenAI.Batching;

/// <summary>
/// Represents one request line in the batch input file.
/// </summary>
public class ChatBatchRequest
{
    /// <summary>
    /// Gets or sets the optional custom id for the line. If omitted, one is generated.
    /// </summary>
    public string CustomId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or set the messages sent as the request payload.
    /// </summary>
    public required IList<ChatMessage> Messages { get; set; }

    /// <summary>
    /// Create a single user-message ChatBatchRequest
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>ChatBatchRequest</returns>
    public static ChatBatchRequest Create(string message)
    {
        return new ChatBatchRequest
        {
            Messages = [new ChatMessage(ChatRole.User, message)]
        };
    }

    /// <summary>
    /// Create a ChatBatchRequest
    /// </summary>
    /// <param name="messages">Messages</param>
    /// <returns>ChatBatchRequest</returns>
    public static ChatBatchRequest Create(IList<ChatMessage> messages)
    {
        return new ChatBatchRequest
        {
            Messages = messages
        };
    }
}