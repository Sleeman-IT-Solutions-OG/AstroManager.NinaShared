using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for scheduler configuration
/// </summary>
public class SchedulerConfigurationDto
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public TargetSelectionStrategy PrimaryStrategy { get; set; } = TargetSelectionStrategy.PriorityFirst;
    
    public TargetSelectionStrategy? SecondaryStrategy { get; set; }
    
    public TargetSelectionStrategy? TertiaryStrategy { get; set; }
    
    public List<string> ObjectTypeFilter { get; set; } = new List<string>();
    
    public List<ECameraFilter> FilterPriority { get; set; } = new List<ECameraFilter>();
    
    [Required]
    [StringLength(50)]
    public string FilterShootingPattern { get; set; } = "Loop";
    
    [Range(1, 1000, ErrorMessage = "Batch size must be between 1 and 1000")]
    public int FilterBatchSize { get; set; } = 20;
    
    [Range(0, 24, ErrorMessage = "Max hours per night must be between 0 and 24")]
    public double MaxHoursPerTargetPerNight { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Max total hours must be positive")]
    public double MaxTotalHoursPerTarget { get; set; } = 0;
    
    [Required]
    [StringLength(50)]
    public string GoalCompletionBehavior { get; set; } = "Stop";
    
    [Range(1, 99, ErrorMessage = "Priority must be between 1 and 99")]
    public int LowerPriorityTo { get; set; } = 90;
    
    [Required]
    [Range(1, 100, ErrorMessage = "Efficiency must be between 1 and 100 percent")]
    public double ImagingEfficiencyPercent { get; set; } = 75.0;
    
    [Range(0, 90, ErrorMessage = "Minimum altitude must be between 0 and 90 degrees")]
    public double MinAltitudeDegrees { get; set; } = 30.0;
    
    public bool UseMoonAvoidance { get; set; } = true;
    
    [Range(1, 1440, ErrorMessage = "Minimum session duration must be between 1 and 1440 minutes")]
    public int MinSessionDurationMinutes { get; set; } = 30;
    
    [Range(1, 1440, ErrorMessage = "Maximum sequence time must be between 1 and 1440 minutes")]
    public int? MaxSequenceTimeMinutes { get; set; }
    
    // === No Target Behavior Settings ===
    
    /// <summary>
    /// What to do when no target is available for imaging
    /// </summary>
    public NoTargetBehavior NoTargetBehavior { get; set; } = NoTargetBehavior.WaitAndRetry;
    
    /// <summary>
    /// Minutes to wait before checking for targets again (when NoTargetBehavior is WaitAndRetry)
    /// </summary>
    [Range(1, 120, ErrorMessage = "Wait minutes must be between 1 and 120")]
    public int NoTargetWaitMinutes { get; set; } = 15;
    
    // === Error Handling Settings ===
    
    /// <summary>
    /// What to do when an error occurs during imaging
    /// </summary>
    public ErrorBehavior ErrorBehavior { get; set; } = ErrorBehavior.SkipTargetTemporarily;
    
    /// <summary>
    /// Minutes to wait before retrying (when ErrorBehavior is WaitAndRetry)
    /// </summary>
    [Range(1, 60, ErrorMessage = "Error wait minutes must be between 1 and 60")]
    public int ErrorWaitMinutes { get; set; } = 5;
    
    /// <summary>
    /// Minutes to skip target when using temporary skip (when ErrorBehavior is SkipTargetTemporarily)
    /// </summary>
    [Range(1, 480, ErrorMessage = "Skip duration must be between 1 and 480 minutes")]
    public int ErrorSkipDurationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Maximum retries before escalating to the configured ErrorBehavior
    /// </summary>
    [Range(1, 10, ErrorMessage = "Max retries must be between 1 and 10")]
    public int MaxErrorRetries { get; set; } = 3;

    // === Runtime Stop/Safety Checks (evaluated between slots in NINA scheduler) ===

    /// <summary>
    /// Always stop when scheduler determines no targets are observable for the remainder of the night.
    /// </summary>
    public bool AlwaysStopWhenNoTargetsForNight { get; set; } = true;

    /// <summary>
    /// Enable stop checks based on safety monitor state.
    /// </summary>
    public bool EnableSafetyMonitorCheck { get; set; } = true;

    /// <summary>
    /// Enable stop checks based on guider total RMS.
    /// </summary>
    public bool EnableGuidingRmsCheck { get; set; } = false;

    /// <summary>
    /// Maximum guider total RMS in arcseconds before violation is triggered.
    /// </summary>
    [Range(0.1, 20.0, ErrorMessage = "Max guiding RMS must be between 0.1 and 20 arcsec")]
    public double MaxGuidingRmsArcSec { get; set; } = 2.5;

    /// <summary>
    /// Enable weather cloud cover checks.
    /// </summary>
    public bool EnableCloudCoverCheck { get; set; } = false;

    /// <summary>
    /// Maximum cloud cover percentage before violation is triggered.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Max cloud cover must be between 0 and 100")]
    public double MaxCloudCoverPercent { get; set; } = 75;

    /// <summary>
    /// Enable weather rain rate checks.
    /// </summary>
    public bool EnableRainRateCheck { get; set; } = false;

    /// <summary>
    /// Maximum rain rate before violation is triggered.
    /// Units depend on the weather driver/device.
    /// </summary>
    [Range(0, 500, ErrorMessage = "Max rain rate must be between 0 and 500")]
    public double MaxRainRate { get; set; } = 0;

    /// <summary>
    /// Enable mount altitude guard checks.
    /// </summary>
    public bool EnableMountAltitudeCheck { get; set; } = true;

    /// <summary>
    /// Minimum allowed mount altitude. Targets below this won't be slewed to.
    /// </summary>
    [Range(0, 90, ErrorMessage = "Mount altitude limit must be between 0 and 90 degrees")]
    public double MinMountAltitudeDegrees { get; set; } = 20;

    /// <summary>
    /// Enable camera cooler power safety checks between exposures.
    /// </summary>
    public bool EnableCoolerPowerCheck { get; set; } = false;

    /// <summary>
    /// Cooler power percentage threshold that triggers corrective action.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Cooler power threshold must be between 0 and 100")]
    public double MaxCoolerPowerPercent { get; set; } = 95;

    /// <summary>
    /// If enabled, the scheduler will raise the target temperature setpoint when cooler power is too high.
    /// </summary>
    public bool ReduceCoolingOnHighCoolerPower { get; set; } = true;

    /// <summary>
    /// Degrees Celsius to warm the setpoint by when cooler power is above threshold.
    /// </summary>
    [Range(0.5, 20, ErrorMessage = "Cooler warmup delta must be between 0.5 and 20 degrees")]
    public double CoolerWarmupDeltaDegrees { get; set; } = 5;

    /// <summary>
    /// Action to take when a configured runtime check is violated.
    /// </summary>
    public SchedulerViolationAction ViolationAction { get; set; } = SchedulerViolationAction.StopScheduler;

    /// <summary>
    /// Retry delay in minutes when ViolationAction is StopTrackingAndRetry.
    /// </summary>
    [Range(1, 120, ErrorMessage = "Violation retry wait minutes must be between 1 and 120")]
    public int ViolationRetryMinutes { get; set; } = 10;
    
    public bool IsDefault { get; set; } = false;
    
    // NOTE: Meridian flip settings are in ClientConfiguration, not here.
    // They come from NINA's profile and are client-specific.
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// For optimistic locking - client sends last known UpdatedAt when updating
    /// Server rejects if record was modified since this timestamp
    /// </summary>
    public DateTime? LastKnownUpdatedAt { get; set; }
}
