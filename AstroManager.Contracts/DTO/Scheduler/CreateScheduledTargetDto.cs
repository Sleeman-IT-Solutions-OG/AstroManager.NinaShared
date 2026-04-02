using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Request DTO for creating a scheduled target (optionally from a target list item)
/// </summary>
public class CreateScheduledTargetDto
{
    /// <summary>
    /// Optional: Source TargetListItem ID to create from (if not provided, manual entry required)
    /// </summary>
    public Guid? SourceTargetListItemId { get; set; }
    
    /// <summary>
    /// Target name (required if no source item)
    /// </summary>
    [StringLength(100)]
    public string? Name { get; set; }
    
    /// <summary>
    /// Object type (required if no source item)
    /// </summary>
    [StringLength(50)]
    public string? ObjectType { get; set; }
    
    /// <summary>
    /// Right Ascension in hours (required if no source item)
    /// </summary>
    [Range(0, 24)]
    public double? RightAscension { get; set; }
    
    /// <summary>
    /// Declination in degrees (required if no source item)
    /// </summary>
    [Range(-90, 90)]
    public double? Declination { get; set; }
    
    /// <summary>
    /// Position Angle in degrees (required if no source item)
    /// </summary>
    [Range(0, 360)]
    public double? PositionAngle { get; set; }
    
    /// <summary>
    /// Target status
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ScheduledTargetStatus>))]
    public ScheduledTargetStatus Status { get; set; } = ScheduledTargetStatus.Active;
    
    /// <summary>
    /// Number of times to repeat imaging (mosaic panels)
    /// </summary>
    [Range(1, 100)]
    public int RepeatCount { get; set; } = 1;
    
    /// <summary>
    /// REQUIRED: Observatory for this scheduled target
    /// </summary>
    [Required]
    public Guid ObservatoryId { get; set; }
    
    /// <summary>
    /// REQUIRED: Equipment for this scheduled target
    /// </summary>
    [Required]
    public Guid EquipmentId { get; set; }
    
    
    /// <summary>
    /// Imaging goals (filter + goal time in minutes)
    /// Optional - can be configured later in the scheduler
    /// </summary>
    public List<CreateImagingGoalDto> ImagingGoals { get; set; } = new List<CreateImagingGoalDto>();
    
    /// <summary>
    /// Whether to remove the source target list item after creating scheduled target
    /// </summary>
    public bool RemoveSourceItem { get; set; } = false;
    
    /// <summary>
    /// Optional priority override (if not set, uses source item priority)
    /// </summary>
    [Range(1, 99, ErrorMessage = "Priority must be between 1 and 99")]
    public int? Priority { get; set; }
    
    /// <summary>
    /// Optional reference to a SchedulerTargetTemplate for scheduler settings
    /// </summary>
    public Guid? SchedulerTargetTemplateId { get; set; }
}

/// <summary>
/// DTO for creating an imaging goal - uses ExposureTemplate for exposure settings
/// </summary>
public class CreateImagingGoalDto
{
    /// <summary>
    /// Optional: ID of existing goal to update (null for new goals)
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Required: ExposureTemplate that defines filter, exposure time, binning, gain, offset
    /// </summary>
    [Required]
    public Guid ExposureTemplateId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Goal exposure count must be positive")]
    public int GoalExposureCount { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Completed exposures must be positive")]
    public int CompletedExposures { get; set; } = 0;
    
    public bool IsEnabled { get; set; } = true;
}
