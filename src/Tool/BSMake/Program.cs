using BSMake.Library;
using BurnSystems.Logging;
using BurnSystems.Logging.Provider;

TheLog.AddProvider(new ConsoleProvider());

var commandLineArguments = BurnSystems.CommandLine.Parser.ParseIntoOrShowUsage<CommandLineArguments>(args);
if (commandLineArguments == null)
{
    return 0;
}

try
{
    var logic = new Logic(commandLineArguments);
    await logic.Execute();
    return 0;
}
catch (Exception exc)
{
    TheLog.Fatal($"An error occurred: {exc}");
    return -1;
}

