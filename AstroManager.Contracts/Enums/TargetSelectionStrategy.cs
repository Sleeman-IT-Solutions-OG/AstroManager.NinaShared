namespace Shared.Model.Enums;

/// <summary>
/// Strategy for selecting which target to schedule next
/// </summary>
public enum TargetSelectionStrategy
{
    /// <summary>
    /// Select targets with highest priority first
    /// </summary>
    PriorityFirst,
    
    /// <summary>
    /// Select targets at optimal altitude first
    /// </summary>
    AltitudeFirst,
    
    /// <summary>
    /// Select targets with shortest observability window first
    /// </summary>
    TimeFirst,
    
    /// <summary>
    /// Select targets with highest remaining time first
    /// </summary>
    HighestTimeFirst,
    
    /// <summary>
    /// Select targets with filters requiring largest moon avoidance distance first
    /// </summary>
    MoonAvoidanceFirst
}
