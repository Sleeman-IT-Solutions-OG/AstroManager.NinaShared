namespace Shared.Model.Enums;

/// <summary>
/// Instruction returned to client after reporting an error
/// </summary>
public enum ErrorInstruction
{
    /// <summary>
    /// Retry the failed operation
    /// </summary>
    Retry = 0,
    
    /// <summary>
    /// Wait for specified minutes, then retry
    /// </summary>
    Wait = 1,
    
    /// <summary>
    /// Skip the current target and move to next
    /// </summary>
    SkipTarget = 2,
    
    /// <summary>
    /// End the sequence
    /// </summary>
    Stop = 3
}
