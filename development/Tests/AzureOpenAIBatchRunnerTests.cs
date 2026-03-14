using System.Text.Json.Nodes;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.AzureOpenAI.Batching;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tests;

public sealed class AzureOpenAIBatchRunnerTests
{
    private sealed class StructuredReply
    {
        public required string Answer { get; set; }
    }

    [Fact]
    public void BuildJsonl_ChatClient_UsesAzureBatchEndpointAndSharedOptions()
    {
        ChatBatchOptions options = new()
        {
            Model = "gpt-5-nano-batch",
            ClientType = ChatBatchClientType.ChatClient,
            Instructions = "You are a batch system instruction",
            MaxOutputTokens = 256,
            ReasoningEffort = OpenAIReasoningEffort.Low
        };

        ChatBatchRequest line = new()
        {
            CustomId = "line-1",
            Messages =
            [
                new ChatMessage(ChatRole.System, "You are helpful"),
                new ChatMessage(ChatRole.User, "Hello")
            ]
        };

        string jsonl = BatchRunner.BuildJsonl(options, [line]);
        JsonObject payload = BatchRunner.ParseJsonObject(jsonl);

        Assert.Equal("/chat/completions", payload["url"]?.GetValue<string>());
        Assert.Equal("POST", payload["method"]?.GetValue<string>());
        Assert.Equal("line-1", payload["custom_id"]?.GetValue<string>());

        JsonObject body = payload["body"]?.AsObject() ?? throw new InvalidOperationException();
        Assert.Equal("gpt-5-nano-batch", body["model"]?.GetValue<string>());
        Assert.Equal(256, body["max_completion_tokens"]?.GetValue<int>());
        Assert.Equal("low", body["reasoning_effort"]?.GetValue<string>());

        JsonArray messages = body["messages"]?.AsArray() ?? throw new InvalidOperationException();
        Assert.Equal(3, messages.Count);
        Assert.Equal("system", messages[0]?["role"]?.GetValue<string>());
        Assert.Equal("You are a batch system instruction", messages[0]?["content"]?.GetValue<string>());
        Assert.Equal("system", messages[1]?["role"]?.GetValue<string>());
        Assert.Equal("You are helpful", messages[1]?["content"]?.GetValue<string>());
        Assert.Equal("user", messages[2]?["role"]?.GetValue<string>());
        Assert.Equal("Hello", messages[2]?["content"]?.GetValue<string>());
    }

    [Fact]
    public void BuildJsonl_ResponsesApi_UsesAzureBatchEndpointAndReasoningObject()
    {
        ChatBatchOptions options = new()
        {
            Model = "gpt-5-mini-batch",
            ClientType = ChatBatchClientType.ResponsesApi,
            Instructions = "You are a batch system instruction",
            MaxOutputTokens = 128,
            ReasoningEffort = OpenAIReasoningEffort.Low,
            ReasoningSummaryVerbosity = OpenAIReasoningSummaryVerbosity.Concise
        };

        ChatBatchRequest line = new()
        {
            Messages =
            [
                new ChatMessage(ChatRole.User, "Say hi")
            ]
        };

        string jsonl = BatchRunner.BuildJsonl(options, [line]);
        JsonObject payload = BatchRunner.ParseJsonObject(jsonl);

        Assert.Equal("/responses", payload["url"]?.GetValue<string>());

        JsonObject body = payload["body"]?.AsObject() ?? throw new InvalidOperationException();
        Assert.Equal("gpt-5-mini-batch", body["model"]?.GetValue<string>());
        Assert.Equal(128, body["max_output_tokens"]?.GetValue<int>());

        JsonObject reasoning = body["reasoning"]?.AsObject() ?? throw new InvalidOperationException();
        Assert.Equal("low", reasoning["effort"]?.GetValue<string>());
        Assert.Equal("concise", reasoning["summary"]?.GetValue<string>());

        JsonArray input = body["input"]?.AsArray() ?? throw new InvalidOperationException();
        Assert.Equal(2, input.Count);
        Assert.Equal("message", input[0]?["type"]?.GetValue<string>());
        Assert.Equal("system", input[0]?["role"]?.GetValue<string>());
        Assert.Equal("You are a batch system instruction", input[0]?["content"]?.GetValue<string>());
        Assert.Equal("message", input[1]?["type"]?.GetValue<string>());
        Assert.Equal("user", input[1]?["role"]?.GetValue<string>());
        Assert.Equal("Say hi", input[1]?["content"]?.GetValue<string>());
    }

    [Fact]
    public void ParseResultLines_ChatCompletion_ReturnsAssistantMessage()
    {
        const string jsonl =
            """
            {"custom_id":"line-7","response":{"status_code":200,"request_id":"req_123","body":{"choices":[{"message":{"role":"assistant","content":"Hello back"}}]}}}
            """;

        IReadOnlyList<ChatBatchRunResponse> resultLines = ChatBatchRun.ParseResultLines(jsonl, "/chat/completions");

        ChatBatchRunResponse result = Assert.Single(resultLines);
        Assert.Equal("line-7", result.CustomId);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("req_123", result.RequestId);

        ChatMessage message = Assert.Single(result.Messages);
        Assert.Equal(ChatRole.Assistant, message.Role);
        Assert.Equal("Hello back", message.Text);
    }

