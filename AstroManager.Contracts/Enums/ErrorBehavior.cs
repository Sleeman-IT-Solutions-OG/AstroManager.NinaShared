namespace Shared.Model.Enums;

/// <summary>
/// Defines behavior when an error occurs during imaging
/// </summary>
public enum ErrorBehavior
{
    /// <summary>
    /// End the sequence immediately on error
    /// </summary>
    StopSequence = 0,
    
    /// <summary>
    /// Wait for configured minutes, then retry the failed operation
    /// </summary>
    WaitAndRetry = 1,
    
    /// <summary>
    /// Mark the target as inactive and move to the next target
    /// </summary>
    SkipTarget = 2,
    
    /// <summary>
    /// Skip the target temporarily for configured minutes, then allow it to be scheduled again
    /// </summary>
    SkipTargetTemporarily = 3
}
