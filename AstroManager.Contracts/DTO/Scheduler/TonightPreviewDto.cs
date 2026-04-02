namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// DTO for tonight's imaging preview - shows which targets are best to image tonight
/// </summary>
public class TonightPreviewDto
{
    /// <summary>
    /// Date of the preview (tonight's date)
    /// </summary>
    public DateTime PreviewDate { get; set; }
    
    /// <summary>
    /// Observatory used for calculations
    /// </summary>
    public Guid ObservatoryId { get; set; }
    public string? ObservatoryName { get; set; }
    
    /// <summary>
    /// Equipment filter
    /// </summary>
    public Guid EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    
    /// <summary>
    /// Astronomical twilight start (imaging start time)
    /// </summary>
    public DateTime? AstronomicalDuskUtc { get; set; }
    
    /// <summary>
    /// Astronomical twilight end (imaging end time)
    /// </summary>
    public DateTime? AstronomicalDawnUtc { get; set; }
    
    /// <summary>
    /// Total available imaging hours tonight
    /// </summary>
    public double AvailableHours { get; set; }
    
    /// <summary>
    /// Moon illumination percentage
    /// </summary>
    public double MoonIlluminationPercent { get; set; }
    
    /// <summary>
    /// Targets sorted by recommended imaging order
    /// </summary>
    public List<TonightTargetPreviewDto> Targets { get; set; } = new();
    
    /// <summary>
    /// Summary statistics
    /// </summary>
    public TonightPreviewSummary Summary { get; set; } = new();
}

/// <summary>
/// Preview info for a single target tonight
/// </summary>
public class TonightTargetPreviewDto
{
    public Guid TargetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ObjectType { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; } = "Active";
    
    /// <summary>
    /// Right Ascension in hours
    /// </summary>
    public double RightAscension { get; set; }
    
    /// <summary>
    /// Declination in degrees
    /// </summary>
    public double Declination { get; set; }
    
    /// <summary>
    /// Time when target rises above minimum altitude (UTC)
    /// </summary>
    public DateTime? RiseTimeUtc { get; set; }
    
    /// <summary>
    /// Time when target sets below minimum altitude (UTC)
    /// </summary>
    public DateTime? SetTimeUtc { get; set; }
    
    /// <summary>
    /// Time when target transits (highest altitude) (UTC)
    /// </summary>
    public DateTime? TransitTimeUtc { get; set; }
    
    /// <summary>
    /// Maximum altitude tonight in degrees
    /// </summary>
    public double MaxAltitudeDegrees { get; set; }
    
    /// <summary>
    /// Current altitude in degrees (if currently visible)
    /// </summary>
    public double? CurrentAltitudeDegrees { get; set; }
    
    /// <summary>
    /// Hours available for imaging tonight
    /// </summary>
    public double AvailableHoursTonight { get; set; }
    
    /// <summary>
    /// Recommended start time for imaging this target (UTC)
    /// </summary>
    public DateTime? RecommendedStartUtc { get; set; }
    
    /// <summary>
    /// Recommended end time for imaging this target (UTC)
    /// </summary>
    public DateTime? RecommendedEndUtc { get; set; }
    
    /// <summary>
    /// Angular distance from moon in degrees
    /// </summary>
    public double? MoonDistanceDegrees { get; set; }
    
    /// <summary>
    /// Whether moon avoidance is triggered for this target
    /// </summary>
    public bool MoonAvoidanceTriggered { get; set; }
    
    /// <summary>
    /// Reason if target cannot be imaged tonight
    /// </summary>
    public string? ExclusionReason { get; set; }
    
    /// <summary>
    /// Whether target is imageable tonight
    /// </summary>
    public bool IsImageableTonight => string.IsNullOrEmpty(ExclusionReason);
    
    /// <summary>
    /// Overall completion percentage
    /// </summary>
    public double CompletionPercent { get; set; }
    
    /// <summary>
    /// Remaining hours needed to complete goals
    /// </summary>
    public double RemainingHours { get; set; }
    
    /// <summary>
    /// Score for ranking (higher = better to image tonight)
    /// </summary>
    public double Score { get; set; }
    
    /// <summary>
    /// Imaging goals summary
    /// </summary>
    public List<TonightGoalPreviewDto> Goals { get; set; } = new();
}

/// <summary>
/// Brief goal info for preview
/// </summary>
public class TonightGoalPreviewDto
{
    public string Filter { get; set; } = string.Empty;
    public int GoalCount { get; set; }
    public int CompletedCount { get; set; }
    public int RemainingCount => GoalCount - CompletedCount;
    public double CompletionPercent => GoalCount > 0 ? Math.Round((CompletedCount * 100.0) / GoalCount, 1) : 0;
}

/// <summary>
/// Summary statistics for tonight's preview
/// </summary>
public class TonightPreviewSummary
{
    public int TotalTargets { get; set; }
    public int ImageableTargets { get; set; }
    public int ExcludedTargets { get; set; }
    public double TotalAvailableHours { get; set; }
    public double TotalRemainingHours { get; set; }
    public int HighPriorityCount { get; set; }
    public int MoonAffectedCount { get; set; }
}
