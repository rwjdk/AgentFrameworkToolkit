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

    /// <summary>
    /// Get OpenWeatherMap Tools
    /// </summary>
    /// <param name="openWeatherMapOptions">Required OpenWeatherMap configuration</param>
    /// <param name="options">Optional customization options</param>
    /// <returns>Tools</returns>
    public IList<AITool> GetWeatherTools(OpenWeatherMapOptions openWeatherMapOptions, GetOpenWeatherMapToolsOptions? options = null)
    {
        GetOpenWeatherMapToolsOptions optionsToUse = options ?? new GetOpenWeatherMapToolsOptions();
        List<AITool> result = [];
        if (optionsToUse.GetWeatherForCity)
        {
            result.Add(WeatherTools.GetWeatherForCity(
                openWeatherMapOptions,
                optionsToUse.GetWeatherForCityToolName,
                optionsToUse.GetWeatherForCityToolDescription));
        }
        return result;
    }

    /// <summary>
    /// Get File System-related Tools
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <returns>Tools</returns>
    public IList<AITool> GetFileSystemTools(GetFileSystemToolsOptions? options = null)
    {
        GetFileSystemToolsOptions optionsToUse = options ?? new GetFileSystemToolsOptions();
        List<AITool> result = [];
        if (optionsToUse.GetContentOfFile)
        {
            result.Add(FileSystemTools.GetContentOfFile(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.GetContentOfFileToolName,
                optionsToUse.GetContentOfFileToolDescription));
        }
        if (optionsToUse.GetFiles)
        {
            result.Add(FileSystemTools.GetFiles(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.GetFilesToolName,
                optionsToUse.GetFilesToolDescription));
        }
        if (optionsToUse.GetFolders)
        {
            result.Add(FileSystemTools.GetFolders(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.GetFoldersToolName,
                optionsToUse.GetFoldersToolDescription));
        }
        if (optionsToUse.FileExists)
        {
            result.Add(FileSystemTools.FileExists(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.FileExistsToolName,
                optionsToUse.FileExistsToolDescription));
        }
        if (optionsToUse.FolderExists)
        {
            result.Add(FileSystemTools.FolderExists(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.FolderExistsToolName,
                optionsToUse.FolderExistsToolDescription));
        }
        if (optionsToUse.CreateFile)
        {
            result.Add(FileSystemTools.CreateFile(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.CreateFileToolName,
                optionsToUse.CreateFileToolDescription));
        }
        if (optionsToUse.CreateFolder)
        {
            result.Add(FileSystemTools.CreateFolder(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.CreateFolderToolName,
                optionsToUse.CreateFolderToolDescription));
        }
        if (optionsToUse.MoveFile)
        {
            result.Add(FileSystemTools.MoveFile(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.MoveFileToolName,
                optionsToUse.MoveFileToolDescription));
        }
        if (optionsToUse.DeleteFile)
        {
            result.Add(FileSystemTools.DeleteFile(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.DeleteFileToolName,
                optionsToUse.DeleteFileToolDescription));
        }
        if (optionsToUse.DeleteFolder)
        {
            result.Add(FileSystemTools.DeleteFolder(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.DeleteFolderToolName,
                optionsToUse.DeleteFolderToolDescription));
        }
        if (optionsToUse.CopyFile)
        {
            result.Add(FileSystemTools.CopyFile(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.CopyFileToolName,
                optionsToUse.CopyFileToolDescription));
        }
        if (optionsToUse.CopyFolder)
        {
            result.Add(FileSystemTools.CopyFolder(
                optionsToUse.FileSystemToolsOptions,
                optionsToUse.CopyFolderToolName,
                optionsToUse.CopyFolderToolDescription));
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

/// <summary>
/// Options for GetWeatherTools method for provider OpenWeatherMap
/// </summary>
public class GetOpenWeatherMapToolsOptions
{
    /// <summary>
    /// Include GetWeatherForCity tool (Default: true)
    /// </summary>
    public bool GetWeatherForCity { get; set; } = true;

    /// <summary>
    /// Optional name override for GetWeatherForCity tool (Default: null)
    /// </summary>
    public string? GetWeatherForCityToolName { get; set; }

    /// <summary>
    /// Optional description override for GetWeatherForCity tool (Default: null)
    /// </summary>
    public string? GetWeatherForCityToolDescription { get; set; }
}

/// <summary>
/// Options for GetFileSystemTools method
/// </summary>
public class GetFileSystemToolsOptions
{
    /// <summary>
    /// Include GetContentOfFile tool (Default: true)
    /// </summary>
    public bool GetContentOfFile { get; set; } = true;

    /// <summary>
    /// Include GetFiles tool (Default: true)
    /// </summary>
    public bool GetFiles { get; set; } = true;

    /// <summary>
    /// Include GetFolders tool (Default: true)
    /// </summary>
    public bool GetFolders { get; set; } = true;

    /// <summary>
    /// Include FileExists tool (Default: true)
    /// </summary>
    public bool FileExists { get; set; } = true;

    /// <summary>
    /// Include FolderExists tool (Default: true)
    /// </summary>
    public bool FolderExists { get; set; } = true;

    /// <summary>
    /// Include CreateFile tool (Default: true)
    /// </summary>
    public bool CreateFile { get; set; } = true;

    /// <summary>
    /// Include CreateFolder tool (Default: true)
    /// </summary>
    public bool CreateFolder { get; set; } = true;

    /// <summary>
    /// Include MoveFile tool (Default: true)
    /// </summary>
    public bool MoveFile { get; set; } = true;

    /// <summary>
    /// Include DeleteFile tool (Default: true)
    /// </summary>
    public bool DeleteFile { get; set; } = true;

    /// <summary>
    /// Include DeleteFolder tool (Default: true)
    /// </summary>
    public bool DeleteFolder { get; set; } = true;

    /// <summary>
    /// Include CopyFile tool (Default: true)
    /// </summary>
    public bool CopyFile { get; set; } = true;

    /// <summary>
    /// Include CopyFolder tool (Default: true)
    /// </summary>
    public bool CopyFolder { get; set; } = true;

    /// <summary>
    /// Options for FileSystemTools
    /// </summary>
    public FileSystemToolsOptions? FileSystemToolsOptions { get; set; }

    /// <summary>
    /// Optional name override for GetContentOfFile tool (Default: null)
    /// </summary>
    public string? GetContentOfFileToolName { get; set; }

    /// <summary>
    /// Optional description override for GetContentOfFile tool (Default: null)
    /// </summary>
    public string? GetContentOfFileToolDescription { get; set; }

    /// <summary>
    /// Optional name override for GetFiles tool (Default: null)
    /// </summary>
    public string? GetFilesToolName { get; set; }

    /// <summary>
    /// Optional description override for GetFiles tool (Default: null)
    /// </summary>
    public string? GetFilesToolDescription { get; set; }

    /// <summary>
    /// Optional name override for GetFolders tool (Default: null)
    /// </summary>
    public string? GetFoldersToolName { get; set; }

    /// <summary>
    /// Optional description override for GetFolders tool (Default: null)
    /// </summary>
    public string? GetFoldersToolDescription { get; set; }

    /// <summary>
    /// Optional name override for FileExists tool (Default: null)
    /// </summary>
    public string? FileExistsToolName { get; set; }

    /// <summary>
    /// Optional description override for FileExists tool (Default: null)
    /// </summary>
    public string? FileExistsToolDescription { get; set; }

    /// <summary>
    /// Optional name override for FolderExists tool (Default: null)
    /// </summary>
    public string? FolderExistsToolName { get; set; }

    /// <summary>
    /// Optional description override for FolderExists tool (Default: null)
    /// </summary>
    public string? FolderExistsToolDescription { get; set; }

    /// <summary>
    /// Optional name override for CreateFile tool (Default: null)
    /// </summary>
    public string? CreateFileToolName { get; set; }

    /// <summary>
    /// Optional description override for CreateFile tool (Default: null)
    /// </summary>
    public string? CreateFileToolDescription { get; set; }

    /// <summary>
    /// Optional name override for CreateFolder tool (Default: null)
    /// </summary>
    public string? CreateFolderToolName { get; set; }

    /// <summary>
    /// Optional description override for CreateFolder tool (Default: null)
    /// </summary>
    public string? CreateFolderToolDescription { get; set; }

    /// <summary>
    /// Optional name override for MoveFile tool (Default: null)
    /// </summary>
    public string? MoveFileToolName { get; set; }

    /// <summary>
    /// Optional description override for MoveFile tool (Default: null)
    /// </summary>
    public string? MoveFileToolDescription { get; set; }

    /// <summary>
    /// Optional name override for DeleteFile tool (Default: null)
    /// </summary>
    public string? DeleteFileToolName { get; set; }

    /// <summary>
    /// Optional description override for DeleteFile tool (Default: null)
    /// </summary>
    public string? DeleteFileToolDescription { get; set; }

    /// <summary>
    /// Optional name override for DeleteFolder tool (Default: null)
    /// </summary>
    public string? DeleteFolderToolName { get; set; }

    /// <summary>
    /// Optional description override for DeleteFolder tool (Default: null)
    /// </summary>
    public string? DeleteFolderToolDescription { get; set; }

    /// <summary>
    /// Optional name override for CopyFile tool (Default: null)
    /// </summary>
    public string? CopyFileToolName { get; set; }

    /// <summary>
    /// Optional description override for CopyFile tool (Default: null)
    /// </summary>
    public string? CopyFileToolDescription { get; set; }

    /// <summary>
    /// Optional name override for CopyFolder tool (Default: null)
    /// </summary>
    public string? CopyFolderToolName { get; set; }

    /// <summary>
    /// Optional description override for CopyFolder tool (Default: null)
    /// </summary>
    public string? CopyFolderToolDescription { get; set; }
}
