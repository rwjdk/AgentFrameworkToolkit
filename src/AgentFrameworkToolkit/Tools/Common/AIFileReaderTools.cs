using System.Text;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools for reading files
/// </summary>
public static class AIFileReaderTools
{
    /// <summary>
    /// Get Text Content of Local File
    /// </summary>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetFileContentAsText(string? toolName = "get_file_content_as_text", string? toolDescription = null)
    {
        //todo - add options
        return AIFunctionFactory.Create((string filePath) => File.ReadAllText(filePath, Encoding.UTF8), toolName, toolDescription);
    }

    /// <summary>
    /// Get FilePaths for a Local Folder Path
    /// </summary>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetFilesInFolder(string? toolName = "get_files_in_folder_path", string? toolDescription = null)
    {
        //todo - add options
        return AIFunctionFactory.Create((string folderPath, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) => Directory.GetFiles(folderPath, searchPattern, searchOption), toolName, toolDescription);
    }
}
