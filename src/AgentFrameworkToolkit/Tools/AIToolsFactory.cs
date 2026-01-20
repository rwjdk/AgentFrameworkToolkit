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
    /// Get Tools from various services
    /// </summary>
    /// <param name="toolsWithAttributeFromObjectInstances"></param>
    /// <param name="toolsWithAttributeFromTypes"></param>
    /// <param name="toolsFromAgentSkillFolders"></param>
    /// <param name="timeTools">Time Tools to Include</param>
    /// <param name="otherTools">Other own or common tools you create</param>
    /// <returns>A List of Tools</returns>
    public IList<AITool> GetToolsFromMultipleSources(
        IEnumerable<object>? toolsWithAttributeFromObjectInstances = null,
        IEnumerable<Type>? toolsWithAttributeFromTypes = null,
        IEnumerable<AgentSkillFolder>? toolsFromAgentSkillFolders = null,
        TimeTool? timeTools = null,
        IEnumerable<AITool>? otherTools = null
    )
    {
        List<AITool> tools = [];

        if (toolsWithAttributeFromObjectInstances != null)
        {
            TryAddTools(GetTools(toolsWithAttributeFromObjectInstances.ToArray()));
        }

        if (toolsWithAttributeFromTypes != null)
        {
            TryAddTools(GetTools(toolsWithAttributeFromTypes.ToArray()));
        }

        if (toolsFromAgentSkillFolders != null)
        {
            foreach (AgentSkillFolder folder in toolsFromAgentSkillFolders)
            {
                TryAddTools(GetToolsFromAgentSkills(folder.Path, folder.Options).GetAsTools());
            }
        }

        if (timeTools.HasValue)
        {
            TryAddTools(new TimeTools().Get(timeTools.Value));
        }

        if (otherTools != null)
        {
            TryAddTools(otherTools);
        }


        return tools;

        void TryAddTool(AITool tool)
        {
            if (tools.All(x => x.Name != tool.Name))
            {
                tools.Add(tool);
            }
        }

        void TryAddTools(IEnumerable<AITool> toolsToAdd)
        {
            foreach (AITool tool in toolsToAdd)
            {
                TryAddTool(tool);
            }
        }
    }

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
/// Settings for an AgentSkill Folder
/// </summary>
/// <param name="Path"></param>
/// <param name="Options"></param>
public record AgentSkillFolder(string Path, AgentSkillsOptions? Options = null);
