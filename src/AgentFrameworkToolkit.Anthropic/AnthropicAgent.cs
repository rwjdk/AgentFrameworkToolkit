using Anthropic.Models.Messages;
using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AgentFrameworkToolkit.Anthropic;

/// <summary>
/// An Agent targeting Anthropic (Claude)
/// </summary>
/// <param name="innerAgent">The inner generic Agent</param>
[PublicAPI]
public class AnthropicAgent(AIAgent innerAgent) : Agent(innerAgent)
{
    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
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
    public new async Task<AgentResponse<T>> RunAsync<T>(
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default) =>
        await RunAsync<T>([], session, serializerOptions, options, cancellationToken);

    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
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
    public new async Task<AgentResponse<T>> RunAsync<T>(
        string message,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await RunAsync<T>(new ChatMessage(ChatRole.User, message), session, serializerOptions, options, cancellationToken);
    }

    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
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
    public new async Task<AgentResponse<T>> RunAsync<T>(
        ChatMessage message,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await RunAsync<T>([message], session, serializerOptions, options, cancellationToken);
    }



    /// <summary>
    /// Runs the agent with a collection of chat messages, requesting a response of the specified type <typeparamref name="T"/>.
    /// </summary>
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
    public new async Task<AgentResponse<T>> RunAsync<T>(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        JsonSerializerOptions? serializerOptions = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions effectiveSerializerOptions = serializerOptions ?? AgentAbstractionsJsonUtilities.DefaultOptions;
        ChatResponseFormatJson responseFormat = ChatResponseFormat.ForJsonSchema<T>(effectiveSerializerOptions);
        (ChatResponseFormatJson wrappedResponseFormat, bool isWrappedInObject) = WrapNonObjectSchema(responseFormat);

        OutputConfig outputConfig = CreateOutputConfig(wrappedResponseFormat);
        ChatClientAgentRunOptions runOptions = CreateRunOptionsWithOutputConfig(options, outputConfig);
        runOptions.ResponseFormat = wrappedResponseFormat;

        AgentResponse response = await RunAsync(messages, session, runOptions, cancellationToken).ConfigureAwait(false);
        return new AgentResponse<T>(response, effectiveSerializerOptions)
        {
            IsWrappedInObject = isWrappedInObject
        };
    }

    private ChatClientAgentRunOptions CreateRunOptionsWithOutputConfig(AgentRunOptions? options, OutputConfig outputConfig)
    {
        ChatClientAgentRunOptions chatClientRunOptions = options?.Clone() as ChatClientAgentRunOptions ?? CreateChatClientRunOptions(options);
        ChatOptions chatOptions = chatClientRunOptions.ChatOptions ?? new ChatOptions();

        ChatOptions? defaultChatOptions = GetService(typeof(ChatOptions)) as ChatOptions;
        string? modelId = chatOptions.ModelId ?? defaultChatOptions?.ModelId;
        int? maxOutputTokens = chatOptions.MaxOutputTokens ?? defaultChatOptions?.MaxOutputTokens;
        Func<IChatClient, object?>? defaultFactory = defaultChatOptions?.RawRepresentationFactory;

        Func<IChatClient, object?>? existingFactory = chatOptions.RawRepresentationFactory;
        chatOptions.RawRepresentationFactory = chatClient =>
        {
            object? existingRawRepresentation = existingFactory?.Invoke(chatClient);

            if (existingRawRepresentation is MessageCreateParams messageCreateParams)
            {
                if (messageCreateParams.OutputConfig != null)
                {
                    return messageCreateParams;
                }

                return messageCreateParams with
                {
                    OutputConfig = new OutputConfig(outputConfig)
                };
            }

            if (existingRawRepresentation != null)
            {
                return existingRawRepresentation;
            }

            object? defaultRawRepresentation = defaultFactory?.Invoke(chatClient);
            if (defaultRawRepresentation is MessageCreateParams defaultMessageCreateParams)
            {
                if (defaultMessageCreateParams.OutputConfig != null)
                {
                    return defaultMessageCreateParams;
                }

                return defaultMessageCreateParams with
                {
                    OutputConfig = new OutputConfig(outputConfig)
                };
            }

            if (defaultRawRepresentation != null)
            {
                return defaultRawRepresentation;
            }

            if (string.IsNullOrWhiteSpace(modelId) || !maxOutputTokens.HasValue)
            {
                return null;
            }

            return new MessageCreateParams
            {
                Model = modelId,
                MaxTokens = maxOutputTokens.Value,
                Messages = [],
                OutputConfig = new OutputConfig(outputConfig)
            };
        };

        chatClientRunOptions.ChatOptions = chatOptions;
        return chatClientRunOptions;
    }

