using Microsoft.Extensions.AI;
using System;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Date and Time based tools
/// </summary>
public static class AITimeTools
{
    /// <summary>
    /// All Time AITools
    /// </summary>
    /// <returns>Tools</returns>
    public static IList<AITool> All()
    {
        return [GetLocalNow(), GetUtcNow()];
    }

    /// <summary>
    /// UTC Now
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetUtcNow(string? toolName = "get_utc_now", string? toolDescription = null)
    {
        return AIFunctionFactory.Create(() => DateTime.UtcNow, toolName, description: toolDescription);
    }

    /// <summary>
    /// Local Now (Machine or provided timeZoneId)
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetLocalNow(string? toolName = "get_local_now", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string? timeZoneId = null) => string.IsNullOrWhiteSpace(timeZoneId) ? DateTime.Now : GetDateTimeForTimezoneId(timeZoneId), toolName, toolDescription);
    }

    private static DateTime GetDateTimeForTimezoneId(string timeZoneId)
    {
        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
    }
}
