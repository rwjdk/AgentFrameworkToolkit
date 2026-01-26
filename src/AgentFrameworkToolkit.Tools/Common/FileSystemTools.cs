using System.Text;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools for file system operations
/// </summary>
public static class FileSystemTools
{
    /// <summary>
    /// Get All the Tools with their default settings
    /// </summary>
    /// <returns></returns>
    public static IList<AITool> All(FileSystemToolsOptions? options = null)
    {
        List<AITool> tools = [];
        tools.AddRange(AllRead(options));
        tools.AddRange(AllWrite(options));
        return tools;
    }

    /// <summary>
    /// Get All the Read Tools with their default settings
    /// </summary>
    /// <returns></returns>
    public static IList<AITool> AllRead(FileSystemToolsOptions? options = null)
    {
        return
        [
            GetContentOfFile(options),
            GetFiles(options),
            GetFolders(options),
            FileExists(options),
            FolderExists(options)
        ];
    }

    /// <summary>
    /// Get All the Write Tools with their default settings
    /// </summary>
    /// <returns></returns>
    public static IList<AITool> AllWrite(FileSystemToolsOptions? options = null)
    {
        return
        [
            CreateFile(options),
            CreateFolder(options),
            MoveFile(options),
            DeleteFile(options),
            DeleteFolder(options),
            CopyFile(options),
            CopyFolder(options)
        ];
    }

    /// <summary>
    /// Get Text Content of Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetContentOfFile(FileSystemToolsOptions? options = null, string? toolName = "get_content_of_file", string? toolDescription = null)
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
    public static AITool GetFiles(FileSystemToolsOptions? options = null, string toolName = "get_files", string? toolDescription = null)
    {
        return AIFunctionFactory.Create(
            (string folderPath, string searchPattern = "*",
                SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            {
                GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
                return Directory.GetFiles(folderPath, searchPattern, searchOption);
            }, toolName, toolDescription);
    }

    /// <summary>
    /// Get FolderPaths for a Local Folder Path
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetFolders(FileSystemToolsOptions? options = null, string toolName = "get_folders", string? toolDescription = null)
    {
        return AIFunctionFactory.Create(
            (string folderPath, string searchPattern = "*",
                SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            {
                GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
                return Directory.GetDirectories(folderPath, searchPattern, searchOption);
            }, toolName, toolDescription);
    }

    /// <summary>
    /// Check if a Local File Exists
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool FileExists(FileSystemToolsOptions? options = null, string toolName = "file_exists", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath, options);
            return File.Exists(filePath);
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Check if a Local Folder Exists
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool FolderExists(FileSystemToolsOptions? options = null, string toolName = "folder_exists", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string folderPath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
            return Directory.Exists(folderPath);
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Write Text Content to Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CreateFile(FileSystemToolsOptions? options = null, string? toolName = "create_file", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath, string content, bool overwrite = true) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath, options);

            if (!overwrite && File.Exists(filePath))
            {
                throw new IOException($"File '{filePath}' already exists.");
            }

            File.WriteAllText(filePath, content, options?.Encoding ?? Encoding.UTF8);
            return filePath;
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Create a Local Folder
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CreateFolder(FileSystemToolsOptions? options = null, string toolName = "create_folder", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string folderPath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Move a Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool MoveFile(FileSystemToolsOptions? options = null, string toolName = "move_file", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string sourceFilePath, string destinationFilePath, bool overwrite = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(sourceFilePath, options);
            GuardThatOperationsAreWithinConfinedFolderPaths(destinationFilePath, options);
            File.Move(sourceFilePath, destinationFilePath, overwrite);
            return destinationFilePath;
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Delete a Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool DeleteFile(FileSystemToolsOptions? options = null, string toolName = "delete_file", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath, options);
            File.Delete(filePath);
            return filePath;
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Delete a Local Folder
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool DeleteFolder(FileSystemToolsOptions? options = null, string toolName = "delete_folder", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string folderPath, bool recursive = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
            Directory.Delete(folderPath, recursive);
            return folderPath;
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Copy a Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CopyFile(FileSystemToolsOptions? options = null, string toolName = "copy_file", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string sourceFilePath, string destinationFilePath, bool overwrite = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(sourceFilePath, options);
            GuardThatOperationsAreWithinConfinedFolderPaths(destinationFilePath, options);
            File.Copy(sourceFilePath, destinationFilePath, overwrite);
            return destinationFilePath;
        }, toolName, toolDescription);
    }

    /// <summary>
    /// Copy a Local Folder
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CopyFolder(FileSystemToolsOptions? options = null, string toolName = "copy_folder", string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string sourceFolderPath, string destinationFolderPath, bool overwrite = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(sourceFolderPath, options);
            GuardThatOperationsAreWithinConfinedFolderPaths(destinationFolderPath, options);
            CopyDirectory(sourceFolderPath, destinationFolderPath, overwrite);
            return destinationFolderPath;
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

    private static void CopyDirectory(string sourceFolderPath, string destinationFolderPath, bool overwrite)
    {
        DirectoryInfo sourceDirectory = new(sourceFolderPath);
        if (!sourceDirectory.Exists)
        {
            throw new DirectoryNotFoundException($"Source folder not found: '{sourceFolderPath}'.");
        }

        DirectoryInfo destinationDirectory = Directory.CreateDirectory(destinationFolderPath);
        foreach (FileInfo file in sourceDirectory.GetFiles())
        {
            string destinationFilePath = Path.Combine(destinationDirectory.FullName, file.Name);
            file.CopyTo(destinationFilePath, overwrite);
        }

        foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
        {
            string destinationSubFolderPath = Path.Combine(destinationDirectory.FullName, subDirectory.Name);
            CopyDirectory(subDirectory.FullName, destinationSubFolderPath, overwrite);
        }
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
