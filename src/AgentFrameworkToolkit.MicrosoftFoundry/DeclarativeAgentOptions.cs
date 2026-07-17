using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.MicrosoftFoundry;

/// <summary>
/// Options for a Declarative Agent
/// </summary>
[PublicAPI]
public class DeclarativeAgentOptions
{
    /// <summary>
    /// The Unique Name of the Agent
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Model to use
    /// </summary>
    public required string Model { get; set; }
    
    /// <summary>
    /// Instruction for the Agent to be fed to the LLM as System/Developer Message
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// A set of Tools that the Agent are allowed to call
    /// </summary>
    public IList<AITool>? Tools { get; set; }

    /// <summary>
    /// Reasoning Effort to use
    /// </summary>
    public ResponseReasoningEffortLevel? ReasoningEffort { get; set; }

    /// <summary>
    /// Reasoning Summary Verbosity
    /// </summary>
    public ResponseReasoningSummaryVerbosity? ReasoningSummaryVerbosity { get; set; }

    /// <summary>
    /// Determine if the WebSearch Tools should be available for the Agent (Default: false)
    /// </summary>
    public bool WebSearchTool { get; set; }

    /// <summary>
    /// Determine if the Code Interpreter Tool should be available for the Agent (Default: false)
    /// </summary>
    public bool CodeInterpreterTool { get; set; }

    /// <summary>
    /// MCP tools you wish to give to the agent
    /// </summary>
    public IList<McpTool>? McpTools { get; set; } = [];

    /// <summary>
    /// An Action, if set, will apply Tool Calling Middleware so you can inspect Tool Call Details
    /// </summary>
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }

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
    /// An optional <see cref="IServiceProvider"/> to use for resolving services required by the <see cref="AIFunction"/> instances being invoked.
    /// </summary>
    public IServiceProvider? Services { get; set; }

}