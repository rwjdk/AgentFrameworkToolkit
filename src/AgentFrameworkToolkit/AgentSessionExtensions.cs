using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit;

/// <summary>
/// Extensions for AgentSession
/// </summary>
public static class AgentSessionExtensions
{
    /// <summary>
    /// Get messages for the session (require that Agent use the default InMemoryChatHistory)
    /// </summary>
    /// <returns></returns>
    public static IList<ChatMessage> GetMessages(this AgentSession session)
    {
        if (!session.TryGetInMemoryChatHistory(out List<ChatMessage>? messages))
        {
            throw new AgentFrameworkToolkitException("Session is not connected to an InMemoryChatHistory, so can't retrieve messages");
        }
        return messages ?? [];
    }
}