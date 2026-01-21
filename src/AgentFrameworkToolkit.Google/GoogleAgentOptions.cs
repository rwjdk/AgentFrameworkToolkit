using Google.GenAI.Types;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using static Microsoft.Agents.AI.ChatClientAgentOptions;

namespace AgentFrameworkToolkit.Google;

/// <summary>
/// Options for a Google Agent
/// </summary>
public class GoogleAgentOptions
{
    /// <summary>
    /// Model to use
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Id of the Agent
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The Name of the Agent (Optional in most cases, but some scenarios do require one)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The Description of the Agent (Information only and not used by the LLM)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Instruction for the Agent to be fed to the LLM as System/Developer Message
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// A set of Tools that the Agent are allowed to call
    /// </summary>
    public IList<AITool>? Tools { get; set; }

    /// <summary>
    /// An Action, if set, will apply Tool Calling Middleware so you can inspect Tool Call Details
    /// </summary>
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }

    /// <summary>
    /// The maximum number of tokens in the generated chat response.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// An Action that allow you to inject additional ChatClientAgentOptions settings beyond what these options can do
    /// </summary>
    public Action<ChatClientAgentOptions>? AdditionalChatClientAgentOptions { get; set; }

    /// <summary>The temperature for generating chat responses.</summary>
    /// <remarks>
    /// This value controls the randomness of predictions made by the model. Use a lower value to decrease randomness in the response.
    /// </remarks>
    public float? Temperature { get; set; }

    /// <summary>
    /// Only models lower than Gemini 3: Indicates the thinking budget in tokens. 0 is DISABLED. -1 is AUTOMATIC. The default values
    /// and allowed ranges are model dependent.
    /// </summary>
    public int? ThinkingBudget { get; set; }

    /// <summary>
    /// Only Gemini 3 and Higher: How much Thinking is allowed (Pro support HIGH and LOW, Flash support HIGH, MEDIUM, LOW and MINIMAL)
    /// </summary>
    public ThinkingLevel? ThinkingLevel { get; set; }

    /// <summary>
    /// Indicates whether to include thoughts in the response. If true, thoughts are returned only
    /// if the model supports thought and thoughts are available.
    /// </summary>
    public bool IncludeThoughts { get; set; }

    /// <summary>
    /// An optional <see cref="IServiceProvider"/> to use for resolving services required by the <see cref="AIFunction"/> instances being invoked.
    /// </summary>
    public IServiceProvider? Services { get; set; }

    /// <summary>
    /// Optional logger factory for enabling logging within the agent.
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    /// Enable Tool Calling Middleware allowing you to inspect, manipulate and cancel a tool-call
    /// </summary>
    public MiddlewareDelegates.ToolCallingMiddlewareDelegate? ToolCallingMiddleware { get; set; }

    /// <summary>
    /// Enable OpenTelemetry Middleware for OpenTelemetry Logging
    /// </summary>
    public OpenTelemetryMiddleware? OpenTelemetryMiddleware { get; set; }

    /// <summary>
    /// Enable Logging Middleware for custom Logging
    /// </summary>
    public LoggingMiddleware? LoggingMiddleware { get; set; }

    /// <summary>
    /// Gets or sets a factory function to create an instance of <see cref="ChatMessageStore"/>
    /// which will be used to store chat messages for this agent.
    /// </summary>
    public Func<ChatMessageStoreFactoryContext, CancellationToken, ValueTask<ChatMessageStore>>? ChatMessageStoreFactory { get; set; }

    /// <summary>
    /// Gets or sets a factory function to create an instance of <see cref="AIContextProvider"/>
    /// which will be used to create a context provider for each new thread, and can then
    /// provide additional context for each agent run.
    /// </summary>
    public Func<AIContextProviderFactoryContext, CancellationToken, ValueTask<AIContextProvider>>? AIContextProviderFactory { get; set; }
}
