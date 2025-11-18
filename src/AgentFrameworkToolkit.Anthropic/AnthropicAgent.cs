using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Anthropic;

public class AnthropicAgent(AIAgent innerAgent) : AIAgent
{
    public AIAgent InnerAgent => innerAgent;
    public override string Id => innerAgent.Id;
    public override string? Name => innerAgent.Name;
    public override string? Description => innerAgent.Description;
    public override string DisplayName => innerAgent.DisplayName;

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        return innerAgent.GetService(serviceType, serviceKey);
    }

    public override bool Equals(object? obj)
    {
        return innerAgent.Equals(obj);
    }

    public override int GetHashCode()
    {
        return innerAgent.GetHashCode();
    }

    public override string? ToString()
    {
        return innerAgent.ToString();
    }

    public override AgentThread GetNewThread()
    {
        return innerAgent.GetNewThread();
    }

    public override AgentThread DeserializeThread(JsonElement serializedThread, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return innerAgent.DeserializeThread(serializedThread, jsonSerializerOptions);
    }

    public override Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunAsync(messages, thread, options, cancellationToken);
    }

    public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken);
    }
}