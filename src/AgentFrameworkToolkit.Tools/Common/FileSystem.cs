using System.Text;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools for reading files
/// </summary>
public static class FileSystem
{
    /// <summary>
    /// Get All the Tools with their default settings
    /// </summary>
    /// <returns></returns>
    public static IList<AITool> AllTools(FileSystemToolsOptions? options = null)
    {
        List<AITool> tools = [];
        tools.AddRange(AllReadTools(options));
        tools.AddRange(AllWriteTools(options));
        return tools;
    }

    /// <summary>
    /// Get All the Read Tools with their default settings
    /// </summary>
    /// <returns></returns>
    public static IList<AITool> AllReadTools(FileSystemToolsOptions? options = null)
    {
        return
        [
            GetFileContentAsText(options),
            GetFilesInFolder(options)
        ];
    }

    /// <summary>
    /// Get All the Write Tools with their default settings
    /// </summary>
    /// <returns></returns>
    public static IList<AITool> AllWriteTools(FileSystemToolsOptions? options = null)
    {
        return
        [
            //todo
        ];
    }

    /// <summary>
    /// Get Text Content of Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetFileContentAsText(FileSystemToolsOptions? options = null, string? toolName = "get_file_content_as_text",
        string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath, options);
            return File.ReadAllText(filePath, options?.Encoding ?? Encoding.UTF8);
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Get FilePaths for a Local Folder Path
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetFilesInFolder(FileSystemToolsOptions? options = null, string toolName = "get_files_in_folder_path", string? toolDescription = null)
    {
        return AIFunctionFactory.Create(
            (string folderPath, string searchPattern = "*",
                SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            {
                GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
                return Directory.GetFiles(folderPath, searchPattern, searchOption);
            }, toolName, toolDescription);
    }

    private static void GuardThatOperationsAreWithinConfinedFolderPaths(string folderPath, FileSystemToolsOptions? options)
    {
        if (options?.ConfinedToTheseFolderPaths == null)
        {
            return; //No confinements defined
        }

        if (options.ConfinedToTheseFolderPaths.Any(x => folderPath.StartsWith(x, StringComparison.CurrentCultureIgnoreCase)))
        {
            return; //Allowed Folder
        }

        //If we reach here, then it means that we are not in an allowed folder
        throw new Exception($"Operations on FolderPath '{folderPath}' is not defined as an allowed Path");
    }
}

/// <summary>
/// Options for the File System Tools
/// </summary>
public class FileSystemToolsOptions
{
    /// <summary>
    /// If set all operations will be checked if they happen within these folder-paths (+ subfolders) (if not set then no restrictions apply)
    /// </summary>
    public IList<string>? ConfinedToTheseFolderPaths { get; set; }

    /// <summary>
    /// Encoding to use for read/write
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
