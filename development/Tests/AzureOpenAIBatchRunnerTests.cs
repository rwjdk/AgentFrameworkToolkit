using System.Text.Json.Nodes;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tests;

public sealed class AzureOpenAIBatchRunnerTests
{
    [Fact]
    public void BuildJsonl_ChatClient_UsesAzureBatchEndpointAndSharedOptions()
    {
        BatchRunOptions options = new()
        {
            Model = "gpt-5-nano-batch",
            ClientType = BatchClientType.ChatClient,
            MaxOutputTokens = 256,
            ReasoningEffort = OpenAIReasoningEffort.Low
        };

        BatchRunLine line = new()
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
        Assert.Equal(2, messages.Count);
        Assert.Equal("system", messages[0]?["role"]?.GetValue<string>());
        Assert.Equal("You are helpful", messages[0]?["content"]?.GetValue<string>());
        Assert.Equal("user", messages[1]?["role"]?.GetValue<string>());
        Assert.Equal("Hello", messages[1]?["content"]?.GetValue<string>());
    }

    [Fact]
    public void BuildJsonl_ResponsesApi_UsesAzureBatchEndpointAndReasoningObject()
    {
        BatchRunOptions options = new()
        {
            Model = "gpt-5-mini-batch",
            ClientType = BatchClientType.ResponsesApi,
            MaxOutputTokens = 128,
            ReasoningEffort = OpenAIReasoningEffort.Low,
            ReasoningSummaryVerbosity = OpenAIReasoningSummaryVerbosity.Concise
        };

        BatchRunLine line = new()
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
        Assert.Single(input);
        Assert.Equal("message", input[0]?["type"]?.GetValue<string>());
        Assert.Equal("user", input[0]?["role"]?.GetValue<string>());
        Assert.Equal("Say hi", input[0]?["content"]?.GetValue<string>());
    }

    [Fact]
    public void ParseResultLines_ChatCompletion_ReturnsAssistantMessage()
    {
        const string jsonl =
            """
            {"custom_id":"line-7","response":{"status_code":200,"request_id":"req_123","body":{"choices":[{"message":{"role":"assistant","content":"Hello back"}}]}}}
            """;

        IReadOnlyList<BatchRunResultLine> resultLines = BatchRun.ParseResultLines(jsonl, "/chat/completions");

        BatchRunResultLine result = Assert.Single(resultLines);
        Assert.Equal("line-7", result.CustomId);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("req_123", result.RequestId);

        ChatMessage message = Assert.Single(result.Messages);
        Assert.Equal(ChatRole.Assistant, message.Role);
        Assert.Equal("Hello back", message.Text);
    }
}
