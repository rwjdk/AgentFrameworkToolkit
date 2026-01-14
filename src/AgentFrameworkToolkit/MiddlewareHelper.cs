using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit;

/// <summary>
/// Helps with applying Middleware
/// </summary>
public static class MiddlewareHelper
{
    /// <summary>
    /// Apply Middleware to the Agent, if needed
    /// </summary>
    /// <param name="innerAgent">The inner Agent</param>
    /// <param name="rawToolCallDetails">An Action, if set, will apply Tool Calling Middleware so you can inspect Tool Call Details</param>
    /// <param name="toolCallingMiddleware">Enable Tool Calling Middleware allowing you to inspect, manipulate and cancel a tool-call</param>
    /// <param name="openTelemetryMiddleware">Enable OpenTelemetry Middleware for OpenTelemetry Logging</param>
    /// <param name="loggingMiddleware">Enable Logging Middleware for custom Logging</param>
    /// <param name="services">An optional <see cref="IServiceProvider"/> to use for resolving services required by the <see cref="AIFunction"/> instances being invoked.</param>
    /// <returns>The Agent back with applied middleware</returns>
    public static AIAgent ApplyMiddleware(
        AIAgent innerAgent,
        Action<ToolCallingDetails>? rawToolCallDetails,
        MiddlewareDelegates.ToolCallingMiddlewareDelegate? toolCallingMiddleware,
        OpenTelemetryMiddleware? openTelemetryMiddleware,
        LoggingMiddleware? loggingMiddleware,
        IServiceProvider? services)
    {
        if (rawToolCallDetails == null && toolCallingMiddleware == null && openTelemetryMiddleware == null && loggingMiddleware == null)
        {
            return innerAgent;
        }

        AIAgentBuilder builder = innerAgent.AsBuilder();
        if (rawToolCallDetails != null)
        {
            builder = builder.Use(new ToolCallsHandler(rawToolCallDetails).ToolCallingMiddlewareAsync);
        }

        if (openTelemetryMiddleware != null)
        {
            builder = builder.UseOpenTelemetry(openTelemetryMiddleware.Source, openTelemetryMiddleware.Configure);
        }

        if (toolCallingMiddleware != null)
        {
            builder = builder.Use(toolCallingMiddleware.Invoke);
        }

        if (loggingMiddleware != null)
        {
            builder = builder.UseLogging(loggingMiddleware.LoggerFactory, loggingMiddleware.Configure);
        }

        innerAgent = builder.Build(services);
        return innerAgent;
    }
}
