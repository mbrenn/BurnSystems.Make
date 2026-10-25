using BurnSystems.Logging;

namespace BurnSystems.Make.BuildAgent
{
    /// <summary>
    /// Contains some support methods for the Npm
    /// </summary>
    public static class NpmHelper
    {
        /// <summary>
        /// Stores the logger
        /// </summary>
        private static readonly ILogger Logger = new ClassLogger(typeof(NpmHelper));

        /// <summary>
        /// Checks if npm install has been performed in the past by checking if a node_module directory
        /// exists. It does not check whether the node_module is up-to-date. This is implemented by the
        /// call 'NpmInstall'.
        /// </summary>
        /// <returns>true, if an installation had occured, false if assumed to be already installed</returns>
        public static async Task<bool> InstallIfNotInstalled()
        {
            if (!Directory.Exists("node_modules"))
            {
                await NpmInstall();
                Logger.Info("'Npm Install' got executed");
                return true;
            }

            Logger.Info("'Npm Install' got skipped");
            return false;
        }

        /// <summary>
        /// Just calls NpmInstall
        /// </summary>
        /// <returns>The task for async operation</returns>
        public static async Task NpmInstall()
        {
            await ProcessInvoke.Run("npm", ["install"]);
        }

        /// <summary>
        /// Executes npx with the given arguments
        /// </summary>
        /// <param name="arguments">Arguments to be given</param>
        /// <returns>The task for async operation</returns>
       public static async Task Execute(string[] arguments)
        {
            await ProcessInvoke.Run("npx", arguments);
        }

        /// <summary>
        /// Deletes the node modules path
        /// </summary>
        public static Task DeleteNodeModules(string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "node_modules";
            }

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

            return Task.CompletedTask;
        }
    }
}
