namespace BSMake.Library;

/// <summary>
/// Stores the information about the created build agent. 
/// </summary>
public class BuildAgentInformation
{
    /// <summary>
    /// Defines the type of the build agent
    /// </summary>
    public BuildAgentType AgentType { get; init; }

    /// <summary>
    /// Stores the name of the output assembly.
    /// This information is used to call the right assembly
    /// </summary>
    public string OutputAssemblyName { get; init; } = string.Empty;
    
    /// <summary>
    /// Just returns a non-existing BuildAgentInformation
    /// </summary>
    public static BuildAgentInformation NotExisting => new BuildAgentInformation
    {
        AgentType = BuildAgentType.NotExisting
    };
}