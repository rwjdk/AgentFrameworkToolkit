using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Date and Time based tools
/// </summary>
public static class Time
{
    /// <summary>
    /// All the Time Tools with their default settings
    /// </summary>
    /// <returns>Tools</returns>
    public static IList<AITool> AllTools(GetLocalNowOptions? getLocalNowOptions = null)
    {
        return [
            GetLocalNowTool(getLocalNowOptions),
            GetUtcNowTool()
        ];
    }


    /// <summary>
    /// UTC Now
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetUtcNowTool(string ? toolName = "get_utc_now", string? toolDescription = null)
    {
        return AIFunctionFactory.Create(() => DateTime.UtcNow, toolName, description: toolDescription);
    }

    /// <summary>
    /// Local Now (Machine or provided timeZoneId)
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public static AITool GetLocalNowTool(GetLocalNowOptions? options = null, string? toolName = "get_local_now", string? toolDescription = null)
    {
        GetLocalNowOptions getLocalNowOptions = options ?? new GetLocalNowOptions();
        return AIFunctionFactory.Create((string? timeZoneId = null) =>
        {
            timeZoneId ??= getLocalNowOptions.DefaultLocalTimezoneIdIfNoneIsProvided;
            return string.IsNullOrWhiteSpace(timeZoneId) ? DateTime.Now : GetDateTimeForTimezoneId(timeZoneId);
        }, toolName, toolDescription);
    }

    private static DateTime GetDateTimeForTimezoneId(string timeZoneId)
    {
        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
    }
}

/// <summary>
/// Options for GetLocalNowTool
/// </summary>
public class GetLocalNowOptions
{
    /// <summary>
    /// The timezone Id (https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.getsystemtimezones)
    /// </summary>
    public string? DefaultLocalTimezoneIdIfNoneIsProvided { get; set; }
}
