using Shared.Model.DTO.DSO;
using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for scheduled targets
/// </summary>
public class ScheduledTargetDto
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(100, ErrorMessage = "Target name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Range(0, 24, ErrorMessage = "Right Ascension must be between 0 and 24 hours")]
    public double RightAscension { get; set; }
    
    [Range(-90, 90, ErrorMessage = "Declination must be between -90 and 90 degrees")]
    public double Declination { get; set; }
    
    [StringLength(50, ErrorMessage = "Object type cannot exceed 50 characters")]
    public string? ObjectType { get; set; }
    
    public double? MagnitudeV { get; set; }
    public double? Distance { get; set; }
    public double? SizeMinArcmin { get; set; }
    public double? SizeMaxArcmin { get; set; }
    public double? PA { get; set; }
    
    
    public bool IsMosaic { get; set; } = false;
    public int MosaicPanelsX { get; set; } = 1;
    public int MosaicPanelsY { get; set; } = 1;
    public bool MosaicUseRotator { get; set; } = false;
    
    [Range(0, 100, ErrorMessage = "Mosaic overlap must be between 0 and 100 percent")]
    public double MosaicOverlapPercent { get; set; } = 10.0;
    
    /// <summary>
    /// If true, each panel can have custom imaging goals.
    /// If false, all panels inherit goals from parent target (unified mode).
    /// </summary>
    public bool UseCustomPanelGoals { get; set; } = false;
    
    /// <summary>
    /// Panel shooting strategy: Sequential (complete one panel before next) or Parallel (rotate through panels)
    /// </summary>
    public MosaicShootingStrategy MosaicShootingStrategy { get; set; } = MosaicShootingStrategy.Parallel;
    
    /// <summary>
    /// Panel ordering method: Manual (user-defined order) or Auto (by observability)
    /// </summary>
    public MosaicPanelOrderingMethod MosaicPanelOrderingMethod { get; set; } = MosaicPanelOrderingMethod.Manual;
    
    /// <summary>
    /// Goal ordering method: Controls whether base goals or custom panel-specific goals are shot first
    /// Only applies when UseCustomPanelGoals is true
    /// </summary>
    public GoalOrderingMethod GoalOrderingMethod { get; set; } = GoalOrderingMethod.BaseGoalsFirst;
    
    // Scheduler Settings - template reference and overrides
    /// <summary>
    /// Optional reference to a SchedulerTargetTemplate for scheduler settings
    /// </summary>
    public Guid? SchedulerTargetTemplateId { get; set; }
    
    /// <summary>
    /// The linked SchedulerTargetTemplate (populated when retrieved)
    /// </summary>
    public SchedulerTargetTemplateDto? SchedulerTargetTemplate { get; set; }
    
    // Image-related properties
    public bool ShowImage { get; set; } = false;
    public string? AstroBinImageId { get; set; }
    public string? AstroBinImageUrl { get; set; }
    public string? AstroBinCachedImageDataBase64 { get; set; }
    public string? AstroBinCachedImageContentType { get; set; }
    public string? HipsImageDataBase64 { get; set; }
    public string? HipsImageContentType { get; set; }
    
    [Required]
    public List<ECameraFilter> RelevantFilters { get; set; } = new List<ECameraFilter>();
    
    public List<string> UserTags { get; set; } = new List<string>();
    
    [Range(1, 99, ErrorMessage = "Priority must be between 1 and 99")]
    public int Priority { get; set; } = 50;
    
    /// <summary>
    /// Number of times to repeat this target's imaging goals (multiplier)
    /// Default is 1 (no multiplication). Set to 2 for double goals, 3 for triple, etc.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Repeat count must be between 1 and 100")]
    public int RepeatCount { get; set; } = 1;
    
    public string? Notes { get; set; }
    
    [Required]
    public ScheduledTargetStatus Status { get; set; } = ScheduledTargetStatus.Active;

    /// <summary>
    /// If set, target is temporarily inactive until this UTC time.
    /// </summary>
    public DateTime? TemporaryInactiveUntilUtc { get; set; }
    
    /// <summary>
    /// REQUIRED: Observatory ID for this scheduled target
    /// Use SessionData.Observatories to get the full observatory details
    /// </summary>
    [Required]
    public Guid ObservatoryId { get; set; }
    
    /// <summary>
    /// REQUIRED: Equipment ID for this scheduled target
    /// Use SessionData.Equipments to get the full equipment details
    /// </summary>
    [Required]
    public Guid EquipmentId { get; set; }
    
    /// <summary>
    /// Optional reference to the original TargetListItem
    /// </summary>
    public Guid? SourceTargetListItemId { get; set; }
    
    /// <summary>
    /// Imaging goals for this target (parent goals for unified mode)
    /// </summary>
    public List<ImagingGoalDto> ImagingGoals { get; set; } = new List<ImagingGoalDto>();
    
    /// <summary>
    /// Mosaic panels (if IsMosaic = true)
    /// </summary>
    public List<ScheduledTargetPanelDto> Panels { get; set; } = new List<ScheduledTargetPanelDto>();
    
    /// <summary>
    /// Total number of panels
    /// </summary>
    public int TotalPanelCount => MosaicPanelsX * MosaicPanelsY;
    
    /// <summary>
    /// Whether this target has panels generated
    /// </summary>
    public bool HasPanels => Panels?.Any() == true;
    
    /// <summary>
    /// Total imaging goal time in minutes (sum of all filter goals)
    /// </summary>
    public double TotalGoalTimeMinutes
    {
        get
        {
            var repeatCount = Math.Max(1, RepeatCount);

            if (IsMosaic && HasPanels)
            {
                return Panels
                    .Where(p => p.IsEnabled)
                    .SelectMany(p => p.ImagingGoals)
                    .Where(g => g.IsEnabled)
                    .Sum(g => g.GoalTimeMinutes * repeatCount);
            }

            return ImagingGoals
                .Where(g => g.IsEnabled)
                .Sum(g => g.GoalTimeMinutes * repeatCount);
        }
    }
    
    /// <summary>
    /// Total completed time in minutes
    /// </summary>
    public double TotalCompletedTimeMinutes
    {
        get
        {
            if (IsMosaic && HasPanels)
            {
                return Panels
                    .Where(p => p.IsEnabled)
                    .SelectMany(p => p.ImagingGoals)
                    .Where(g => g.IsEnabled)
                    .Sum(g => g.CompletedTimeMinutes);
            }

            return ImagingGoals
                .Where(g => g.IsEnabled)
                .Sum(g => g.CompletedTimeMinutes);
        }
    }
    
    /// <summary>
    /// Total scheduled time in minutes
    /// </summary>
    public double TotalScheduledTimeMinutes
    {
        get
        {
            if (IsMosaic && HasPanels)
            {
                return Panels
                    .Where(p => p.IsEnabled)
                    .SelectMany(p => p.ImagingGoals)
                    .Where(g => g.IsEnabled)
                    .Sum(g => g.ScheduledTimeMinutes);
            }

            return ImagingGoals
                .Where(g => g.IsEnabled)
                .Sum(g => g.ScheduledTimeMinutes);
        }
    }
    
    /// <summary>
    /// Overall completion percentage
    /// </summary>
    public double CompletionPercentage => TotalGoalTimeMinutes > 0 
        ? Math.Round((TotalCompletedTimeMinutes / TotalGoalTimeMinutes) * 100, 1) 
        : 0;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Optional: Captured images summary for this target (populated when includeCapturedImagesSummary=true)
    /// </summary>
    public CapturedImageSummaryDto? CapturedImagesSummary { get; set; }
}

/// <summary>
/// Slim DTO for temporarily skipped targets used by RC/session status cards.
/// </summary>
public class ScheduledTargetTemporarySkipDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? TemporaryInactiveUntilUtc { get; set; }
}
