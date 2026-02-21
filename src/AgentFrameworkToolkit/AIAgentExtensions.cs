using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AgentFrameworkToolkit;

/// <summary>
/// Various Extensions for an AI Agent
/// </summary>
[PublicAPI]
[Obsolete("These extension methods are still here for polyfill reasons and will go away once Microsoft Fix https://github.com/microsoft/agent-framework/issues/4118")]
public static class AIAgentExtensions
{
    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="agent">The Agent to use</param>
    /// <param name="messages">The collection of messages to send to the agent for processing.</param>
    /// <param name="session">
    /// The conversation sessions to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The sessions will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse"/> with the agent's output.</returns>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <remarks>
    /// <para>
    /// This is the primary invocation method that implementations must override. It handles collections of messages,
    /// allowing for complex conversational scenarios including multi-turn interactions, function calls, and
    /// context-rich conversations.
    /// </para>
    /// <para>
    /// The messages are processed in the order provided and become part of the conversation history.
    /// The agent's response will also be added to <paramref name="session"/> if one is provided.
    /// </para>
    /// </remarks>
    [Obsolete("These extension methods are still here for polyfill reasons and will go away once Microsoft Fix https://github.com/microsoft/agent-framework/issues/4118")]
    public static async Task<AgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (agent is ChatClientAgent chatClientAgent)
        {
            return await chatClientAgent.RunAsync<T>(messages, session, serializerOptions, options, cancellationToken);
        }

        JsonSerializerOptions jsonSerializerOptions;
        if (serializerOptions != null)
        {
            jsonSerializerOptions = serializerOptions;
        }
        else
        {
            jsonSerializerOptions = new()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            };
            if (JsonSerializer.IsReflectionEnabledByDefault)
            {
                jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }

            jsonSerializerOptions.MakeReadOnly();
        }

        ChatResponseFormatJson responseFormat = ChatResponseFormat.ForJsonSchema<T>(jsonSerializerOptions);

        bool isWrappedInObject = false;
        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            JsonElement schema = responseFormat.Schema!.Value;
            if (!SchemaRepresentsObject(schema))
            {
                // For non-object-representing schemas, we wrap them in an object schema, because all
                // the real LLM providers today require an object schema as the root. This is currently
                // true even for providers that support native structured output.
                isWrappedInObject = true;
                schema = JsonSerializer.SerializeToElement(new JsonObject
                {
                    { "$schema", "https://json-schema.org/draft/2020-12/schema" },
                    { "type", "object" },
                    { "properties", new JsonObject { { "data", JsonElementToJsonNode(schema) } } },
                    { "additionalProperties", false },
                    { "required", new JsonArray("data") },
                }, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject)));
                responseFormat = ChatResponseFormat.ForJsonSchema(schema, responseFormat.SchemaName, responseFormat.SchemaDescription);
            }

            static JsonNode? JsonElementToJsonNode(JsonElement element) =>
                element.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.Array => JsonArray.Create(element),
                    JsonValueKind.Object => JsonObject.Create(element),
                    _ => JsonValue.Create(element)
                };
        }

        if (options != null)
        {
            if (options is ChatClientAgentRunOptions { ChatOptions: not null } chatClientAgentRunOptions)
            {
                chatClientAgentRunOptions.ChatOptions.ResponseFormat = responseFormat;
            }
            else
            {
                throw new NotSupportedException("Structure Output is not possible when provided options are not ChatClientAgentRunOptions");
            }
        }
        else
        {
            options = new ChatClientAgentRunOptions
            {
                ChatOptions = new()
                {
                    ResponseFormat = responseFormat
                }
            };
        }

        AgentResponse response = await agent.RunAsync(messages, session, options, cancellationToken);
        ChatResponse<T> chatResponse = new(response.AsChatResponse(), jsonSerializerOptions);
        if (isWrappedInObject)
        {
            Type type = chatResponse.GetType();
            PropertyInfo? propertyInfo = type.GetProperty("IsWrappedInObject", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(chatResponse, isWrappedInObject);
            }
        }

        AgentResponse finalAgentResponse = new(chatResponse);
        return new AgentResponse<T>(finalAgentResponse, jsonSerializerOptions)
        {
            IsWrappedInObject = isWrappedInObject
        };
    }

    static bool SchemaRepresentsObject(JsonElement schemaElement)
    {
        if (schemaElement.ValueKind is JsonValueKind.Object)
        {
            foreach (JsonProperty property in schemaElement.EnumerateObject())
            {
                if (property.NameEquals("type"u8))
                {
                    return property.Value.ValueKind == JsonValueKind.String
                           && property.Value.ValueEquals("object"u8);
                }
            }
        }

        return false;
    }


    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="agent">The Agent to Use</param>
    /// <param name="session">
    /// The conversation session to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The session will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse"/> with the agent's output.</returns>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <remarks>
    /// <para>
    /// This is the primary invocation method that implementations must override. It handles collections of messages,
    /// allowing for complex conversational scenarios including multi-turn interactions, function calls, and
    /// context-rich conversations.
    /// </para>
    /// <para>
    /// The messages are processed in the order provided and become part of the conversation history.
    /// The agent's response will also be added to <paramref name="session"/> if one is provided.
    /// </para>
    /// </remarks>
    [Obsolete("These extension methods are still here for polyfill reasons and will go away once Microsoft Fix https://github.com/microsoft/agent-framework/issues/4118")]
    public static async Task<AgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default) =>
        await RunAsync<T>(agent, [], session, serializerOptions, options, cancellationToken);

    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="agent">The agent to use</param>
    /// <param name="message">The message to send to the agent for processing.</param>
    /// <param name="session">
    /// The conversation session to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The session will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse"/> with the agent's output.</returns>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <remarks>
    /// <para>
    /// This is the primary invocation method that implementations must override. It handles collections of messages,
    /// allowing for complex conversational scenarios including multi-turn interactions, function calls, and
    /// context-rich conversations.
    /// </para>
    /// <para>
    /// The messages are processed in the order provided and become part of the conversation history.
    /// The agent's response will also be added to <paramref name="session"/> if one is provided.
    /// </para>
    /// </remarks>
    [Obsolete("These extension methods are still here for polyfill reasons and will go away once Microsoft Fix https://github.com/microsoft/agent-framework/issues/4118")]
    public static async Task<AgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        string message,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await RunAsync<T>(agent, new ChatMessage(ChatRole.User, message), session, serializerOptions, options, cancellationToken);
    }

    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="agent">The agent to use</param>
    /// <param name="message">The message to send to the agent for processing.</param>
    /// <param name="session">
    /// The conversation session to use for this invocation. If <see langword="null"/>, a new session will be created.
    /// The session will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AgentResponse"/> with the agent's output.</returns>
    /// <typeparam name="T">The type of structured output to request.</typeparam>
    /// <remarks>
    /// <para>
    /// This is the primary invocation method that implementations must override. It handles collections of messages,
    /// allowing for complex conversational scenarios including multi-turn interactions, function calls, and
    /// context-rich conversations.
    /// </para>
    /// <para>
    /// The messages are processed in the order provided and become part of the conversation history.
    /// The agent's response will also be added to <paramref name="session"/> if one is provided.
    /// </para>
    /// </remarks>
    [Obsolete("These extension methods are still here for polyfill reasons and will go away once Microsoft Fix https://github.com/microsoft/agent-framework/issues/4118")]
    public static async Task<AgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        ChatMessage message,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await RunAsync<T>(agent, [message], session, serializerOptions, options, cancellationToken);
    }
}
