using System.Diagnostics;
using System.Xml.Linq;
using BurnSystems.Logging;

namespace BSMake.Library;

/// <summary>
/// Defines the potential results of calling the build agent.
/// It just describes the positive events. Negative events are reported via an exception 
/// </summary>
public enum BuildAgentExecutionResult
{
    SuccessNoBuildAgentExisting, 
    SuccessBuildAgentExecuted
}

/// <summary>
/// Defines the potential types of a buildagent, including a non-existent one
/// </summary>
public enum BuildAgentType
{
    NotExisting,
    Executable,
    Library
}

/// <summary>
/// This binary build logic is responsible from sequence to request the execution of the build agent down to
/// actually executing the build agent.
/// It takes care that the build agent is compiled or a cached version is used,
/// it takes care to load the build agent and is responsible to evaluate and report potential failures.  
/// </summary>
public class BuildAgentLogic
{
    /// <summary>
    /// Stores the logger used to log messages
    /// </summary>
    private static readonly ILogger Logger = new ClassLogger(typeof(BuildAgentLogic));
    
    /// <summary>
    /// Stores the commandlinearguments being used to describe the current configuration
    /// </summary>
    private readonly CommandLineArguments _arguments;

    public BuildAgentLogic(CommandLineArguments arguments)
    {
        _arguments = arguments;
    }

    /// <summary>
    /// This is the main entry point which is called to start the full process from
    /// building the build agent down to executing it. 
    /// </summary>
    public async Task<BuildAgentExecutionResult> ExecuteBuildAgent()
    {
        var typeOfBuildAgent = GetTypeOfBuildAgent();
        Logger.Info($"Type of build agent is {typeOfBuildAgent}");
        
        switch (typeOfBuildAgent)
        {
            case BuildAgentType.NotExisting:
                return BuildAgentExecutionResult.SuccessNoBuildAgentExisting;
            case BuildAgentType.Library:
                throw new InvalidOperationException("Library is currently not supported");
            case BuildAgentType.Executable:
                // We have determined that we have an executable build agent
                await BuildBuildAgent();
                await TriggerBuildAgentAsExecutable();
                return BuildAgentExecutionResult.SuccessBuildAgentExecuted;
            default:
                throw new InvalidOperationException($"Unknown build agent type {typeOfBuildAgent}");
        }
    }

    /// <summary>
    /// Checks, if the build agent is existing. Here, the existing of a .csproj-file within the
    /// is determined. If the build agent is existing, the type of the build agent itself is reported
    /// </summary>
    /// <returns></returns>
    public BuildAgentType GetTypeOfBuildAgent()
    {
        // Gets the .csproj file, if existing
        var projectPath = Path.Combine(Environment.CurrentDirectory, ".bsmake");

        var csProjs = Directory.EnumerateFiles(projectPath, "*.csproj").ToList();
        switch (csProjs.Count)
        {
            case 0:
                return BuildAgentType.NotExisting;
            case 1:
                break;
            case 2:
                throw new InvalidOperationException("Multiple .csproj files in .bsmake directory");
        }
        
        var csProj = csProjs.Single();
        
        // Ok, we have the csproj-file, now determine the project type
        var document = XDocument.Load(csProj);
        var outputType =
            document.Root?
                .Elements("PropertyGroup")
                .Elements("OutputType")
                .FirstOrDefault();

        // Per default library
        if (outputType == null)
        {
            return BuildAgentType.Library;
        }
        
        // Otherwise, evaluate
        return outputType.Value switch
        {
            "Library" => BuildAgentType.Library,
            "Exe" => BuildAgentType.Executable,
            _ => throw new InvalidOperationException($"Unknown output type: {outputType.Value}")
        };
    }

    private async Task BuildBuildAgent()
    {
        Logger.Info("Triggered building of agent");

        // Figures out age of the build agent itself
        if (Directory.Exists("./.bsmake/bin"))
        {
            var exeFiles = Directory.GetFiles("./.bsmake/bin/", "*.exe", SearchOption.AllDirectories);
            var earliestExeFile = exeFiles.Select(File.GetLastWriteTime).Min();
            Logger.Info($"Earliest .exe file is {earliestExeFile}");

            // Figures out latest age of all .cs files stored recursively
            var csFiles = Directory.GetFiles("./.bsmake/", "*.cs", SearchOption.AllDirectories);
            var latestCsFile = csFiles.Select(File.GetLastWriteTime).Max();
            Logger.Info($"Latest .cs file is {latestCsFile}");

            if (earliestExeFile >= latestCsFile)
            {
                Logger.Info("Build agent is up to date, skipping");
                return;
            }
        }

        Logger.Info("Build agent is outdated, rebuilding");
        var oldCurrentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, ".bsmake");

        var process = Process.Start("dotnet", "build");
        await process.WaitForExitAsync();

        Environment.CurrentDirectory = oldCurrentDirectory;

    }

    /// <summary>
    /// Calls the build agent which is an executable
    /// </summary>
    private async Task TriggerBuildAgentAsExecutable()
    {
        // Find .exe in bin directory
        var exeFiles = Directory.GetFiles("./.bsmake/bin/", "*.exe", SearchOption.AllDirectories);
        switch (exeFiles.Length)
        {
            case 0: 
                throw new InvalidOperationException("No executable found in bin directory");
            case 1:
                break;
            default:
                throw new InvalidOperationException("Multiple executables found in bin directory");
        }
        
        Logger.Info($"Calling executable: {exeFiles.Single()}");
        var exeFile = exeFiles.Single();
        var process = Process.Start(exeFile);
        await process.WaitForExitAsync();
    }
}