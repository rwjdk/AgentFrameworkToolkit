using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit;

internal static class StructuredOutputSchemaHelper
{
    internal static StructuredOutputSchemaDefinition Create<T>(JsonSerializerOptions? serializerOptions = null)
    {
        JsonSerializerOptions effectiveSerializerOptions = serializerOptions ?? AIJsonUtilities.DefaultOptions;
        ChatResponseFormatJson responseFormat = ChatResponseFormat.ForJsonSchema<T>(effectiveSerializerOptions);
        (ChatResponseFormatJson wrappedResponseFormat, bool isWrappedInObject) = WrapNonObjectSchema(responseFormat);

        return new StructuredOutputSchemaDefinition
        {
            ResponseFormat = wrappedResponseFormat,
            IsWrappedInObject = isWrappedInObject,
            SerializerOptions = effectiveSerializerOptions,
            SchemaName = GetSchemaName(wrappedResponseFormat, typeof(T))
        };
    }

    internal static (ChatResponseFormatJson ResponseFormat, bool IsWrappedInObject) WrapNonObjectSchema(ChatResponseFormatJson responseFormat)
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
            ["additionalProperties"] = false,
            ["required"] = new JsonArray("data")
        };

        JsonElement schema = JsonSerializer.SerializeToElement(wrappedSchema, AIJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject)));
        ChatResponseFormatJson wrappedResponseFormat = ChatResponseFormat.ForJsonSchema(schema, responseFormat.SchemaName, responseFormat.SchemaDescription);
        return (wrappedResponseFormat, true);
    }

    internal static bool SchemaRepresentsObject(JsonElement schema)
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

    internal static JsonNode? JsonElementToJsonNode(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Array => JsonArray.Create(element),
            JsonValueKind.Object => JsonObject.Create(element),
            _ => JsonValue.Create(element)
        };
    }

    internal static JsonElement NormalizeObjectSchemas(JsonElement schema)
    {
        JsonNode? rootNode = JsonElementToJsonNode(schema);
        if (rootNode == null)
        {
            return schema;
        }

        NormalizeSchemaNode(rootNode);
        return JsonSerializer.SerializeToElement(rootNode, AIJsonUtilities.DefaultOptions.GetTypeInfo(rootNode.GetType()));
    }

    private static string GetSchemaName(ChatResponseFormatJson responseFormat, Type type)
    {
        if (!string.IsNullOrWhiteSpace(responseFormat.SchemaName))
        {
            return SanitizeSchemaName(responseFormat.SchemaName);
        }

        return SanitizeSchemaName(GetTypeName(type));
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsArray)
        {
            return $"{GetTypeName(type.GetElementType()!)}_array";
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string genericTypeName = type.Name;
        int genericMarker = genericTypeName.IndexOf('`');
        if (genericMarker >= 0)
        {
            genericTypeName = genericTypeName[..genericMarker];
        }

        string argumentNames = string.Join("_", type.GetGenericArguments().Select(GetTypeName));
        return $"{genericTypeName}_{argumentNames}";
    }

    private static string SanitizeSchemaName(string value)
    {
        StringBuilder builder = new(value.Length);

        foreach (char character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) || character is '_' or '-' ? character : '_');
        }

        string sanitized = builder.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(sanitized) ? "structured_output" : sanitized;
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
}

internal sealed class StructuredOutputSchemaDefinition
{
    public required ChatResponseFormatJson ResponseFormat { get; init; }

    public required bool IsWrappedInObject { get; init; }

    public required JsonSerializerOptions SerializerOptions { get; init; }

    public required string SchemaName { get; init; }
}
