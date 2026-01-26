using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Date and Time-based tools
/// </summary>
[PublicAPI]
public static class TimeTools
{
    /// <summary>
    /// All the Time Tools with their default settings
    /// </summary>
    /// <returns>Tools</returns>
    public static IList<AITool> All(GetNowLocalOptions? getNowLocalOptions = null)
    {
        return [
            GetNowLocal(getNowLocalOptions),
            GetNowUtc()
        ];
    }

    /// <summary>
    /// UTC Now
    /// </summary>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetNowUtc(string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create(() => DateTime.UtcNow, toolName ?? "get_now_utc", description: toolDescription ?? "Get current UTC time");
    }

    /// <summary>
    /// Local Now (Machine, defined by options or AI provided timeZoneId)
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetNowLocal(GetNowLocalOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        GetNowLocalOptions getNowLocalOptions = options ?? new GetNowLocalOptions();

        if (getNowLocalOptions.IncludeTimezoneParameter)
        {
            return AIFunctionFactory.Create((string? timeZoneId = null) =>
            {
                timeZoneId ??= getNowLocalOptions.DefaultLocalTimezoneIdIfNoneIsProvided;
                return string.IsNullOrWhiteSpace(timeZoneId) ? DateTime.Now : GetDateTimeForTimezoneId(timeZoneId);
            }, toolName ?? "get_now_local", toolDescription ?? "Get current local time");
        }
        else
        {
            return AIFunctionFactory.Create(() =>
            {
                string? timeZoneId = getNowLocalOptions.DefaultLocalTimezoneIdIfNoneIsProvided;
                return string.IsNullOrWhiteSpace(timeZoneId) ? DateTime.Now : GetDateTimeForTimezoneId(timeZoneId);
            }, toolName ?? "get_now_local", toolDescription ?? "Get current local time");
        }
    }

    private static DateTime GetDateTimeForTimezoneId(string timeZoneId)
    {
        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
    }
}

/// <summary>
/// Options for GetNowLocal Tool
/// </summary>
[PublicAPI]
public class GetNowLocalOptions
{
    /// <summary>
    /// Include an optional TimezoneId parameter for the AI to set (default = true)
    /// </summary>
    public bool IncludeTimezoneParameter { get; set; } = true;

    /// <summary>
    /// The timezone Id (https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.getsystemtimezones)
    /// </summary>
    public string? DefaultLocalTimezoneIdIfNoneIsProvided { get; set; }
}
