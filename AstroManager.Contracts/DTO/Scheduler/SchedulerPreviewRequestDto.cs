using Shared.Model.Enums;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Request DTO for generating a scheduler preview for a single night
/// </summary>
public class SchedulerPreviewRequestDto
{
    /// <summary>
    /// Scheduler configuration ID to use (required)
    /// </summary>
    public Guid ConfigurationId { get; set; }
    
    /// <summary>
    /// Observatory ID for location-based calculations (required)
    /// </summary>
    public Guid ObservatoryId { get; set; }
    
    /// <summary>
    /// Equipment ID to filter targets (required)
    /// </summary>
    public Guid EquipmentId { get; set; }
    
    /// <summary>
    /// Date to preview (the night starting on this date)
    /// </summary>
    public DateTime PreviewDate { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Optional: Specific target IDs to include (null = all eligible targets)
    /// </summary>
    public List<Guid>? TargetIds { get; set; }
    
    /// <summary>
    /// Optional: Override settings from the configuration
    /// </summary>
    public SchedulerConfigOverrideDto? Overrides { get; set; }
    
    /// <summary>
    /// Include detailed explanations for why targets were/weren't scheduled
    /// </summary>
    public bool IncludeDetailedExplanations { get; set; } = false;
    
    /// <summary>
    /// Include altitude data points for graphing (increases response size)
    /// </summary>
    public bool IncludeAltitudeData { get; set; } = false;
    
    /// <summary>
    /// Interval in minutes for altitude data points (default: 15)
    /// </summary>
    public int AltitudeDataIntervalMinutes { get; set; } = 15;
}

/// <summary>
/// Request DTO for running the scheduler over a date range
/// </summary>
public class SchedulerRunRequestDto
{
    /// <summary>
    /// Scheduler configuration ID to use (required)
    /// </summary>
    public Guid ConfigurationId { get; set; }
    
    /// <summary>
    /// Observatory ID for location-based calculations (required)
    /// </summary>
    public Guid ObservatoryId { get; set; }
    
    /// <summary>
    /// Equipment ID to filter targets (required)
    /// </summary>
    public Guid EquipmentId { get; set; }
    
    /// <summary>
    /// Start date for scheduling
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End date for scheduling
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Optional: Specific target IDs to include (null = all eligible targets)
    /// </summary>
    public List<Guid>? TargetIds { get; set; }
    
    /// <summary>
    /// Whether to clear existing sessions in the date range before generating new ones
    /// </summary>
    public bool ClearExistingSessions { get; set; } = true;
    
    /// <summary>
    /// Optional: Override settings from the configuration
    /// </summary>
    public SchedulerConfigOverrideDto? Overrides { get; set; }
}

/// <summary>
/// Override settings that can be applied on top of a scheduler configuration
/// </summary>
public class SchedulerConfigOverrideDto
{
    public TargetSelectionStrategy? PrimaryStrategy { get; set; }
    public TargetSelectionStrategy? SecondaryStrategy { get; set; }
    public TargetSelectionStrategy? TertiaryStrategy { get; set; }
    public string? FilterShootingPattern { get; set; }
    public int? FilterBatchSize { get; set; }
    public double? MaxHoursPerTargetPerNight { get; set; }
    public double? MaxTotalHoursPerTarget { get; set; }
    public double? MinAltitudeDegrees { get; set; }
    public double? ImagingEfficiencyPercent { get; set; }
    public int? MinSessionDurationMinutes { get; set; }
    public int? MaxSequenceTimeMinutes { get; set; }
    public string? GoalCompletionBehavior { get; set; }
    public int? LowerPriorityTo { get; set; }
    public bool? UseMoonAvoidance { get; set; }
    
    /// <summary>
    /// Meridian flip settings from NINA (optional)
    /// </summary>
    public MeridianFlipSettingsDto? MeridianFlipSettings { get; set; }
}

/// <summary>
/// Meridian flip settings from NINA profile
/// </summary>
public class MeridianFlipSettingsDto
{
    /// <summary>
    /// Whether meridian flip handling is enabled
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Minutes after meridian before triggering flip (positive = wait after meridian)
    /// </summary>
    public double MinutesAfterMeridian { get; set; }
    
    /// <summary>
    /// Minutes to pause imaging before expected flip time
    /// </summary>
    public double PauseTimeBeforeFlipMinutes { get; set; }
    
    /// <summary>
    /// Maximum minutes to wait for meridian before allowing imaging to start
    /// If target is within this window of meridian, don't start imaging it
    /// </summary>
    public double MaxMinutesToMeridian { get; set; } = 5;
    
    /// <summary>
    /// Whether to use sidereal time for calculations (more accurate)
    /// </summary>
    public bool UseSiderealTime { get; set; } = true;
}
