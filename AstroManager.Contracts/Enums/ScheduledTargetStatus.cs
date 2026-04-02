namespace Shared.Model.Enums;

/// <summary>
/// Status enumeration for scheduled targets
/// </summary>
public enum ScheduledTargetStatus
{
    /// <summary>
    /// Target is active and available for scheduling
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// Target is temporarily paused
    /// </summary>
    Paused = 1,
    
    /// <summary>
    /// Target has been completed
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Target has been archived
    /// </summary>
    Archived = 3
}
