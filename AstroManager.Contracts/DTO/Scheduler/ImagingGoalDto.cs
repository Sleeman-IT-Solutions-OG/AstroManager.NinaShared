using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for imaging goals.
/// All exposure settings come from the linked ExposureTemplate - they are read-only here for display.
/// </summary>
public class ImagingGoalDto
{
    public Guid Id { get; set; }
    
    public Guid ScheduledTargetId { get; set; }
    
    /// <summary>
    /// Reference to the exposure template (REQUIRED)
    /// </summary>
    [Required]
    public Guid ExposureTemplateId { get; set; }
    
    /// <summary>
    /// The linked ExposureTemplate (populated when retrieved from API)
    /// </summary>
    public ExposureTemplateDto? ExposureTemplate { get; set; }
    
    /// <summary>
    /// Exposure template name (for display, populated from ExposureTemplate)
    /// </summary>
    public string? ExposureTemplateName => ExposureTemplate?.Name;
    
    /// <summary>
    /// Filter (read-only, from ExposureTemplate)
    /// </summary>
    public ECameraFilter Filter => ExposureTemplate?.Filter ?? ECameraFilter.L;

    /// <summary>
    /// Canonical filter name for runtime, UI, and custom-filter-aware scheduling.
    /// </summary>
    public string FilterName => ExposureTemplate?.EffectiveFilterName ?? Filter.ToString();

    /// <summary>
    /// Optional standard-filter mapping for the template filter.
    /// </summary>
    public ECameraFilter? StandardFilter => ExposureTemplate?.StandardFilter ?? ExposureTemplate?.Filter;
    
    /// <summary>
    /// Filter priority (read-only, from ExposureTemplate.DefaultFilterPriority)
    /// </summary>
    public int FilterPriority => ExposureTemplate?.DefaultFilterPriority ?? 50;
    
    /// <summary>
    /// Exposure time in seconds (read-only, from ExposureTemplate)
    /// </summary>
    public int ExposureTimeSeconds => ExposureTemplate?.ExposureTimeSeconds ?? 300;
    
    /// <summary>
    /// Camera binning (read-only, from ExposureTemplate)
    /// </summary>
    public int Binning => ExposureTemplate?.Binning ?? 1;
    
    /// <summary>
    /// Camera gain (read-only, from ExposureTemplate, -1 = camera default)
    /// </summary>
    public int Gain => ExposureTemplate?.Gain ?? -1;
    
    /// <summary>
    /// Camera offset (read-only, from ExposureTemplate, -1 = camera default)
    /// </summary>
    public int Offset => ExposureTemplate?.Offset ?? -1;
    
    /// <summary>
    /// Dither every X exposures (read-only, from ExposureTemplate, -1 = use scheduler default)
    /// </summary>
    public int DitherEveryX => ExposureTemplate?.DitherEveryX ?? -1;
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Goal count must be positive")]
    public int GoalExposureCount { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Completed exposures must be positive")]
    public int CompletedExposures { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Goal time must be positive")]
    public double GoalTimeMinutes => (GoalExposureCount * ExposureTimeSeconds) / 60.0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Completed time must be positive")]
    public double CompletedTimeMinutes => (CompletedExposures * ExposureTimeSeconds) / 60.0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Scheduled time must be positive")]
    public double ScheduledTimeMinutes { get; set; } = 0;
    
    /// <summary>
    /// Completion percentage (0-100+)
    /// </summary>
    public double CompletionPercentage => GoalExposureCount > 0 
        ? Math.Round((CompletedExposures / (double)GoalExposureCount) * 100, 1) 
        : 0;
    
    /// <summary>
    /// Whether this goal is completed (100% or more)
    /// </summary>
    public bool IsCompleted => CompletedExposures >= GoalExposureCount;
    
    /// <summary>
    /// Remaining time to reach goal in minutes
    /// </summary>
    public double RemainingTimeMinutes => Math.Max(0, GoalTimeMinutes - CompletedTimeMinutes);
    
    /// <summary>
    /// Goal time in hours (for display)
    /// </summary>
    public double GoalTimeHours => Math.Round(GoalTimeMinutes / 60.0, 2);
    
    /// <summary>
    /// Completed time in hours (for display)
    /// </summary>
    public double CompletedTimeHours => Math.Round(CompletedTimeMinutes / 60.0, 2);
    
    /// <summary>
    /// Scheduled time in hours (for display)
    /// </summary>
    public double ScheduledTimeHours => Math.Round(ScheduledTimeMinutes / 60.0, 2);
    
    /// <summary>
    /// Remaining time in hours (for display)
    /// </summary>
    public double RemainingTimeHours => Math.Round(RemainingTimeMinutes / 60.0, 2);
    
    /// <summary>
    /// Number of exposures needed to reach goal
    /// </summary>
    public int TotalExposuresNeeded => GoalExposureCount;
    
    /// <summary>
    /// Number of exposures remaining
    /// </summary>
    public int RemainingExposures => Math.Max(0, GoalExposureCount - CompletedExposures);
    
    /// <summary>
    /// Whether this imaging goal is enabled for scheduling
    /// Disabled goals are skipped by the scheduler
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
