using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// An Agent targeting Google
/// </summary>
/// <param name="innerAgent">The inner generic Agent</param>
[PublicAPI]
public class GoogleAgent(AIAgent innerAgent) : AIAgent
{
    /// <summary>
    /// The inner generic Agent
    /// </summary>
    public AIAgent InnerAgent => innerAgent;

    /// <inheritdoc />
    public override string Id => innerAgent.Id;

    /// <inheritdoc />
    public override string? Name => innerAgent.Name;

    /// <inheritdoc />
    public override string? Description => innerAgent.Description;

    /// <inheritdoc />
    public override string DisplayName => innerAgent.DisplayName;

    /// <inheritdoc />
    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        return innerAgent.GetService(serviceType, serviceKey);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return innerAgent.Equals(obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return innerAgent.GetHashCode();
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return innerAgent.ToString();
    }

    /// <inheritdoc />
    public override AgentThread GetNewThread()
    {
        return innerAgent.GetNewThread();
    }

    /// <inheritdoc />
    public override AgentThread DeserializeThread(JsonElement serializedThread, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return innerAgent.DeserializeThread(serializedThread, jsonSerializerOptions);
    }

    /// <inheritdoc />
    public override Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunAsync(messages, thread, options, cancellationToken);
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken);
    }
}