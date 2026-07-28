using System.Diagnostics;
using BurnSystems.Logging;

namespace BurnSystems.Make.BuildAgent;

/// <summary>
/// Defines the parameters which start a specific process
/// </summary>
public class ProcessInvokeParameter
{
    public required string Command { get; set; }
    public required string[] Arguments { get; set; }
    public bool SearchInPath { get; set; } = true;
}

/// <summary>
/// Helper class to invoke a process
/// </summary>
public static class ProcessInvoke
{
    /// <summary>
    /// Stores the logger
    /// </summary>
    private static readonly ILogger Logger = new ClassLogger(typeof(ProcessInvoke));
    
    /// <summary>
    /// Runs a process and returns the exit code
    /// </summary>
    /// <param name="command">Command To be invoked</param>
    /// <param name="arguments">Arguments of the command</param>
    /// <returns></returns>
    public static async Task<int> Run(string command, string[] arguments)
    {
        return await Run(new ProcessInvokeParameter
        {
            Command = command,
            Arguments = arguments
        });
    }

    public static async Task<int> Run(ProcessInvokeParameter parameter)
    {
        // Check, if executable is existing, otherwise check in path, if required
        var command = parameter.Command;
        var realCommand = CheckFileForExistings(".", command);
        if (realCommand == null)
        {
            // Check in path
            if (parameter.SearchInPath)
            {
                foreach (var directory in GetPathDirectories())
                {
                    realCommand = CheckFileForExistings(directory, command);
                    if (realCommand != null)
                    {
                        break;
                    }
                }
            }
        }

        if (realCommand == null)
        {
            throw new InvalidOperationException($"{parameter.Command} was not found.");
        }
        
        Logger.Info("Executing command: " + realCommand);
        
        var process = Process.Start(realCommand, parameter.Arguments);
        await process.WaitForExitAsync();

        return process.ExitCode;
    }
    

    /// <summary>
    /// Gets all directories which are in the path variable
    /// </summary>
    /// <returns></returns>
    private static List<string> GetPathDirectories()
    {
        var result = new List<string>();
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path)) return result;
        
        var separator = new[] { EnvironmentHelper.IsUnix() ? ':' : ';' };
        var paths = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in paths)
        {
            result.Add(p.Trim(' ', '"', '\''));
        }

        return result;
    }

    /// <summary>
    /// Checks, if a certain command exists at the directory including some alternative filenames 
    /// </summary>
    /// <param name="directory">Directory to be checked</param>
    /// <param name="command">Command which might be modified</param>
    /// <returns></returns>
    public static string? CheckFileForExistings(string directory, string command)
    {
        foreach (var commandFilename in GetFilenames(command))
        {
            var path = Path.Combine(directory, commandFilename);
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all potential filenames 
    /// </summary>
    /// <param name="binaryFileName">Name of the file which shall be evaluated</param>
    /// <returns>An enumeration of potential files</returns>
    public static IEnumerable<string> GetFilenames(string binaryFileName)
    {
        if (binaryFileName.EndsWith(".exe") || binaryFileName.EndsWith(".cmd") || binaryFileName.EndsWith(".bat"))
        {
            return [binaryFileName];
        }

        if (EnvironmentHelper.IsWindows())
        {
            return [binaryFileName + ".exe", binaryFileName + ".cmd", binaryFileName + ".bat"];
        }
        
        return [binaryFileName, binaryFileName + ".exe", binaryFileName + ".cmd", binaryFileName + ".bat"];
    }
}