using BSMake.Library;
using NUnit.Framework;

namespace BSMake.Tests;

[TestFixture]
public class ProjectTests
{
    [Test]
    public async Task TestCallingOfProject()
    {
        var helper = new Helper();
        helper.StartInExampleDirectory();
        
        // Delete the 'Primes.cs' which has to be recreated by the build
        var pathPrimes = "Example.Library/Primes.cs";
        if (File.Exists(pathPrimes))
        {
            File.Delete(pathPrimes);
        }
        
        // Call the build agents.
        // Ok, we may start the solution cleaning
        var commandLineArgumentsPrepare = new CommandLineArguments
        {
            Verb = "build"
        };
        
        var logicPrepare = new Logic(commandLineArgumentsPrepare);
        await logicPrepare.Execute();
        
        // Check, if the 'Primes.cs' has been created by the build
        Assert.IsTrue(File.Exists(pathPrimes));
        
        // Cleaning up
        helper.RestoreWorkingDirectory();
    }
    
}