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
        CommandLineArguments = commandLineArguments;
    }

    /// <summary>
    /// Starts the logger
    /// </summary>
    private static readonly ILogger logger = new ClassLogger(typeof(Logic));
    
    public async Task Execute()
    {
        var currentDirectory = Environment.CurrentDirectory;
        try
        {
            if (!string.IsNullOrEmpty(CommandLineArguments.DirectoryPath))
            {
                Environment.CurrentDirectory = CommandLineArguments.DirectoryPath;
            }

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
        finally
        {
            Environment.CurrentDirectory = currentDirectory;
        }
    }

    public async Task Build()
    {
        // Before starting the build process, check if we have something to do.
        // Otherwise, the logging is too polluted
        var buildAgentLogic = new BuildAgentLogic(CommandLineArguments);
        var buildAgentInformation = buildAgentLogic.GetInformationOfBuildAgent();
        if(buildAgentInformation.AgentType == BuildAgentType.NotExisting)
        {
            logger.Info($"No Build Agent defined in '{buildAgentLogic.AbsoluteBuildAgentPath}'");
        }
            
        var solutionFile = CheckThatSolutionFileIsExisting();
        if (solutionFile == null && buildAgentInformation.AgentType == BuildAgentType.NotExisting)
        {
            // Short circuit to get rid of all the messages
            return;
        }
        
        using var _ = new StopWatchLogger(logger, "BSMake Build");
        
        if (solutionFile != null)
        {
            logger.Info($"Building Solution: {solutionFile}");

            // Try to execute the .bsmake process
            await buildAgentLogic.ExecuteBuildAgent();

            // After that is done, execute the dotnet build command

            // Creates the arguments for the call
            var processArguments = new List<string> { "build" };

            if (!string.IsNullOrEmpty(CommandLineArguments.Configuration))
            {
                processArguments.Add("-c");
                processArguments.Add(CommandLineArguments.Configuration);
            }

            var buildProcess = Process.Start("dotnet", processArguments);
            await buildProcess.WaitForExitAsync();
            if (buildProcess.ExitCode != 0)
            {
                logger.Error($"Building failed: Exit Code = {buildProcess.ExitCode}");
                throw new InvalidOperationException("Building failed");
            }
        }
        else
        {
            // We seem to be in project context. This means, that we are looking of a .bsmake directory
            // exists. In case, it does not exist, just give an information about that.
            // In case, it is existing, execute the build agent
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
        var buildAgentLogic = new BuildAgentLogic(CommandLineArguments);

        var buildAgentInformation = buildAgentLogic.GetInformationOfBuildAgent();
        if (buildAgentInformation.AgentType == BuildAgentType.NotExisting)
        {
            logger.Info("No .bsmake folder exists for project, skipping clean");
        }

        var solutionFile = CheckThatSolutionFileIsExisting();

        if (solutionFile == null && buildAgentInformation.AgentType == BuildAgentType.NotExisting)
        {
            // Short circuit to get rid of all the messages
            return;
        }
        
        using var _ = new StopWatchLogger(logger, "BSMake clean");

        if (solutionFile != null)
        {
            logger.Info($"Cleaning {solutionFile}");

            // Clean in project directory
            await buildAgentLogic.ExecuteCleanAgent();

            // Call the MSBuild clean routine
            var processArguments = new List<string> { "clean" };

            if (!string.IsNullOrEmpty(CommandLineArguments.Configuration))
            {
                processArguments.Add("-c");
                processArguments.Add(CommandLineArguments.Configuration);
            }

            var cleanProcess = Process.Start("dotnet", processArguments);
            await cleanProcess.WaitForExitAsync();
            if (cleanProcess.ExitCode != 0)
            {
                logger.Error($"Cleaning failed: Exit Code = {cleanProcess.ExitCode}");
                throw new InvalidOperationException("Cleaning failed");
            }

            logger.Info("Cleaning finished");
        }
        else
        {

            // We seem to be in project context. This means, that we are looking of a .bsmake directory
            // exists. In case, it does not exist, just give an information about that.
            // In case, it is existing, execute the build agent

            // Try to execute the .bsmake process
            await buildAgentLogic.ExecuteBuildAgent();
            await buildAgentLogic.ExecuteCleanAgent();
        }
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