namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for reporting completed exposures from telescope client
/// </summary>
public class ExposureCompletedDto
{
    /// <summary>
    /// The scheduled target ID
    /// </summary>
    public Guid ScheduledTargetId { get; set; }
    
    /// <summary>
    /// The imaging goal ID that was completed
    /// </summary>
    public Guid ImagingGoalId { get; set; }
    
    /// <summary>
    /// Number of exposures completed (usually 1)
    /// </summary>
    public int ExposureCount { get; set; } = 1;
    
    /// <summary>
    /// Exposure time in seconds (for verification)
    /// </summary>
    public int ExposureTimeSeconds { get; set; }
}
