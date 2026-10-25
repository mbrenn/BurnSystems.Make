namespace BurnSystems.Make.BuildAgent
{
    /// <summary>
    /// Helper methods to support easy creation of Build.cs
    /// </summary>
    /// <param name="args"></param>
    public class BsMake(string[] args)
    {
        public ProgramArguments Arguments { get; }= ProgramArguments.Parse(args);

        /// <summary>
        /// Calls the async task in case the verb is fitting to the verb as provided
        /// in the arguments
        /// </summary>
        /// <param name="verb">Verb to be checked</param>
        /// <param name="asyncTask">The task to be executed in case the verb is fitting</param>
        /// <returns>The async task</returns>
        public async Task Verb(string verb, Func<Task> asyncTask)
        {
            if (verb.ToLowerInvariant() == Arguments.Verb.ToLowerInvariant())
            {
                await asyncTask();
            }
        }
    }
}
