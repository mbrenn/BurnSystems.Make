using BSMake.Library;
using NUnit.Framework;

namespace BSMake.Tests;

[TestFixture]
public class ProjectTests
{
    private const string PathPrimes = "Example.Library/Primes.cs";

    private const string PathTestFileDebug = "Example.Library/Test_Debug.txt";

    private const string PathTestFileRelease = "Example.Library/Test_Release.txt";

    [Test]
    public async Task TestCallingOfProject()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();
        
        // Delete the 'Primes.cs' which has to be recreated by the build
        DeleteTestFiles();

        // Call the build agents.
        // Ok, we may start the solution building
        var commandLineArgumentsPrepare = new CommandLineArguments
        {
            Verb = "build"
        };
        
        var logicPrepare = new Logic(commandLineArgumentsPrepare);
        await logicPrepare.Execute();
        
        // Check, if the 'Primes.cs' has been created by the build
        Assert.IsTrue(File.Exists(PathPrimes));
        
        // Check that the debug run is started by default
        Assert.That((await File.ReadAllTextAsync(PathPrimes)).Contains("19]"));
        Assert.That(!File.Exists(PathTestFileRelease));

        // Cleaning up
        DeleteTestFiles();
        helper.RestoreWorkingDirectory();
    }

    [Test]
    public async Task TestCallingOfProjectForRelease()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();

        // Delete the 'Primes.cs' which has to be recreated by the build
        var pathPrimes = "Example.Library/Primes.cs";
        
        DeleteTestFiles();

        // Call the build agents.
        // Ok, we may start the solution building
        var commandLineArgumentsPrepare = new CommandLineArguments
        {
            Verb = "build",
            Configuration = "Release"
        };

        var logicPrepare = new Logic(commandLineArgumentsPrepare);
        await logicPrepare.Execute();

        // Check, if the 'Primes.cs' has been created by the build
        Assert.IsTrue(File.Exists(pathPrimes));

        // Check that the debug run is started by default
        Assert.That(File.Exists(PathTestFileRelease));
        
        // Cleaning up
        DeleteTestFiles();
        helper.RestoreWorkingDirectory();
    }

    private static void DeleteTestFiles()
    {
        File.Delete(PathPrimes);
        File.Delete(PathTestFileDebug);
        File.Delete(PathTestFileRelease);
    }
}