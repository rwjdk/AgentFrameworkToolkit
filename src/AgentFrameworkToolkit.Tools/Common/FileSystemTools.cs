using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools for file system operations
/// </summary>
[PublicAPI]
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
    public static AITool GetContentOfFile(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath, options);
            return File.ReadAllText(filePath, options?.Encoding ?? Encoding.UTF8);
        }, toolName ?? "get_content_of_file", toolDescription ?? "Read and return the text content of a file");
    }

    /// <summary>
    /// Get FilePaths for a Local Folder Path
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetFiles(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create(
            (string folderPath, string searchPattern = "*",
                SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            {
                GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
                return Directory.GetFiles(folderPath, searchPattern, searchOption);
            }, toolName ?? "get_files", toolDescription ?? "Get a list of file paths in a folder");
    }

    /// <summary>
    /// Get FolderPaths for a Local Folder Path
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool GetFolders(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create(
            (string folderPath, string searchPattern = "*",
                SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            {
                GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
                return Directory.GetDirectories(folderPath, searchPattern, searchOption);
            }, toolName ?? "get_folders", toolDescription ?? "Get a list of folder paths in a folder");
    }

    /// <summary>
    /// Check if a Local File Exists
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool FileExists(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath, options);
            return File.Exists(filePath);
        }, toolName ?? "file_exists", toolDescription ?? "Check if a file exists at the specified path");
    }

    /// <summary>
    /// Check if a Local Folder Exists
    /// </summary>
    /// <param name="options">Optional options</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool FolderExists(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string folderPath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
            return Directory.Exists(folderPath);
        }, toolName ?? "folder_exists", toolDescription ?? "Check if a folder exists at the specified path");
    }

    /// <summary>
    /// Write Text Content to Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CreateFile(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
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
        }, toolName ?? "create_file", toolDescription ?? "Create a new file with the specified content");
    }

    /// <summary>
    /// Create a Local Folder
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CreateFolder(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string folderPath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }, toolName ?? "create_folder", toolDescription ?? "Create a new folder at the specified path");
    }

    /// <summary>
    /// Move a Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool MoveFile(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string sourceFilePath, string destinationFilePath, bool overwrite = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(sourceFilePath, options);
            GuardThatOperationsAreWithinConfinedFolderPaths(destinationFilePath, options);
            File.Move(sourceFilePath, destinationFilePath, overwrite);
            return destinationFilePath;
        }, toolName ?? "move_file", toolDescription ?? "Move a file from source path to destination path");
    }

    /// <summary>
    /// Delete a Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool DeleteFile(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string filePath) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(filePath, options);
            File.Delete(filePath);
            return filePath;
        }, toolName ?? "delete_file", toolDescription ?? "Delete a file at the specified path");
    }

    /// <summary>
    /// Delete a Local Folder
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool DeleteFolder(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string folderPath, bool recursive = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(folderPath, options);
            Directory.Delete(folderPath, recursive);
            return folderPath;
        }, toolName ?? "delete_folder", toolDescription ?? "Delete a folder at the specified path");
    }

    /// <summary>
    /// Copy a Local File
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CopyFile(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string sourceFilePath, string destinationFilePath, bool overwrite = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(sourceFilePath, options);
            GuardThatOperationsAreWithinConfinedFolderPaths(destinationFilePath, options);
            File.Copy(sourceFilePath, destinationFilePath, overwrite);
            return destinationFilePath;
        }, toolName ?? "copy_file", toolDescription ?? "Copy a file from source path to destination path");
    }

    /// <summary>
    /// Copy a Local Folder
    /// </summary>
    /// <param name="options">Optional Options to use</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>Tool</returns>
    public static AITool CopyFolder(FileSystemToolsOptions? options = null, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create((string sourceFolderPath, string destinationFolderPath, bool overwrite = false) =>
        {
            GuardThatOperationsAreWithinConfinedFolderPaths(sourceFolderPath, options);
            GuardThatOperationsAreWithinConfinedFolderPaths(destinationFolderPath, options);
            CopyDirectory(sourceFolderPath, destinationFolderPath, overwrite);
            return destinationFolderPath;
        }, toolName ?? "copy_folder", toolDescription ?? "Copy a folder and its contents from source path to destination path");
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
[PublicAPI]
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
