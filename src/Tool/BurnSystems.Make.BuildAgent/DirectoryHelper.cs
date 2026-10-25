namespace BurnSystems.Make.BuildAgent;

/// <summary>
/// Some helper for Directories
/// </summary>
public static class DirectoryHelper
{
    /// <summary>
    /// Copies the files from source directory to targetdirectory without recursion
    /// </summary>
    /// <param name="sourceDirectory">Directory from which the files are copied</param>
    /// <param name="targetDirectory">Directory to which the files are copied</param>
    /// <param name="overwrite">true, if existing files shall be overwritten</param>
    /// <param name="searchPattern">The pattern of the filename</param>
    public static void CopyFiles(string sourceDirectory, string targetDirectory, bool overwrite = true, string searchPattern = "*.*")
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var sourceFile in Directory.GetFiles(sourceDirectory, searchPattern))
        {
            var filename = Path.GetFileName(sourceFile);
            var targetFile = Path.Combine(targetDirectory, filename);
            File.Copy(sourceFile, targetFile, overwrite);
        }
    }

    /// <summary>
    /// Copies files from the source directory to the target directory recursively, including subdirectories.
    /// </summary>
    /// <param name="sourceDirectory">The directory from which files are copied.</param>
    /// <param name="targetDirectory">The directory to which files are copied.</param>
    /// <param name="overwrite">Indicates whether existing files should be overwritten.</param>
    /// <param name="searchPattern">The pattern used to match file names.</param>
    /// <returns>None</returns>
    /// <exception cref="InvalidOperationException">Thrown when the target file has no parent directory.</exception>
    public static void CopyFilesRecursively(string sourceDirectory, string targetDirectory, bool overwrite = true,
        string searchPattern = "*")
    {
        foreach (var sourceFile in Directory.GetFiles(sourceDirectory, searchPattern, SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
            var targetFile = Path.Combine(targetDirectory, relativePath);
            var targetParentDirectory = Path.GetDirectoryName(targetFile)
                                        ?? throw new InvalidOperationException("Target file has no parent directory");

            Directory.CreateDirectory(targetParentDirectory);
            File.Copy(sourceFile, targetFile, overwrite: overwrite);
        }
    }
}