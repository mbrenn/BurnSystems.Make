using BurnSystems.Logging;
using BurnSystems.Logging.Provider;

namespace BSMake.Tests;

/// <summary>
/// Just some helper methods to support the testing
/// </summary>
public class Helper
{
    /// <summary>
    /// Stores the original working directory before moving to example directory
    /// </summary>
    private string? _storedDirectory;

    public void StartInExampleDirectory()
    {
        if (_storedDirectory != null)
        {
            throw new InvalidOperationException("storedDirectory is not null. Ensure MoveToExampleDirectory is not called twice before restoring.");
        }
        _storedDirectory = Environment.CurrentDirectory;

        while (!Environment.CurrentDirectory.EndsWith("BurnSystems Make"))
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Environment.CurrentDirectory) ?? Environment.CurrentDirectory;
        }
        
        Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "src/Example");
        
        // Initializes the Logger
        TheLog.ClearProviders(); 
        TheLog.AddProvider(new ConsoleProvider());
        TheLog.AddProvider(InMemoryDatabaseProvider.TheOne);
        InMemoryDatabaseProvider.TheOne.ClearLog();
    }

    public void RestoreWorkingDirectory()
    {
        if (_storedDirectory == null)
        {
            throw new InvalidOperationException("Stored directory is null. Ensure MoveToExampleDirectory is called before RestoreExampleDirectory.");
        }
        
        Environment.CurrentDirectory = _storedDirectory;
    }
}