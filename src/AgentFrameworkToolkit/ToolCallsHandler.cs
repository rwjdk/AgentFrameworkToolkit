using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit;

/// <summary>
/// Handler for a Tool Call
/// </summary>
/// <param name="toolCallDetails">Action to Handle the Tool Call Details</param>
[PublicAPI]
public class ToolCallsHandler(Action<ToolCallingDetails> toolCallDetails)
{
    /// <summary>
    /// Result of Tool Calling Middleware
    /// </summary>
    /// <param name="agent">The Agent initiating the Tool Call</param>
    /// <param name="context">Context of the Call</param>
    /// <param name="next">The Next Func</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async ValueTask<object?> ToolCallingMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        object? result = await next(context, cancellationToken);
        toolCallDetails.Invoke(new ToolCallingDetails
        {
            Context = context
        });
        return result;
    }
}