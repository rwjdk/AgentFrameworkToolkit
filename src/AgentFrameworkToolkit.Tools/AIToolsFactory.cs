using System.Reflection;
using AgentFrameworkToolkit.Tools.Common;
using AgentSkillsDotNet;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools;

/// <summary>
/// A Factory to get AI Tools from Classes and Methods
/// </summary>
public class AIToolsFactory
{
    /// <summary>
    /// Get tools from AgentSkills
    /// </summary>
    /// <param name="folderPath">The Local folder with skills sub-folders</param>
    /// <param name="agentSkillsOptions">Options when getting skills</param>
    /// <returns>Tools related to the AgentTools</returns>
    public AgentSkills GetToolsFromAgentSkills(string folderPath, AgentSkillsOptions? agentSkillsOptions = null)
    {
        AgentSkillsFactory agentSkillsFactory = new();
        return agentSkillsFactory.GetAgentSkills(folderPath, agentSkillsOptions);
    }

    /// <summary>
    /// Get all Tools in an object Type with the [AITool] attribute (require an empty constructor)
    /// </summary>
    /// <param name="type">An object type</param>
    /// <returns>AI Tools collection</returns>
    private IList<AITool> GetTools(Type type)
    {
        if (type.IsAbstract)
        {
            List<AITool> staticTools = [];
            IEnumerable<MethodInfo> methodsWithAttribute = GetMethodsWithAttribute(type);
            foreach (MethodInfo methodInfo in methodsWithAttribute)
            {
                AIToolAttribute definition = methodInfo.GetCustomAttribute<AIToolAttribute>()!;
                staticTools.Add(AIFunctionFactory.Create(methodInfo, name: definition.Name, description: definition.Description, target: null));
            }

            return staticTools;
        }

        object instance = Activator.CreateInstance(type)!;
        return GetTools(instance);
    }

    /// <summary>
    /// Get all Tools in an object instance with the [AITool] attribute
    /// </summary>
    /// <param name="objectInstance">An object instance</param>
    /// <returns>AI Tools collection</returns>
    private IList<AITool> GetTools(object objectInstance)
    {
        Type type = objectInstance.GetType();
        IEnumerable<MethodInfo> methods = GetMethodsWithAttribute(type);
        AITool[] tools = methods.Select(methodInfo =>
        {
            AIToolAttribute definition = methodInfo.GetCustomAttribute<AIToolAttribute>()!;
            return AIFunctionFactory.Create(methodInfo, objectInstance, name: definition.Name, description: definition.Description);
        }).Cast<AITool>().ToArray();
        return tools;
    }

    /// <summary>
    /// Get all Tools in a collection of Types with the [AITool] attribute (each require an empty constructor)
    /// </summary>
    /// <param name="types">A collection of Types to get tools from</param>
    /// <returns>AI Tools collection</returns>
    public IList<AITool> GetTools(params Type[] types)
    {
        List<AITool> result = [];
        foreach (Type type in types)
        {
            result.AddRange(GetTools(type));
        }

        return result;
    }

    /// <summary>
    /// Get all Tools in a collection of object instances with the [AITool] attribute
    /// </summary>
    /// <param name="objectInstances">A collection of object instances to get tools from</param>
    /// <returns>AI Tools collection</returns>
    public IList<AITool> GetTools(params object[] objectInstances)
    {
        List<AITool> result = [];
        foreach (object instance in objectInstances)
        {
            result.AddRange(GetTools(instance));
        }

        return result;
    }

    /// <summary>
    /// Get Time-related Tools
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <returns>Tools</returns>
    public IList<AITool> GetTimeTools(GetTimeToolsOptions? options = null)
    {
        GetTimeToolsOptions optionsToUse = options ?? new GetTimeToolsOptions();
        List<AITool> result = [];
        if (optionsToUse.GetLocalNow)
        {
            result.Add(TimeTools.GetNowLocal(
                optionsToUse.GetNowLocalOptions,
                optionsToUse.GetLocalNowToolName,
                optionsToUse.GetLocalNowToolDescription));
        }
        if (optionsToUse.GetUtcNow)
        {
            result.Add(TimeTools.GetNowUtc(
                optionsToUse.GetUtcNowToolName,
                optionsToUse.GetUtcNowToolDescription));
        }
        return result;
    }

    /// <summary>
    /// Get Website-related Tools
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <returns>Tools</returns>
    public IList<AITool> GetWebsiteTools(GetWebsiteToolsOptions? options = null)
    {
        if (options == null)
        {
            return WebsiteTools.All();
        }
        List<AITool> result = [];
        if (options.GetContentOfPage)
        {
            result.Add(WebsiteTools.GetContentOfPage(
                options.GetContentOfPageOptions,
                options.GetContentOfPageToolName,
                options.GetContentOfPageToolDescription));
        }
        return result;
    }

    private static IEnumerable<MethodInfo> GetMethodsWithAttribute(Type type)
    {
        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.FlattenHierarchy);

        return methods.Where(x => x.GetCustomAttribute<AIToolAttribute>() != null).ToList();
    }
}

/// <summary>
/// Options for GetWebsiteTools method
/// </summary>
public class GetWebsiteToolsOptions
{
    /// <summary>
    /// Include GetContentOfPage tool (Default: true)
    /// </summary>
    public bool GetContentOfPage { get; set; } = true;

    /// <summary>
    /// Options for GetContentOfPage tool
    /// </summary>
    public GetContentOfPageOptions? GetContentOfPageOptions { get; set; }

    /// <summary>
    /// Optional name override for GetContentOfPage tool (Default: null)
    /// </summary>
    public string? GetContentOfPageToolName { get; set; }

    /// <summary>
    /// Optional description override for GetContentOfPage tool (Default: null)
    /// </summary>
    public string? GetContentOfPageToolDescription { get; set; }
}

/// <summary>
/// Options for GetTimeTools method
/// </summary>
public class GetTimeToolsOptions
{
    /// <summary>
    /// Include GetUtcNow tool (Default: true)
    /// </summary>
    public bool GetUtcNow { get; set; } = true;

    /// <summary>
    /// Include GetLocalNow tool (Default: true)
    /// </summary>
    public bool GetLocalNow { get; set; } = true;

    /// <summary>
    /// Options for GetNowLocal tool
    /// </summary>
    public GetNowLocalOptions? GetNowLocalOptions { get; set; }

    /// <summary>
    /// Optional name override for GetUtcNow tool (Default: null)
    /// </summary>
    public string? GetUtcNowToolName { get; set; }

    /// <summary>
    /// Optional description override for GetUtcNow tool (Default: null)
    /// </summary>
    public string? GetUtcNowToolDescription { get; set; }

    /// <summary>
    /// Optional name override for GetLocalNow tool (Default: null)
    /// </summary>
    public string? GetLocalNowToolName { get; set; }

    /// <summary>
    /// Optional description override for GetLocalNow tool (Default: null)
    /// </summary>
    public string? GetLocalNowToolDescription { get; set; }
}