    [Fact]
    public void BuildJsonl_ChatClient_StructuredOutput_UsesJsonSchemaResponseFormat()
    {
        ChatBatchOptions options = new()
        {
            Model = "gpt-4.1-nano-batch",
            ClientType = ChatBatchClientType.ChatClient
        };

        ChatBatchRequest line = new()
        {
            Messages =
            [
                new ChatMessage(ChatRole.User, "Return a structured answer")
            ]
        };

        string jsonl = BatchRunner.BuildJsonl<StructuredReply>(options, [line]);
        JsonObject payload = BatchRunner.ParseJsonObject(jsonl);
        JsonObject body = payload["body"]?.AsObject() ?? throw new InvalidOperationException();
        JsonObject responseFormat = body["response_format"]?.AsObject() ?? throw new InvalidOperationException();

        Assert.Equal("json_schema", responseFormat["type"]?.GetValue<string>());

        JsonObject jsonSchema = responseFormat["json_schema"]?.AsObject() ?? throw new InvalidOperationException();
        Assert.Equal("StructuredReply", jsonSchema["name"]?.GetValue<string>());
        Assert.True(jsonSchema["strict"]?.GetValue<bool>());

        JsonObject schema = jsonSchema["schema"]?.AsObject() ?? throw new InvalidOperationException();
        Assert.Equal("object", schema["type"]?.GetValue<string>());
        Assert.True(schema["additionalProperties"]?.GetValue<bool>() == false);
        Assert.NotNull(schema["properties"]?["answer"]);
    }

    [Fact]
    public void BuildJsonl_ResponsesApi_StructuredOutput_WrapsArraySchemaInDataObject()
    {
        ChatBatchOptions options = new()
        {
            Model = "gpt-4.1-nano-batch",
            ClientType = ChatBatchClientType.ResponsesApi
        };

        ChatBatchRequest line = new()
        {
            Messages =
            [
                new ChatMessage(ChatRole.User, "Return a JSON array")
            ]
        };

        string jsonl = BatchRunner.BuildJsonl<string[]>(options, [line]);
        JsonObject payload = BatchRunner.ParseJsonObject(jsonl);
        JsonObject body = payload["body"]?.AsObject() ?? throw new InvalidOperationException();
        JsonObject text = body["text"]?.AsObject() ?? throw new InvalidOperationException();
        JsonObject format = text["format"]?.AsObject() ?? throw new InvalidOperationException();

        Assert.Equal("json_schema", format["type"]?.GetValue<string>());
        Assert.False(string.IsNullOrWhiteSpace(format["name"]?.GetValue<string>()));

        JsonObject schema = format["schema"]?.AsObject() ?? throw new InvalidOperationException();
        Assert.Equal("object", schema["type"]?.GetValue<string>());
        Assert.Equal("array", schema["properties"]?["data"]?["type"]?.GetValue<string>());

        JsonArray required = schema["required"]?.AsArray() ?? throw new InvalidOperationException();
        Assert.Contains(required, node => string.Equals(node?.GetValue<string>(), "data", StringComparison.Ordinal));
    }

    [Fact]
    public void ParseStructuredResultLines_ResponsesApi_UnwrapsArrayPayload()
    {
        const string jsonl =
            """
            {"custom_id":"line-9","response":{"status_code":200,"request_id":"req_789","body":{"output":[{"type":"message","role":"assistant","content":[{"type":"output_text","text":"{\"data\":[\"alpha\",\"beta\"]}"}]}]}}}
            """;

        IReadOnlyList<ChatBatchRunResponse<string[]>> resultLines = ChatBatchRun.ParseStructuredResultLines<string[]>(jsonl, "/responses");

        ChatBatchRunResponse<string[]> result = Assert.Single(resultLines);
        Assert.Equal("line-9", result.CustomId);
        string[] values = Assert.IsType<string[]>(result.Result);
        Assert.Equal(["alpha", "beta"], values);
    }

    [Fact]
    public void BuildResultItems_MatchesRequestResponseAndErrorByCustomId()
    {
        const string inputJsonl =
            """
            {"custom_id":"line-1","method":"POST","url":"/chat/completions","body":{"model":"gpt-4.1-nano-batch","messages":[{"role":"user","content":"Hello"}]}}
            {"custom_id":"line-2","method":"POST","url":"/chat/completions","body":{"model":"gpt-4.1-nano-batch","messages":[{"role":"user","content":"Fail"}]}}
            """;
        const string outputJsonl =
            """
            {"custom_id":"line-1","response":{"status_code":200,"request_id":"req_1","body":{"choices":[{"message":{"role":"assistant","content":"Hi"}}]}}}
            """;
        const string errorJsonl =
            """
            {"custom_id":"line-2","error":{"code":"bad_request","message":"Nope"}}
            """;

        IReadOnlyList<BatchRunResult> results = ChatBatchRun.BuildResultItems(inputJsonl, outputJsonl, errorJsonl, "/chat/completions");

        Assert.Equal(2, results.Count);

        BatchRunResult success = Assert.Single(results, result => result.CustomId == "line-1");
        Assert.Equal("Hello", Assert.Single(success.RequestMessages).Text);
        Assert.Null(success.Error);

        BatchRunResult failure = Assert.Single(results, result => result.CustomId == "line-2");
        Assert.Equal("Fail", Assert.Single(failure.RequestMessages).Text);
        Assert.Equal("bad_request", failure.Error?.ErrorCode);
    }

    [Fact]
    public async Task GetResultAsync_WhenBatchIsNotCompleted_ReturnsEmptyCollection()
    {
        ChatBatchRun batchRun = new(new AzureOpenAIConnection
        {
            Endpoint = "https://example.invalid"
        })
        {
            StatusString = "in_progress"
        };

        IReadOnlyList<BatchRunResult> results = await batchRun.GetResultAsync();

        Assert.Empty(results);
    }
}
