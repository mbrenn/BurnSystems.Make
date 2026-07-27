using BurnSystems.Make.BuildAgent;
using NUnit.Framework;

namespace BSMake.Tests.BuildAgent;

[TestFixture]
public class ProcessInvokeTests
{
    /// <summary>
    /// Tests the execution without path
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestWithoutPath()
    {
        if (!EnvironmentHelper.IsWindows()) return;

        var dotnetStartUp = new ProcessInvokeParameter
        {
            Command = "npx",
            Arguments = ["-h"],
            SearchInPath = false
        };

        try
        {
            var exitCode = await ProcessInvoke.Run(dotnetStartUp);
            Assert.Fail("Should have thrown an exception");
            Assert.That(exitCode, Is.Not.EqualTo(0));
        }
        catch (Exception e)
        {   
            Assert.True(true, "Exception should have been thrown");
        }
    }
    /// <summary>
    /// Tests the execution without path
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task TestWithPath()
    {
        if (!EnvironmentHelper.IsWindows()) return;

        var dotnetStartUp = new ProcessInvokeParameter
        {
            Command = "npx",
            Arguments = ["-h"],
            SearchInPath = true
        };

        var exitCode = await ProcessInvoke.Run(dotnetStartUp);
        Assert.That(exitCode, Is.EqualTo(0));
    }
}