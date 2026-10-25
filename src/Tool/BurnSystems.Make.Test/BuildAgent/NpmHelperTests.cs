using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BurnSystems.Logging.Provider;
using BurnSystems.Make.BuildAgent;
using NUnit.Framework;
using BSMake.Library;

namespace BSMake.Tests.BuildAgent
{
    [TestFixture]
    public class NpmHelperTests
    {
        [Test]
        public async Task TestInstallIfNotInstalled()
        {
            var helper = new Helper();
            helper.StartInExampleDirectory();

            var libraryPath = Path.Combine(Environment.CurrentDirectory, "Example.Library");
            var originalDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = libraryPath;

            try
            {
                var nodeModulesPath = Path.Combine(libraryPath, "node_modules");
                DeleteNodeModules(nodeModulesPath);

                Assert.IsFalse(Directory.Exists(nodeModulesPath));

                // First call: Should install
                var installed = await NpmHelper.InstallIfNotInstalled();
                Assert.IsTrue(installed, "Should return true when installing for the first time");
                Assert.IsTrue(Directory.Exists(nodeModulesPath), "node_modules should exist after installation");

                // Second call: Should skip
                installed = await NpmHelper.InstallIfNotInstalled();
                Assert.IsFalse(installed, "Should return false when already installed");
                
                // Verify logs
                Assert.IsTrue(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("'Npm Install' got executed")));
                Assert.IsTrue(InMemoryDatabaseProvider.TheOne.Messages.Any(x => x.LogMessage.Message.Contains("'Npm Install' got skipped")));
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
                helper.RestoreWorkingDirectory();
            }
        }

        [Test]
        public async Task TestNpmInstall()
        {
            var helper = new Helper();
            helper.StartInExampleDirectory();

            var libraryPath = Path.Combine(Environment.CurrentDirectory, "Example.Library");
            var originalDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = libraryPath;

            try
            {
                var nodeModulesPath = Path.Combine(libraryPath, "node_modules");
                DeleteNodeModules(nodeModulesPath);

                Assert.IsFalse(Directory.Exists(nodeModulesPath));

                await NpmHelper.NpmInstall();
                Assert.IsTrue(Directory.Exists(nodeModulesPath), "node_modules should exist after NpmInstall");
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
                helper.RestoreWorkingDirectory();
            }
        }
        
        [Test]
        public async Task TestExecute()
        {
            // Just test if npx can be executed without error
            // We use --version as it's a safe command
            Assert.DoesNotThrowAsync(async () => await NpmHelper.Execute(new[] { "--version" }));
        }

        [Test]
        public async Task TestNpmIntegrationViaBSMake()
        {
            var helper = new Helper();
            helper.StartInExampleDirectory();

            var libraryPath = Path.Combine(Environment.CurrentDirectory, "Example.Library");
            var originalDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = libraryPath;

            try
            {
                var nodeModulesPath = Path.Combine(libraryPath, "node_modules");
                DeleteNodeModules(nodeModulesPath);

                Assert.IsFalse(Directory.Exists(nodeModulesPath), "node_modules should not exist before test");

                // Call bsmake build logic
                var commandLineArguments = new CommandLineArguments
                {
                    Verb = "build"
                };
                
                var logic = new Logic(commandLineArguments);
                await logic.Execute();

                Assert.IsTrue(Directory.Exists(nodeModulesPath), "node_modules should have been created by bsmake build via build agent");
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
                helper.RestoreWorkingDirectory();
            }
        }

        private static void DeleteNodeModules(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (IOException)
                {
                    // Sometimes node_modules is locked, try to rename it instead
                    var tempPath = path + "_" + Guid.NewGuid();
                    Directory.Move(path, tempPath);
                }
            }
        }
    }
}
