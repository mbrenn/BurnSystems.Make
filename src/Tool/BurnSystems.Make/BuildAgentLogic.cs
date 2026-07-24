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
/// Stores the information about the created build agent. 
/// </summary>
public class BuildAgentInformation
{
    /// <summary>
    /// Defines the type of the build agent
    /// </summary>
    public BuildAgentType AgentType { get; init; }

    /// <summary>
    /// Stores the name of the output assembly.
    /// This information is used to call the right assembly
    /// </summary>
    public string OutputAssemblyName { get; init; } = string.Empty;
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
public class BuildAgentLogic(CommandLineArguments arguments)
{
    /// <summary>
    /// Directory where the build agent files are stored.
    /// </summary>
    private const string BuildAgentDir = ".bsmake";

    /// <summary>
    /// Directory where the build agent binaries are stored.
    /// </summary>
    private static string BuildAgentDirBinary => Path.Combine(BuildAgentDir, "bin"); 
    
    /// <summary>
    /// Stores the logger used to log messages
    /// </summary>
    private static readonly ILogger Logger = new ClassLogger(typeof(BuildAgentLogic));
    
    /// <summary>
    /// Stores the commandlinearguments being used to describe the current configuration
    /// </summary>
    private readonly CommandLineArguments _arguments = arguments;

    /// <summary>
    /// This is the main entry point which is called to start the full process from
    /// building the build agent down to executing it. 
    /// </summary>
    public async Task<BuildAgentExecutionResult> ExecuteBuildAgent()
    {
        var buildAgentInformation = GetInformationOfBuildAgent();
        Logger.Info($"Type of build agent is {buildAgentInformation}");
        
        switch (buildAgentInformation.AgentType)
        {
            case BuildAgentType.NotExisting:
                return BuildAgentExecutionResult.SuccessNoBuildAgentExisting;
            case BuildAgentType.Library:
                throw new InvalidOperationException("Library is currently not supported");
            case BuildAgentType.Executable:
                // We have determined that we have an executable build agent
                await BuildBuildAgent(buildAgentInformation);
                await TriggerBuildAgentAsExecutable(buildAgentInformation);
                return BuildAgentExecutionResult.SuccessBuildAgentExecuted;
            default:
                throw new InvalidOperationException($"Unknown build agent type {buildAgentInformation}");
        }
    }

    /// <summary>
    /// Cleans the build agent 
    /// </summary>
    /// <returns>The result, if the action was successful</returns>
    public async Task<BuildAgentExecutionResult> CleanBuildAgent()
    {
        var typeOfBuildAgent = GetInformationOfBuildAgent();
        Logger.Info($"Type of build agent is {typeOfBuildAgent}");

        switch (typeOfBuildAgent.AgentType)
        {
            case BuildAgentType.NotExisting:
                return BuildAgentExecutionResult.SuccessNoBuildAgentExisting;
            
            case BuildAgentType.Library:
                goto case BuildAgentType.Executable;
                
            case BuildAgentType.Executable:
                await CleanBuildAgentAsExecutable();
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
    public BuildAgentInformation GetInformationOfBuildAgent()
    {
        // Gets the .csproj file, if existing
        var projectPath = Path.Combine(Environment.CurrentDirectory, BuildAgentDir);

        var csProjs = Directory.EnumerateFiles(projectPath, "*.csproj").ToList();
        switch (csProjs.Count)
        {
            case 0:
                return new BuildAgentInformation
                {
                    AgentType = BuildAgentType.NotExisting
                };
            case 1:
                break;
            case 2:
                throw new InvalidOperationException($"Multiple .csproj files in {BuildAgentDir} directory");
        }

        var csProj = csProjs.Single();

        // Ok, we have the csproj-file, now determine the project type
        var document = XDocument.Load(csProj);
        var outputType =
            document.Root?
                .Elements("PropertyGroup")
                .Elements("OutputType")
                .FirstOrDefault();

        var outputNameNode =
            document.Root?
                .Elements("PropertyGroup")
                .Elements("TargetName")
                .FirstOrDefault();

        var outputName = outputNameNode?.Value
                         ?? Path.GetFileNameWithoutExtension(csProj);

        // Per default library
        if (outputType == null)
        {
            return new BuildAgentInformation
            {
                AgentType = BuildAgentType.NotExisting
            };
        }

        // Otherwise, evaluate
        return outputType.Value switch
        {
            "Library" => new BuildAgentInformation
            {
                AgentType = BuildAgentType.Library,
                OutputAssemblyName = outputName
            },
            "Exe" => new BuildAgentInformation
            {
                AgentType = BuildAgentType.Executable,
                OutputAssemblyName = outputName
            },
            _ => throw new InvalidOperationException($"Unknown output type: {outputType.Value}")
        };
    }

    private async Task BuildBuildAgent(BuildAgentInformation buildAgentInformation)
    {
        Logger.Info("Triggered building of agent");

        // Figures out age of the build agent itself
        if (Directory.Exists(BuildAgentDirBinary))
        {
            var exeFiles = Directory.GetFiles(BuildAgentDirBinary, buildAgentInformation.OutputAssemblyName, SearchOption.AllDirectories);
            var earliestExeFile = exeFiles.Length > 0 ? exeFiles.Select(File.GetLastWriteTime).Min() : DateTime.MinValue;
            Logger.Info($"Earliest .exe file is {earliestExeFile}");

            // Figures out latest age of all .cs files stored recursively
            var csFiles = Directory.GetFiles(BuildAgentDir, "*.cs", SearchOption.AllDirectories);
            var latestCsFile =  csFiles.Length > 0 ? csFiles.Select(File.GetLastWriteTime).Max() : DateTime.MaxValue;
            Logger.Info($"Latest .cs file is {latestCsFile}");

            if (earliestExeFile >= latestCsFile && earliestExeFile != DateTime.MinValue)
            {
                Logger.Info("Build agent is up to date, skipping");
                return;
            }
        }

        Logger.Info("Build agent is outdated, rebuilding");
        var oldCurrentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, BuildAgentDir);

        var process = Process.Start("dotnet", "build");
        await process.WaitForExitAsync();

        Environment.CurrentDirectory = oldCurrentDirectory;
    }

    /// <summary>
    /// Calls the build agent which is an executable
    /// </summary>
    private async Task TriggerBuildAgentAsExecutable(BuildAgentInformation buildAgentInformation)
    {
        // Find .exe in bin directory
        var exeFiles = 
            Directory.GetFiles(
                BuildAgentDirBinary,
                buildAgentInformation.OutputAssemblyName,
                SearchOption.AllDirectories);
        
        if (exeFiles.Length == 0)
        {
            exeFiles = 
                Directory.GetFiles(
                    BuildAgentDirBinary,
                    buildAgentInformation.OutputAssemblyName + ".exe",
                    SearchOption.AllDirectories);
        }
        
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

    /// <summary>
    /// Cleans the build agent by calling 'msbuild clean' directly in the build agent directory
    /// </summary>
    private async Task CleanBuildAgentAsExecutable()
    {        
        Logger.Info("Clean build agent in .bsmake directory");
        var oldCurrentDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, BuildAgentDir);

        var process = Process.Start("dotnet", "clean");
        await process.WaitForExitAsync();

        Environment.CurrentDirectory = oldCurrentDirectory;
    }
}