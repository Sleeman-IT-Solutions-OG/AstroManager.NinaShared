using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings;

/// <summary>
/// DTO for creating a new imaging goal template
/// </summary>
public class CreateImagingGoalTemplateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Equipment this template is designed for (optional)
    /// </summary>
    public Guid? EquipmentId { get; set; }
    
    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Individual goals in this template
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one goal is required")]
    public List<CreateImagingGoalTemplateItemDto> Goals { get; set; } = new();
}

/// <summary>
/// DTO for creating an imaging goal template item
/// Now uses ExposureTemplateId to reference exposure settings
/// </summary>
public class CreateImagingGoalTemplateItemDto
{
    /// <summary>
    /// Reference to the exposure template (required)
    /// </summary>
    public Guid ExposureTemplateId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int GoalExposureCount { get; set; } = 24; // Default: 24 exposures
    
    public bool IsEnabled { get; set; } = true;
    
    // Legacy properties for backward compatibility during transition
    // These are ignored if ExposureTemplateId is set
    public ECameraFilter Filter { get; set; }
    public int FilterPriority { get; set; } = 50;
    public int ExposureTimeSeconds { get; set; } = 300;
    
    // Calculated property for backward compatibility
    [Range(0.1, double.MaxValue)]
    public double GoalTimeMinutes => (GoalExposureCount * ExposureTimeSeconds) / 60.0;
}

/// <summary>
/// DTO for updating an existing imaging goal template
/// </summary>
public class UpdateImagingGoalTemplateDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Equipment this template is designed for (optional)
    /// </summary>
    public Guid? EquipmentId { get; set; }
    
    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Individual goals in this template
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one goal is required")]
    public List<CreateImagingGoalTemplateItemDto> Goals { get; set; } = new();
}
