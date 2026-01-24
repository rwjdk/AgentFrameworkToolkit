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
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetNowUtc(string ? toolName = "get_now_utc", string? toolDescription = "Get current UTC time")
    {
        return AIFunctionFactory.Create(() => DateTime.UtcNow, toolName, description: toolDescription);
    }

    /// <summary>
    /// Local Now (Machine, defined by options or AI provided timeZoneId)
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetNowLocal(GetNowLocalOptions? options = null, string? toolName = "get_now_local", string? toolDescription = "Get current local time")
    {
        GetNowLocalOptions getNowLocalOptions = options ?? new GetNowLocalOptions();

        if (getNowLocalOptions.IncludeTimezoneParameter)
        {
            return AIFunctionFactory.Create((string? timeZoneId = null) =>
            {
                timeZoneId ??= getNowLocalOptions.DefaultLocalTimezoneIdIfNoneIsProvided;
                return string.IsNullOrWhiteSpace(timeZoneId) ? DateTime.Now : GetDateTimeForTimezoneId(timeZoneId);
            }, toolName, toolDescription);
        }
        else
        {
            return AIFunctionFactory.Create(() =>
            {
                string? timeZoneId = getNowLocalOptions.DefaultLocalTimezoneIdIfNoneIsProvided;
                return string.IsNullOrWhiteSpace(timeZoneId) ? DateTime.Now : GetDateTimeForTimezoneId(timeZoneId);
            }, toolName, toolDescription);
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
