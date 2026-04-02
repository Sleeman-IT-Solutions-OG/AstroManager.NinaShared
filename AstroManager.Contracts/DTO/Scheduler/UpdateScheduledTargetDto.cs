using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Request DTO for updating a scheduled target
/// </summary>
public class UpdateScheduledTargetDto
{
    [Required]
    public Guid Id { get; set; }
    
    [StringLength(100, ErrorMessage = "Target name cannot exceed 100 characters")]
    public string? Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Range(1, 99, ErrorMessage = "Priority must be between 1 and 99")]
    public int? Priority { get; set; }
    
    [Range(1, 100, ErrorMessage = "Repeat count must be between 1 and 100")]
    public int? RepeatCount { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter<ScheduledTargetStatus>))]
    public ScheduledTargetStatus? Status { get; set; }
    
    public string? Notes { get; set; }
    
    // Coordinates
    public double? RightAscension { get; set; }
    public double? Declination { get; set; }
    public double? PA { get; set; }
    
    // Physical Properties
    public string? ObjectType { get; set; }
    public double? MagnitudeV { get; set; }
    public double? Distance { get; set; }
    public double? SizeMinArcmin { get; set; }
    public double? SizeMaxArcmin { get; set; }
    
    public List<ECameraFilter>? RelevantFilters { get; set; }
    
    public List<string>? UserTags { get; set; }
    
    /// <summary>
    /// Updated imaging goals (will replace existing goals)
    /// </summary>
    public List<CreateImagingGoalDto>? ImagingGoals { get; set; }
    
    // Mosaic properties
    public bool? IsMosaic { get; set; }
    public int? MosaicPanelsX { get; set; }
    public int? MosaicPanelsY { get; set; }
    public double? MosaicOverlapPercent { get; set; }
    public bool? MosaicUseRotator { get; set; }
    public bool? UseCustomPanelGoals { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<MosaicShootingStrategy>))]
    public MosaicShootingStrategy? MosaicShootingStrategy { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<MosaicPanelOrderingMethod>))]
    public MosaicPanelOrderingMethod? MosaicPanelOrderingMethod { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<GoalOrderingMethod>))]
    public GoalOrderingMethod? GoalOrderingMethod { get; set; }
    
    // Image-related properties
    public bool? ShowImage { get; set; }
    public string? AstroBinImageId { get; set; }
    public string? AstroBinImageUrl { get; set; }
    
    /// <summary>
    /// Optional reference to a SchedulerTargetTemplate for scheduler settings
    /// </summary>
    public Guid? SchedulerTargetTemplateId { get; set; }

    /// <summary>
    /// When true, apply SchedulerTargetTemplateId update (including null to clear assignment).
    /// When false, keep existing assignment unchanged.
    /// </summary>
    public bool UpdateSchedulerTargetTemplate { get; set; } = false;
}
