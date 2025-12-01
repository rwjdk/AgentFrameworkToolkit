using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using System.Text;

namespace AgentFrameworkToolkit;

/// <summary>
/// Object represent Tool Call Details
/// </summary>
[PublicAPI]
public class ToolCallingDetails
{
    /// <summary>
    /// The Context of a Function Call
    /// </summary>
    public required FunctionInvocationContext Context { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder toolDetails = new();
        toolDetails.Append($"- Tool Call: '{Context.Function.Name}'");
        if (Context.Arguments.Count > 0)
        {
            toolDetails.Append($" (Args: {string.Join(",", Context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        return toolDetails.ToString();
    }
}