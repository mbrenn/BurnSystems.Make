using BSMake.Library;
using BurnSystems.Logging.Provider;
using NUnit.Framework;

namespace BSMake.Tests;

[TestFixture]
public class SolutionTests
{
    private const string BinaryPath = ".\\Example.Executable\\bin\\Debug\\net10.0\\Example.Executable.exe";

    [Test]
    public async Task TestBuildOfSolution()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();
        
        // Ensure that solution file is existing
        Assert.True(File.Exists("BurnSystems Make Example.slnx"), "Solution file does not exist.");
        
        // Ok, we may start the solution building
        var commandLineArguments = new CommandLineArguments
        {
            Verb = "build"
        };
        
        // Remove the ".\Example.Executable\bin\Debug\net10.0\Example.Executable.exe", if existing
        if (File.Exists(BinaryPath))
        {
            File.Delete(BinaryPath);
        }
        
        var logic = new Logic(commandLineArguments);
        await logic.Execute();
        // Check that a building has occured
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("Building started")), "No building occured.");
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("Building finished")), "No finishing of building has occured.");
        
        Assert.That(File.Exists(BinaryPath));
        
        // Cleaning up
        helper.RestoreWorkingDirectory();
    }
    
    [Test]
    public async Task TestCleanOfSolution()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();
        
        // Ensure that solution file is existing
        Assert.True(File.Exists("BurnSystems Make Example.slnx"), "Solution file does not exist.");
        
        // Ok, we may start the solution cleaning
        var commandLineArguments = new CommandLineArguments
        {
            Verb = "clean"
        };

        if (!File.Exists(BinaryPath))
        {
            await File.WriteAllTextAsync(BinaryPath, "I am a binary file");
        }
        
        var logic = new Logic(commandLineArguments);
        await logic.Execute();
        
        // Check that a cleaning event has occured
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("Cleaning started")), "No cleaning occured.");
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("Cleaning finished")), "No finishing of cleaning has occured.");
        
        Assert.That(!File.Exists(BinaryPath));
        
        // Cleaning up
        helper.RestoreWorkingDirectory();
    }
}