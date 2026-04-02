using Shared.Model.DTO.Scheduler;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings;

/// <summary>
/// DTO for predefined imaging goal template
/// </summary>
public class ImagingGoalTemplateDto
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Equipment this template is designed for (optional)
    /// </summary>
    public Guid? EquipmentId { get; set; }
    
    /// <summary>
    /// Individual goals in this template
    /// </summary>
    public List<ImagingGoalTemplateItemDto> Goals { get; set; } = new();
    
    /// <summary>
    /// Whether this is a system default template
    /// </summary>
    public bool IsSystemDefault { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Individual goal within a template
/// Can optionally reference an ExposureTemplate, or define properties directly
/// </summary>
public class ImagingGoalTemplateItemDto
{
    /// <summary>
    /// Optional reference to an exposure template
    /// </summary>
    public Guid? ExposureTemplateId { get; set; }
    
    /// <summary>
    /// The linked ExposureTemplate (populated when retrieved)
    /// </summary>
    public ExposureTemplateDto? ExposureTemplate { get; set; }
    
    public ECameraFilter Filter { get; set; }
    public int FilterPriority { get; set; } = 50;
    public int ExposureTimeSeconds { get; set; } = 300;
    
    [Range(1, int.MaxValue)]
    public int GoalExposureCount { get; set; } = 120; // Default 10 hours at 5 min exposures
    
    // Computed property for backward compatibility
    public double GoalTimeMinutes => (GoalExposureCount * ExposureTimeSeconds) / 60.0;
    
    public bool IsEnabled { get; set; } = true;
}
