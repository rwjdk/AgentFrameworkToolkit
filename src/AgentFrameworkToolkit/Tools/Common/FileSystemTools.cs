using System.Text;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools for reading files
/// </summary>
public class FileSystemTools
{
    private readonly FileSystemToolsOptions _options;

    /// <summary>
    /// Tools for reading files
    /// </summary>
    public FileSystemTools(FileSystemToolsOptions? options = null)
    {
        _options = options ?? new FileSystemToolsOptions
        {
            Encoding = Encoding.UTF8
        };
    }

    /// <summary>
    /// Get All Read-based File System Tools
    /// </summary>
    /// <returns></returns>
    public IList<AITool> AllRead()
    {
        return
        [
            GetFileContentAsText(),
            GetFilesInFolder()
        ];
    }

    /// <summary>
    /// Get All Write-based File System Tools
    /// </summary>
    /// <returns></returns>
    public IList<AITool> AllWrite()
    {
        return
        [
            //todo
        ];
    }

    /// <summary>
    /// Get Text Content of Local File
    /// </summary>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public AITool GetFileContentAsText(string? toolName = "get_file_content_as_text", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath);
            return File.ReadAllText(filePath, _options.Encoding);
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Get FilePaths for a Local Folder Path
    /// </summary>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public AITool GetFilesInFolder(string? toolName = "get_files_in_folder_path", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string folderPath, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(folderPath);
            return Directory.GetFiles(folderPath, searchPattern, searchOption);
        }, toolName, toolDescription);
    }

    private void GuardThatOperationsAreWithinConfinedFolderPaths(string folderPath)
    {
        if (_options.ConfinedToTheseFolderPaths == null)
        {
            return; //No confinements defined
        }

        if (_options.ConfinedToTheseFolderPaths.Any(x => folderPath.StartsWith(x, StringComparison.CurrentCultureIgnoreCase)))
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
