using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for scheduled sessions
/// </summary>
public class ScheduledSessionDto
{
    public Guid Id { get; set; }
    
    public Guid ScheduledTargetId { get; set; }
    
    /// <summary>
    /// Target name (for display)
    /// </summary>
    public string? TargetName { get; set; }
    
    /// <summary>
    /// Panel ID for mosaic targets (null for non-mosaic)
    /// </summary>
    public Guid? PanelId { get; set; }
    
    /// <summary>
    /// Panel number for mosaic targets (null for non-mosaic)
    /// </summary>
    public int? PanelNumber { get; set; }
    
    /// <summary>
    /// Panel center Right Ascension in hours (for mosaic panels, null otherwise)
    /// </summary>
    public double? PanelCenterRA { get; set; }
    
    /// <summary>
    /// Panel center Declination in degrees (for mosaic panels, null otherwise)
    /// </summary>
    public double? PanelCenterDec { get; set; }
    
    [Required]
    public DateTime SessionDate { get; set; }
    
    [Required]
    public DateTime StartTimeUtc { get; set; }
    
    [Required]
    public DateTime EndTimeUtc { get; set; }
    
    [Required]
    public ECameraFilter Filter { get; set; }
    
    /// <summary>
    /// Filter shooting method (Loop, Batch, Priority)
    /// </summary>
    public string? FilterShootMethod { get; set; }
    
    /// <summary>
    /// Batch size for batch shooting method (null = complete all exposures for filter)
    /// </summary>
    public int? BatchSize { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Planned duration must be positive")]
    public double PlannedDurationMinutes { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Actual duration must be positive")]
    public double ActualDurationMinutes { get; set; } = 0;
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Planned";
    
    public bool IsManualOverride { get; set; } = false;
    
    public string? Notes { get; set; }
    
    public int ScheduledPriority { get; set; }

    public double? PriorityScore { get; set; }
    public List<SchedulerScoreContributionDto>? ScoreBreakdown { get; set; }
    public string? SelectionReason { get; set; }
    
    public double? AverageAltitude { get; set; }
    public double? MoonIllumination { get; set; }
    public double? MoonDistance { get; set; }
    
    /// <summary>
    /// Required minimum moon distance for this filter (from moon avoidance profile)
    /// </summary>
    public double? RequiredMoonDistance { get; set; }
    
    /// <summary>
    /// Filter segments with planned exposures for this session.
    /// Format: "L:10,R:10,Ha:8" means 10 L exposures, 10 R exposures, 8 Ha exposures.
    /// Used for batch shooting where filter switches happen within a session.
    /// </summary>
    public string? FilterSegments { get; set; }
    
    /// <summary>
    /// Total planned exposures across all filters in this session.
    /// </summary>
    public int PlannedExposures { get; set; }
    
    /// <summary>
    /// Display string for filters - shows FilterSegments if available, otherwise single Filter.
    /// Example: "L:10,R:10,Ha:8" or just "L"
    /// </summary>
    public string FilterDisplay => !string.IsNullOrEmpty(FilterSegments) ? FilterSegments : Filter.ToString();
    
    /// <summary>
    /// Duration in minutes (calculated from start/end times)
    /// </summary>
    public double DurationMinutes => (EndTimeUtc - StartTimeUtc).TotalMinutes;
    
    /// <summary>
    /// Duration in hours (for display)
    /// </summary>
    public double DurationHours => Math.Round(DurationMinutes / 60.0, 2);
    
    /// <summary>
    /// Planned duration in hours (for display)
    /// </summary>
    public double PlannedDurationHours => Math.Round(PlannedDurationMinutes / 60.0, 2);
    
    /// <summary>
    /// Actual duration in hours (for display)
    /// </summary>
    public double ActualDurationHours => Math.Round(ActualDurationMinutes / 60.0, 2);
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
