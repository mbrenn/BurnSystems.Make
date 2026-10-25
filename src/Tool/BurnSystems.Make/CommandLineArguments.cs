using BurnSystems.CommandLine.ByAttributes;

namespace BSMake.Library;

/// <summary>
/// Stores the parsed command line arguments
/// </summary>
public class CommandLineArguments
{
    /// <summary>
    /// Defines the verb to be executed
    /// </summary>
    [UnnamedArgument(IsRequired = true, HelpText = "Verb, may be Build or Clean")]
    public string Verb { get; set; } = "build";

    /// <summary>
    /// Defines the directory path in which bsmake shall be executed.
    /// </summary>
    [UnnamedArgument(IsRequired = false, HelpText = "The directory path in which bsmake shall be executed. If not set, the current directory will be used.")]
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Stores the configuration under which BSMake is called
    /// </summary>
    [NamedArgument(IsRequired = false, ShortName = 'c', LongName = "configuration")]
    public string? Configuration { get; set; }
}