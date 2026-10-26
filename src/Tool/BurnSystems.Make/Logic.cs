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
        await ExecuteOperation(
            verb: "build",
            stopwatchLabel: "BSMake Build",
            solutionAction: "Building Solution",
            projectNotExistingMessage: "No .bsmake folder exists for project, skipping build",
            errorAction: "Building",
            executeAgent: async (buildAgentLogic) => await buildAgentLogic.ExecuteBuildAgent(),
            executeInProjectContext: async (buildAgentLogic) => await buildAgentLogic.ExecuteBuildAgent()
        );
    }

    public async Task Clean()
    {
        await ExecuteOperation(
            verb: "clean",
            stopwatchLabel: "BSMake clean",
            solutionAction: "Cleaning",
            projectNotExistingMessage: "No .bsmake folder exists for project, skipping clean",
            errorAction: "Cleaning",
            executeAgent: async (buildAgentLogic) => await buildAgentLogic.ExecuteCleanAgent(),
            executeInProjectContext: async (buildAgentLogic) =>
            {
                await buildAgentLogic.ExecuteBuildAgent();
                await buildAgentLogic.ExecuteCleanAgent();
            }
        );
    }

    /// <summary>
    /// Shared operation flow for Build and Clean verbs.
    /// Creates a <see cref="BuildAgentLogic"/>, checks for solution files, and dispatches
    /// to the appropriate agent actions depending on whether a solution or project context is detected.
    /// </summary>
    /// <param name="verb">The dotnet command verb (e.g., "build" or "clean").</param>
    /// <param name="stopwatchLabel">Label used for the <see cref="StopWatchLogger"/> timing output.</param>
    /// <param name="solutionAction">Human-readable action description logged when a solution file is found.</param>
    /// <param name="projectNotExistingMessage">Message logged when no build agent exists in project context.</param>
    /// <param name="errorAction">Action name used in error messages when dotnet command fails.</param>
    /// <param name="executeAgent">Delegate invoked when a solution file is present.</param>
    /// <param name="executeInProjectContext">Delegate invoked when no solution file is present but a build agent exists.</param>
    private async Task ExecuteOperation(
        string verb,
        string stopwatchLabel,
        string solutionAction,
        string projectNotExistingMessage,
        string errorAction,
        Func<BuildAgentLogic, Task> executeAgent,
        Func<BuildAgentLogic, Task> executeInProjectContext)
    {
        var buildAgentLogic = new BuildAgentLogic(CommandLineArguments);
        var buildAgentInformation = buildAgentLogic.GetInformationOfBuildAgent();

        if (buildAgentInformation.AgentType == BuildAgentType.NotExisting)
        {
            logger.Info($"No Build Agent defined in '{buildAgentLogic.AbsoluteBuildAgentPath}'");
        }

        var solutionFile = CheckThatSolutionFileIsExisting();

        if (solutionFile == null && buildAgentInformation.AgentType == BuildAgentType.NotExisting)
        {
            return;
        }

        using var _ = new StopWatchLogger(logger, stopwatchLabel);

        if (solutionFile != null)
        {
            logger.Info($"{solutionAction} {solutionFile}");
            await executeAgent(buildAgentLogic);
            await RunDotnetCommand(verb, errorAction);

            if (verb == "clean")
            {
                logger.Info("Cleaning finished");
            }
        }
        else
        {
            var informationAboutBuildAgent = buildAgentLogic.GetInformationOfBuildAgent();
            if (informationAboutBuildAgent.AgentType == BuildAgentType.NotExisting)
            {
                logger.Info(projectNotExistingMessage);
            }
            else
            {
                await executeInProjectContext(buildAgentLogic);
            }
        }
    }

    /// <summary>
    /// Executes a dotnet command with the specified verb and optional configuration.
    /// Logs an error and throws if the process exits with a non-zero exit code.
    /// </summary>
    /// <param name="verb">The dotnet command verb (e.g., "build" or "clean").</param>
    /// <param name="errorAction">Action name used in error and exception messages when the command fails.</param>
    private async Task RunDotnetCommand(string verb, string errorAction)
    {
        var processArguments = new List<string> { verb };

        if (!string.IsNullOrEmpty(CommandLineArguments.Configuration))
        {
            processArguments.Add("-c");
            processArguments.Add(CommandLineArguments.Configuration);
        }

        var process = Process.Start("dotnet", processArguments);
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            logger.Error($"{errorAction} failed: Exit Code = {process.ExitCode}");
            throw new InvalidOperationException($"{errorAction} failed");
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