    private static ChatClientAgentRunOptions CreateChatClientRunOptions(AgentRunOptions? options)
    {
        ChatClientAgentRunOptions runOptions = new();

        if (options == null)
        {
            return runOptions;
        }

        runOptions.AllowBackgroundResponses = options.AllowBackgroundResponses;
#pragma warning disable MEAI001
        runOptions.ContinuationToken = options.ContinuationToken;
#pragma warning restore MEAI001
        runOptions.ResponseFormat = options.ResponseFormat;
        runOptions.AdditionalProperties = options.AdditionalProperties?.Clone();

        return runOptions;
    }

    private static OutputConfig CreateOutputConfig(ChatResponseFormatJson responseFormat)
    {
        if (!responseFormat.Schema.HasValue)
        {
            throw new InvalidOperationException("Structured output requires a valid JSON schema.");
        }

        JsonElement schema = NormalizeSchemaForAnthropic(responseFormat.Schema.Value);
        if (schema.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Structured output schema root must be an object.");
        }

        Dictionary<string, JsonElement> schemaProperties = [];
        foreach (JsonProperty property in schema.EnumerateObject())
        {
            schemaProperties[property.Name] = property.Value;
        }

        return new OutputConfig
        {
            Format = new JsonOutputFormat
            {
                Schema = schemaProperties
            }
        };
    }

    private static JsonElement NormalizeSchemaForAnthropic(JsonElement schema)
    {
        JsonNode? rootNode = JsonElementToJsonNode(schema);
        if (rootNode is not JsonObject rootObject)
        {
            return schema;
        }

        NormalizeSchemaNode(rootObject);
        return JsonSerializer.SerializeToElement(rootObject, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject)));
    }

    private static void NormalizeSchemaNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            if (IsObjectSchema(jsonObject))
            {
                jsonObject["additionalProperties"] = false;
            }

            foreach (KeyValuePair<string, JsonNode?> property in jsonObject)
            {
                if (property.Value != null)
                {
                    NormalizeSchemaNode(property.Value);
                }
            }

            return;
        }

        if (node is JsonArray jsonArray)
        {
            foreach (JsonNode? item in jsonArray)
            {
                if (item != null)
                {
                    NormalizeSchemaNode(item);
                }
            }
        }
    }

    private static bool IsObjectSchema(JsonObject jsonObject)
    {
        JsonNode? typeNode = jsonObject["type"];
        if (typeNode is JsonValue typeValue && typeValue.TryGetValue(out string? typeAsString))
        {
            return string.Equals(typeAsString, "object", StringComparison.Ordinal);
        }

        if (typeNode is JsonArray typeArray)
        {
            foreach (JsonNode? typeItem in typeArray)
            {
                if (typeItem is JsonValue typeItemValue &&
                    typeItemValue.TryGetValue(out string? itemTypeAsString) &&
                    string.Equals(itemTypeAsString, "object", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return jsonObject["properties"] is JsonObject ||
               jsonObject["required"] is JsonArray ||
               jsonObject["patternProperties"] is JsonObject;
    }

    private static (ChatResponseFormatJson ResponseFormat, bool IsWrappedInObject) WrapNonObjectSchema(ChatResponseFormatJson responseFormat)
    {
        if (!responseFormat.Schema.HasValue)
        {
            throw new InvalidOperationException("The response format must have a valid JSON schema.");
        }

        if (SchemaRepresentsObject(responseFormat.Schema.Value))
        {
            return (responseFormat, false);
        }

        JsonObject wrappedSchema = new()
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["data"] = JsonElementToJsonNode(responseFormat.Schema.Value)
            },
            ["additionalProperties"] = false
        };

        JsonArray requiredProperties =
        [
            "data"
        ];
        wrappedSchema["required"] = requiredProperties;

        JsonElement schema = JsonSerializer.SerializeToElement(wrappedSchema, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject)));
        ChatResponseFormatJson wrappedResponseFormat = ChatResponseFormat.ForJsonSchema(schema, responseFormat.SchemaName, responseFormat.SchemaDescription);
        return (wrappedResponseFormat, true);
    }

    private static bool SchemaRepresentsObject(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (JsonProperty property in schema.EnumerateObject())
        {
            if (property.NameEquals("type"u8))
            {
                return property.Value.ValueKind == JsonValueKind.String && property.Value.ValueEquals("object"u8);
            }
        }

        return false;
    }

    private static JsonNode? JsonElementToJsonNode(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Array => JsonArray.Create(element),
            JsonValueKind.Object => JsonObject.Create(element),
            _ => JsonValue.Create(element)
        };
    }
}


