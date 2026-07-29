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
        foreach (var sourceFile in Directory.GetFiles(sourceDirectory, searchPattern))
        {
            var filename = Path.GetFileName(sourceFile);
            var targetFile = Path.Combine(targetDirectory, filename);
            File.Copy(sourceFile, targetFile, overwrite);
        }
    }
}