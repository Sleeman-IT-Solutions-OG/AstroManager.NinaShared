using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for a mosaic panel
/// </summary>
public class ScheduledTargetPanelDto
{
    public Guid Id { get; set; }
    
    public Guid ScheduledTargetId { get; set; }
    
    [Required]
    public int PanelNumber { get; set; }
    
    /// <summary>
    /// Shooting order priority (lower numbers shoot first, null for default order)
    /// Used when MosaicPanelOrderingMethod is "Manual"
    /// </summary>
    public int? ShootingOrder { get; set; }
    
    [Required]
    [StringLength(100)]
    public string PanelName { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 24)]
    public double RaHours { get; set; }
    
    [Required]
    [Range(-90, 90)]
    public double DecDegrees { get; set; }
    
    public List<PanelImagingGoalDto> ImagingGoals { get; set; } = new();
    
    public DateTime? LastScheduled { get; set; }
    
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Whether this panel is enabled for scheduling
    /// Disabled panels are skipped by the scheduler
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Total completion percentage across all filters
    /// </summary>
    public double CompletionPercentage { get; set; }
    
    /// <summary>
    /// Remaining time to complete all goals
    /// </summary>
    public double RemainingTimeMinutes { get; set; }
    
    /// <summary>
    /// Number of custom goals for this panel
    /// </summary>
    public int CustomGoalCount => ImagingGoals?.Count(g => g.IsCustomGoal) ?? 0;
    
    /// <summary>
    /// Whether this panel has any custom goals
    /// </summary>
    public bool HasCustomGoals => CustomGoalCount > 0;
    
    /// <summary>
    /// Number of captured images for this panel (populated when includeCapturedImagesSummary=true)
    /// </summary>
    public int CapturedImageCount { get; set; }
}
