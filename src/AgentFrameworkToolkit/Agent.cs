using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgentFrameworkToolkit;

/// <summary>
/// A Generic Agent regardless of Provider derived from AI Agent
/// </summary>
/// <param name="innerAgent">The inner generic Agent</param>
[PublicAPI]
public class Agent(AIAgent innerAgent) : AIAgent
{
    /// <inheritdoc />
    protected override string IdCore => innerAgent.Id;

    /// <summary>
    /// The inner generic Agent
    /// </summary>
    public AIAgent InnerAgent => innerAgent;

    /// <inheritdoc />
    public override string? Name => innerAgent.Name;

    /// <inheritdoc />
    public override string? Description => innerAgent.Description;

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
    protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default)
    {
        return innerAgent.CreateSessionAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = new CancellationToken())
    {
        return innerAgent.SerializeSessionAsync(session, jsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement serializedSession, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.DeserializeSessionAsync(serializedSession, jsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunAsync(messages, session, options, cancellationToken);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunStreamingAsync(messages, session, options, cancellationToken);
    }
}
