namespace BSMake.Library;

/// <summary>
/// Stores the parsed command line arguments
/// </summary>
public class CommandLineArguments
{
    /// <summary>
    /// Defines the verb to be executed
    /// </summary>
    [BurnSystems.CommandLine.ByAttributes.UnnamedArgument(IsRequired = true, HelpText = "Verb, may be Build or Clean")]
    public string? Verb { get; set; }
}