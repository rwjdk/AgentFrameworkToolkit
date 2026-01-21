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
    public override ValueTask<AgentThread> GetNewThreadAsync(CancellationToken cancellationToken = default)
    {
        return innerAgent.GetNewThreadAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override ValueTask<AgentThread> DeserializeThreadAsync(JsonElement serializedThread, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.DeserializeThreadAsync(serializedThread, jsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunAsync(messages, thread, options, cancellationToken);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken);
    }
}
