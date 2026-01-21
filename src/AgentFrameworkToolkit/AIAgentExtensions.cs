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
public static class AIAgentExtensions
{
    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="agent">The Agent to use</param>
    /// <param name="messages">The collection of messages to send to the agent for processing.</param>
    /// <param name="thread">
    /// The conversation thread to use for this invocation. If <see langword="null"/>, a new thread will be created.
    /// The thread will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="useJsonSchemaResponseFormat">
    /// <see langword="true" /> to set a JSON schema on the <see cref="ChatResponseFormat"/>; otherwise, <see langword="false" />. The default is <see langword="true" />.
    /// Using a JSON schema improves reliability if the underlying model supports native structured output with a schema, but might cause an error if the model does not support it.
    /// </param>
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
    /// The agent's response will also be added to <paramref name="thread"/> if one is provided.
    /// </para>
    /// </remarks>
    public static async Task<ChatClientAgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        if (agent is ChatClientAgent chatClientAgent)
        {
            return await chatClientAgent.RunAsync<T>(messages, thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
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

        AgentResponse response = await agent.RunAsync(messages, thread, options, cancellationToken);
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

        return new ChatClientAgentResponse<T>(chatResponse);
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
    /// <param name="thread">
    /// The conversation thread to use for this invocation. If <see langword="null"/>, a new thread will be created.
    /// The thread will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="useJsonSchemaResponseFormat">
    /// <see langword="true" /> to set a JSON schema on the <see cref="ChatResponseFormat"/>; otherwise, <see langword="false" />. The default is <see langword="true" />.
    /// Using a JSON schema improves reliability if the underlying model supports native structured output with a schema, but might cause an error if the model does not support it.
    /// </param>
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
    /// The agent's response will also be added to <paramref name="thread"/> if one is provided.
    /// </para>
    /// </remarks>
    public static async Task<ChatClientAgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default) =>
        await agent.RunAsync<T>([], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);

    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="agent">The agent to use</param>
    /// <param name="message">The message to send to the agent for processing.</param>
    /// <param name="thread">
    /// The conversation thread to use for this invocation. If <see langword="null"/>, a new thread will be created.
    /// The thread will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="useJsonSchemaResponseFormat">
    /// <see langword="true" /> to set a JSON schema on the <see cref="ChatResponseFormat"/>; otherwise, <see langword="false" />. The default is <see langword="true" />.
    /// Using a JSON schema improves reliability if the underlying model supports native structured output with a schema, but might cause an error if the model does not support it.
    /// </param>
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
    /// The agent's response will also be added to <paramref name="thread"/> if one is provided.
    /// </para>
    /// </remarks>
    public static async Task<ChatClientAgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        string message,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        return await agent.RunAsync<T>(new ChatMessage(ChatRole.User, message), thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
    }

    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="agent">The agent to use</param>
    /// <param name="message">The message to send to the agent for processing.</param>
    /// <param name="thread">
    /// The conversation thread to use for this invocation. If <see langword="null"/>, a new thread will be created.
    /// The thread will be updated with the input messages and any response messages generated during invocation.
    /// </param>
    /// <param name="serializerOptions">The JSON serialization options to use.</param>
    /// <param name="options">Optional configuration parameters for controlling the agent's invocation behavior.</param>
    /// <param name="useJsonSchemaResponseFormat">
    /// <see langword="true" /> to set a JSON schema on the <see cref="ChatResponseFormat"/>; otherwise, <see langword="false" />. The default is <see langword="true" />.
    /// Using a JSON schema improves reliability if the underlying model supports native structured output with a schema, but might cause an error if the model does not support it.
    /// </param>
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
    /// The agent's response will also be added to <paramref name="thread"/> if one is provided.
    /// </para>
    /// </remarks>
    public static async Task<ChatClientAgentResponse<T>> RunAsync<T>(
        this AIAgent agent,
        ChatMessage message,
        AgentThread? thread = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        bool? useJsonSchemaResponseFormat = null,
        CancellationToken cancellationToken = default)
    {
        return await agent.RunAsync<T>([message], thread, serializerOptions, options, useJsonSchemaResponseFormat, cancellationToken);
    }
}
