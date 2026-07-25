using System.Runtime.InteropServices;
using BSMake.Library;
using BurnSystems.Logging.Provider;
using NUnit.Framework;

namespace BSMake.Tests;

[TestFixture]
public class SolutionTests
{
    private static readonly string BinaryPath =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Example.Executable/bin/Debug/net10.0/Example.Executable.exe"
            : "Example.Executable/bin/Debug/net10.0/Example.Executable";

    [Test]
    public async Task TestBuildOfSolution()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();
        
        // Ensure that solution file is existing
        Assert.True(File.Exists("BurnSystems Make Example.slnx"), "Solution file does not exist.");
        
        // Remove the ".\Example.Executable\bin\Debug\net10.0\Example.Executable.exe", if existing
        if (File.Exists(BinaryPath))
        {
            File.Delete(BinaryPath);
        }
        
        // Ok, we may start the solution building
        var commandLineArguments = new CommandLineArguments
        {
            Verb = "build"
        };
        
        await new Logic(commandLineArguments).Execute();
        
        // Check that a building has occured
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("Start: BSMake Build")), "No building occured.");
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("End  : BSMake Build")), "No finishing of building has occured.");
        
        Assert.That(File.Exists(BinaryPath));
        
        // Cleaning up
        helper.RestoreWorkingDirectory();
    }

    [Test]
    public async Task TestCallOfSolutionBuildAgent()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();

        var outputTestFile = "output.txt";
        if (File.Exists(outputTestFile))
        {
            File.Delete(outputTestFile);
        }
        
        // Ok, we may start the solution building
        var commandLineArguments = new CommandLineArguments
        {
            Verb = "build"
        };
        
        await new Logic(commandLineArguments).Execute();
    
        Assert.That(File.Exists(outputTestFile));
        File.Delete(outputTestFile);
    }
    
    [Test]
    public async Task TestCleanOfSolution()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();
        
        // Ensure that solution file is existing
        Assert.True(File.Exists("BurnSystems Make Example.slnx"), "Solution file does not exist.");
        
        // Ok, we may start the solution cleaning
        var commandLineArgumentsPrepare = new CommandLineArguments
        {
            Verb = "build"
        };
        
        var logicPrepare = new Logic(commandLineArgumentsPrepare);
        await logicPrepare.Execute();
        
        // File is existing ==> Successful build
        Assert.That(File.Exists(BinaryPath));
        
        // Ok, we may start the solution cleaning
        var commandLineArguments = new CommandLineArguments
        {
            Verb = "clean"
        };
        
        var logic = new Logic(commandLineArguments);
        await logic.Execute();
        
        // Check that a cleaning event has occured
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("Start: BSMake clean")), "No cleaning occured.");
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("Clean build agent in .bsmake directory")), "No cleaning of project directory occured.");
        Assert.True(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("End  : BSMake clean")), "No finishing of cleaning has occured.");
        
        // We need to check that the build directory for the .bsmake/solution is also removed  
        Assert.That(!File.Exists(BinaryPath));
        
        // Cleaning up
        helper.RestoreWorkingDirectory();
    }
}