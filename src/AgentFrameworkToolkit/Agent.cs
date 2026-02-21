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
    protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Run the agent with no message assuming that all required instructions are already provided to the agent or on the session, and requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <param name="session">
    /// The conversation session to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The session will be updated with any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">Optional JSON serializer options to use for deserializing the response.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse{T}"/> with the agent's output.</returns>
    /// <remarks>
    /// This overload is useful when the agent has sufficient context from previous messages in the session
    /// or from its initial configuration to generate a meaningful response without additional input.
    /// </remarks>
    public new Task<AgentResponse<T>> RunAsync<T>(
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default) =>
#pragma warning disable CS0618 // Type or member is obsolete
        AIAgentExtensions.RunAsync<T>(innerAgent, [], session, serializerOptions, options, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Runs the agent with a text message from the user, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <param name="message">The user message to send to the agent.</param>
    /// <param name="session">
    /// The conversation session to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The session will be updated with the input message and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">Optional JSON serializer options to use for deserializing the response.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse{T}"/> with the agent's output.</returns>
    /// <exception cref="ArgumentException"><paramref name="message"/> is <see langword="null"/>, empty, or contains only whitespace.</exception>
    /// <remarks>
    /// The provided text will be wrapped in a <see cref="ChatMessage"/> with the <see cref="ChatRole.User"/> role
    /// before being sent to the agent. This is a convenience method for simple text-based interactions.
    /// </remarks>
    public new Task<AgentResponse<T>> RunAsync<T>(
        string message,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return AIAgentExtensions.RunAsync<T>(innerAgent, new ChatMessage(ChatRole.User, message), session, serializerOptions, options, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Runs the agent with a single chat message, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <param name="message">The chat message to send to the agent.</param>
    /// <param name="session">
    /// The conversation session to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The session will be updated with the input message and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">Optional JSON serializer options to use for deserializing the response.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse{T}"/> with the agent's output.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
    public new Task<AgentResponse<T>> RunAsync<T>(
        ChatMessage message,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return AIAgentExtensions.RunAsync<T>(innerAgent, [message], session, serializerOptions, options, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <param name="messages">The collection of messages to send to the agent for processing.</param>
    /// <param name="session">
    /// The conversation session to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The session will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">Optional JSON serializer options to use for deserializing the response.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse{T}"/> with the agent's output.</returns>
    /// <remarks>
    /// <para>
    /// This method handles collections of messages, allowing for complex conversational scenarios including
    /// multi-turn interactions, function calls, and context-rich conversations.
    /// </para>
    /// <para>
    /// The messages are processed in the order provided and become part of the conversation history.
    /// The agent's response will also be added to <paramref name="session"/> if one is provided.
    /// </para>
    /// </remarks>
    public new Task<AgentResponse<T>> RunAsync<T>(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return AIAgentExtensions.RunAsync<T>(InnerAgent, messages, session, serializerOptions, options, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
