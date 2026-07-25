namespace BSMake.Library;

/// <summary>
/// Defines the potential results of calling the build agent.
/// It just describes the positive events. Negative events are reported via an exception 
/// </summary>
public enum BuildAgentExecutionResult
{
    SuccessNoBuildAgentExisting,
    SuccessBuildAgentExecuted
}