namespace Shared.Model.Enums;

/// <summary>
/// Defines behavior when no target is available for imaging
/// </summary>
public enum NoTargetBehavior
{
    /// <summary>
    /// End the sequence immediately when no targets are available
    /// </summary>
    StopSequence = 0,
    
    /// <summary>
    /// Wait for configured minutes, then check again for available targets
    /// </summary>
    WaitAndRetry = 1,
    
    /// <summary>
    /// Continue imaging already-completed targets to gather more data
    /// </summary>
    ShootCompletedTargets = 2
}
