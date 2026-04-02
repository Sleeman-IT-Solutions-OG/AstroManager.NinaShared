namespace Shared.Model.Enums;

/// <summary>
/// Type of slot returned by the scheduler
/// </summary>
public enum SlotType
{
    /// <summary>
    /// Take an exposure with the provided settings
    /// </summary>
    Exposure = 0,
    
    /// <summary>
    /// Wait for specified time before requesting next slot
    /// </summary>
    Wait = 1,
    
    /// <summary>
    /// End the sequence (no more targets, user preference, etc.)
    /// </summary>
    Stop = 2,
    
    /// <summary>
    /// Park the telescope (end of night, safety event, etc.)
    /// </summary>
    Park = 3
}
