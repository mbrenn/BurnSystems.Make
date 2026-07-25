using System.Diagnostics;
using BurnSystems.Logging;

namespace BSMake.Library;

/// <summary>
/// Contains the logic for the BSMake tool and is entry point
/// </summary>
public class Logic
{
    private CommandLineArguments CommandLineArguments { get; }

    public Logic(CommandLineArguments  commandLineArguments)
    {
        this.CommandLineArguments = commandLineArguments;
    }

    /// <summary>
    /// Starts the logger
    /// </summary>
    private static readonly ILogger logger = new ClassLogger(typeof(Logic));
    
    public async Task Execute()
    {
        if (string.IsNullOrEmpty(CommandLineArguments.Verb))
        {
            throw new InvalidOperationException("Verb is required");
        }
        
        switch (CommandLineArguments.Verb.ToLower())
        {
            case "build":
                await Build();
                return;
            case "clean":
                await Clean();
                return;
            default:
                throw new InvalidOperationException($"Invalid verb: {CommandLineArguments.Verb}");
        }
    }

    public async Task Build()
    {
        using var _ = new StopWatchLogger(logger, "BSMake Build");
            
        var solutionFile = CheckThatSolutionFileIsExisting();
        if (solutionFile != null)
        {
            logger.Info($"Building Solution: {solutionFile}");

            // Try to execute the .bsmake process
            var buildAgentLogic = new BuildAgentLogic(CommandLineArguments);
            await buildAgentLogic.ExecuteBuildAgent();

            // After that is done, execute the dotnet build command
            var buildProcess = Process.Start("dotnet", "build");
            await buildProcess.WaitForExitAsync();
            if (buildProcess.ExitCode == -1)
            {
                logger.Error("Building failed");
                throw new InvalidOperationException("Building failed");
            }
        }
        else
        {
            // We seem to be in project context. This means, that we are looking of a .bsmake directory
            // exists. In case, it does not exist, just give an information about that.
            // In case, it is existing, execute the build agent
            var buildAgentLogic = new BuildAgentLogic(CommandLineArguments);
            var informationAboutBuildAgent = buildAgentLogic.GetInformationOfBuildAgent();
            if (informationAboutBuildAgent.AgentType == BuildAgentType.NotExisting)
            {
                logger.Info("No .bsmake folder exists for project, skipping build");
            }
            else
            {
                // Try to execute the .bsmake process
                await buildAgentLogic.ExecuteBuildAgent();
            }
        }
    }

    public async Task Clean()
    {
        using var _ = new StopWatchLogger(logger, "BSMake clean");
        
        var solutionFile = CheckThatSolutionFileIsExisting();
        logger.Info($"Cleaning {solutionFile}");
        
        // Clean in project directory
        var buildAgentLogic = new BuildAgentLogic(CommandLineArguments);
        await buildAgentLogic.CleanBuildAgent();
        
        // Call the MSBuild clean routine
        var cleanProcess = Process.Start("dotnet", "clean");
        await cleanProcess.WaitForExitAsync();
        if (cleanProcess.ExitCode == -1)
        {
            logger.Error("Cleaning failed");
            throw new InvalidOperationException("Cleaning failed");
        }
        
        logger.Info("Cleaning finished");
    }

    /// <summary>
    /// Checks that the solution file in current directory is existing
    /// </summary>
    private string? CheckThatSolutionFileIsExisting()
    {
        return Directory.EnumerateFiles(".")
            .FirstOrDefault(x => x.EndsWith(".sln" )|| x.EndsWith(".slnx"));
    }
}