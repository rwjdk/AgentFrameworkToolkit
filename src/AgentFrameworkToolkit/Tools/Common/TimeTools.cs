using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Date and Time based tools
/// </summary>
public class TimeTools
{
    private readonly TimeToolsOptions _options;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="option">Options for the TimeTools</param>
    public TimeTools(TimeToolsOptions? option = null)
    {
        _options = option ?? new TimeToolsOptions();
    }

    /// <summary>
    /// Get All time Tools
    /// </summary>
    /// <returns></returns>
    public IList<AITool> GetAll()
    {
        return Get(TimeTool.All);
    }

    /// <summary>
    /// Get Time Tools with their default names (new up individual tools for more control)
    /// </summary>
    /// <returns>Time Tools</returns>
    public IList<AITool> Get(TimeTool tool)
    {
        List<AITool> tools = [];
        if (tool.HasFlag(TimeTool.GetLocalNow))
        {
            tools.Add(GetLocalNow());
        }

        if (tool.HasFlag(TimeTool.GetUtcNow))
        {
            tools.Add(GetUtcNow());
        }

        return tools;
    }

    /// <summary>
    /// UTC Now
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public AITool GetUtcNow(string? toolName = "get_utc_now", string? toolDescription = null)
    {
        return AIFunctionFactory.Create(() => DateTime.UtcNow, toolName, description: toolDescription);
    }

    /// <summary>
    /// Local Now (Machine or provided timeZoneId)
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns>DateTimeOffset</returns>
    public AITool GetLocalNow(string? toolName = "get_local_now", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string? timeZoneId = null) =>
        {
            timeZoneId ??= _options.DefaultLocalTimezoneIdIfNoneIsProvided;
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
/// Time tools to include
/// </summary>
[Flags]
public enum TimeTool
{
    /// <summary>
    /// No tools
    /// </summary>
    None = 0,

    /// <summary>
    /// All the tools
    /// </summary>
    All = GetUtcNow | GetLocalNow,

    /// <summary>
    /// Get the Current UTC Time
    /// </summary>
    GetUtcNow = 1,

    /// <summary>
    /// Get the Local Time (Machine specific or based on the provided TimeZoneId)
    /// </summary>
    GetLocalNow = 2,
}

/// <summary>
/// Options for the Time Tools
/// </summary>
public class TimeToolsOptions
{
    /// <summary>
    /// The timezone Id (https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.getsystemtimezones)
    /// </summary>
    public string? DefaultLocalTimezoneIdIfNoneIsProvided { get; set; }
}

/// <summary>
/// If Date/Time should be return in local or UTC
/// </summary>
public enum TimeToolsKind
{
    /// <summary>
    /// Local (Note: this is the machines running this codes Local Time)
    /// </summary>
    Local,

    /// <summary>
    /// UTC
    /// </summary>
    Utc
}
