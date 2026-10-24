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
    Console.WriteLine(commandLineArguments.Configuration ?? "No Configuration");
    await logic.Execute();
    return 0;
}
catch (Exception exc)
{
    Console.Error.WriteLine($"An error occurred: {exc}");
    Console.WriteLine($"An error occured: {exc.Message}");
    return -1;
}

