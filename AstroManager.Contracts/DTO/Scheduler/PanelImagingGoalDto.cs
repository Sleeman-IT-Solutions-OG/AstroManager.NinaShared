using Shared.Model.DTO.Settings;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for panel imaging goal - uses ExposureTemplate for exposure settings
/// </summary>
public class PanelImagingGoalDto
{
    public Guid Id { get; set; }
    
    public Guid ScheduledTargetPanelId { get; set; }
    
    /// <summary>
    /// Required: ExposureTemplate that defines filter, exposure time, binning, gain, offset
    /// </summary>
    [Required]
    public Guid ExposureTemplateId { get; set; }
    
    /// <summary>
    /// The linked ExposureTemplate (populated when retrieved from API)
    /// </summary>
    public ExposureTemplateDto? ExposureTemplate { get; set; }
    
    // Read-only properties from ExposureTemplate (for convenience/display)
    public ECameraFilter Filter => ExposureTemplate?.Filter ?? ECameraFilter.L;
    public int FilterPriority => ExposureTemplate?.DefaultFilterPriority ?? 50;
    public int ExposureTimeSeconds => ExposureTemplate?.ExposureTimeSeconds ?? 300;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Goal exposure count must be positive")]
    public int GoalExposureCount { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Completed exposures must be positive")]
    public int CompletedExposures { get; set; }
    
    // Calculated properties
    public double GoalTimeMinutes => (GoalExposureCount * ExposureTimeSeconds) / 60.0;
    public double CompletedTimeMinutes => (CompletedExposures * ExposureTimeSeconds) / 60.0;
    public double GoalTimeHours => GoalTimeMinutes / 60.0;
    public double CompletedTimeHours => CompletedTimeMinutes / 60.0;
    
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
    /// Whether this is a custom panel-specific goal (true) or a base goal synced from parent target (false)
    /// </summary>
    public bool IsCustomGoal { get; set; } = false;
    
    /// <summary>
    /// Whether this imaging goal is enabled for scheduling
    /// Disabled goals are skipped by the scheduler
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
