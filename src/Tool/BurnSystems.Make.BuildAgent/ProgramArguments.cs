namespace BurnSystems.Make.BuildAgent
{
    public class ProgramArguments
    {
        /// <summary>
        /// Gets the configuration of the call
        /// </summary>
        public string Configuration { get; internal set; } = string.Empty;

        public string Verb { get; internal set; } = string.Empty;

        public static ProgramArguments Parse(string[] arguments)
        {
            var result = new ProgramArguments();
            var foundConfiguration = false;

            foreach (var argument in arguments)
            {
                if (foundConfiguration)
                {
                    result.Configuration = argument;
                    continue;
                }

                if (argument == "-c")
                {
                    foundConfiguration = true;
                    continue;
                }

                if (string.IsNullOrEmpty(result.Verb))
                {
                    result.Verb = argument;
                }
            }

            return result;
        }
    }
}
