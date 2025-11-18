using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit;

public class ToolCallsHandler(Action<ToolCallingDetails> toolCallDetails)
{
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