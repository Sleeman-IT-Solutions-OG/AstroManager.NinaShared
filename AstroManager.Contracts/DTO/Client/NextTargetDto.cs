using Shared.Model.DTO.Scheduler;

namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for the next target to shoot (returned to telescope client)
/// Contains all information needed to generate a sequence file
/// </summary>
public class NextTargetDto
{
    /// <summary>
    /// Session ID
    /// </summary>
    public Guid SessionId { get; set; }
    
    /// <summary>
    /// Scheduled Target ID (for reporting progress back)
    /// </summary>
    public Guid ScheduledTargetId { get; set; }
    
    /// <summary>
    /// Target name (e.g., "M31 - Andromeda Galaxy")
    /// </summary>
    public string TargetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Right Ascension in hours (0-24)
    /// </summary>
    public double RightAscensionHours { get; set; }
    
    /// <summary>
    /// Declination in degrees (-90 to +90)
    /// </summary>
    public double DeclinationDegrees { get; set; }
    
    /// <summary>
    /// Position Angle (rotation) in degrees
    /// </summary>
    public double? PositionAngle { get; set; }
    
    /// <summary>
    /// Session start time (UTC)
    /// </summary>
    public DateTime StartTimeUtc { get; set; }
    
    /// <summary>
    /// Session end time (UTC)
    /// </summary>
    public DateTime EndTimeUtc { get; set; }
    
    /// <summary>
    /// Maximum imaging duration in minutes
    /// </summary>
    public double MaxDurationMinutes { get; set; }
    
    /// <summary>
    /// Imaging goals (filters, exposure times, counts)
    /// </summary>
    public List<ImagingGoalDto> ImagingGoals { get; set; } = new();
    
    /// <summary>
    /// Filter shooting method (Loop, Batch, etc.)
    /// </summary>
    public string FilterShootMethod { get; set; } = "Loop";
    
    /// <summary>
    /// Batch size for batch shooting method (number of exposures per filter before switching)
    /// Only used when FilterShootMethod is "Batch"
    /// </summary>
    public int? BatchSize { get; set; }
}
