using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for Exposure Templates.
/// Defines reusable exposure configurations (Filter, ExposureTime, Binning, Gain, Offset, MoonAvoidanceProfile)
/// that can be referenced by imaging goals.
/// </summary>
public class ExposureTemplateDto
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    /// <summary>
    /// User-friendly name for this template (e.g., "Ha 300s", "LRGB Standard", "NB Deep")
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "Template name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Camera filter type
    /// </summary>
    [Required]
    public ECameraFilter Filter { get; set; }
    
    /// <summary>
    /// Exposure time in seconds
    /// </summary>
    [Required]
    [Range(1, 3600, ErrorMessage = "Exposure time must be between 1 and 3600 seconds")]
    public int ExposureTimeSeconds { get; set; } = 300;
    
    /// <summary>
    /// Camera binning (1x1, 2x2, etc.)
    /// </summary>
    [Range(1, 4, ErrorMessage = "Binning must be between 1 and 4")]
    public int Binning { get; set; } = 1;
    
    /// <summary>
    /// Camera gain setting
    /// </summary>
    [Range(-1, 1000, ErrorMessage = "Gain must be between -1 (default) and 1000")]
    public int Gain { get; set; } = -1;
    
    /// <summary>
    /// Camera offset setting
    /// </summary>
    [Range(-1, 1000, ErrorMessage = "Offset must be between -1 (default) and 1000")]
    public int Offset { get; set; } = -1;
    
    /// <summary>
    /// Reference to moon avoidance profile (optional)
    /// If set, this filter will use specific moon avoidance settings
    /// </summary>
    public Guid? MoonAvoidanceProfileId { get; set; }
    
    /// <summary>
    /// Moon avoidance profile name (for display)
    /// </summary>
    public string? MoonAvoidanceProfileName { get; set; }
    
    /// <summary>
    /// Default filter priority when creating imaging goals with this template.
    /// 1 = highest priority, -1 = use scheduler default
    /// </summary>
    [Range(-1, 200, ErrorMessage = "Filter priority must be between -1 (default) and 200")]
    public int DefaultFilterPriority { get; set; } = 1;
    
    /// <summary>
    /// Acceptable twilight for this filter (Astronomical or Nautical).
    /// Astronomical = darker (default), Nautical = allows some twilight
    /// </summary>
    public ETwilightType AcceptableTwilight { get; set; } = ETwilightType.Astronomical;
    
    /// <summary>
    /// Camera readout mode. Empty string = use NINA default.
    /// </summary>
    [StringLength(50)]
    public string? ReadoutMode { get; set; }
    
    /// <summary>
    /// Dither every X exposures. -1 = use NINA/scheduler default.
    /// </summary>
    [Range(-1, 100)]
    public int DitherEveryX { get; set; } = -1;
    
    /// <summary>
    /// Minimum altitude in degrees for this filter. -1 = use scheduler default.
    /// </summary>
    [Range(-1, 90)]
    public double MinAltitude { get; set; } = -1;

    /// <summary>
    /// Minimum auto grade band required for an image to be auto-accepted.
    /// A is strictest, E accepts all.
    /// </summary>
    [RegularExpression("^[A-E]$", ErrorMessage = "Minimum grade band must be A, B, C, D, or E")]
    public string MinGradeBand { get; set; } = "E";
    
    /// <summary>
    /// Whether this template is active and available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Observatory ID this template belongs to (for template scoping)
    /// </summary>
    public Guid? ObservatoryId { get; set; }
    
    /// <summary>
    /// Observatory name (for display)
    /// </summary>
    public string? ObservatoryName { get; set; }
    
    /// <summary>
    /// Equipment profile ID this template is associated with (for template scoping)
    /// </summary>
    public Guid? EquipmentId { get; set; }
    
    /// <summary>
    /// Equipment name (for display)
    /// </summary>
    public string? EquipmentName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Display name combining filter and exposure time (e.g., "Ha - 300s")
    /// </summary>
    public string DisplayName => $"{Filter} - {ExposureTimeSeconds}s";
    
    /// <summary>
    /// Full display name with binning and gain if non-default
    /// </summary>
    public string FullDisplayName
    {
        get
        {
            var parts = new List<string> { $"{Filter}", $"{ExposureTimeSeconds}s" };
            if (Binning > 1) parts.Add($"Bin{Binning}");
            if (Gain >= 0) parts.Add($"G{Gain}");
            if (Offset >= 0) parts.Add($"O{Offset}");
            return string.Join(" | ", parts);
        }
    }
}

/// <summary>
/// DTO for creating a new imaging template
/// </summary>
public class CreateExposureTemplateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public ECameraFilter Filter { get; set; }
    
    [Required]
    [Range(1, 3600)]
    public int ExposureTimeSeconds { get; set; } = 300;
    
    [Range(1, 4)]
    public int Binning { get; set; } = 1;
    
    [Range(-1, 1000)]
    public int Gain { get; set; } = -1;
    
    [Range(-1, 1000)]
    public int Offset { get; set; } = -1;
    
    public Guid? MoonAvoidanceProfileId { get; set; }
    
    [Range(-1, 200)]
    public int DefaultFilterPriority { get; set; } = 1;
    
    public ETwilightType AcceptableTwilight { get; set; } = ETwilightType.Astronomical;
    
    [StringLength(50)]
    public string? ReadoutMode { get; set; }
    
    [Range(-1, 100)]
    public int DitherEveryX { get; set; } = -1;
    
    [Range(-1, 90)]
    public double MinAltitude { get; set; } = -1;

    [RegularExpression("^[A-E]$")]
    public string MinGradeBand { get; set; } = "E";
    
    public Guid? ObservatoryId { get; set; }
    public Guid? EquipmentId { get; set; }
}

/// <summary>
/// DTO for updating an imaging template
/// </summary>
public class UpdateExposureTemplateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public ECameraFilter Filter { get; set; }
    
    [Required]
    [Range(1, 3600)]
    public int ExposureTimeSeconds { get; set; } = 300;
    
    [Range(1, 4)]
    public int Binning { get; set; } = 1;
    
    [Range(-1, 1000)]
    public int Gain { get; set; } = -1;
    
    [Range(-1, 1000)]
    public int Offset { get; set; } = -1;
    
    public Guid? MoonAvoidanceProfileId { get; set; }
    
    [Range(-1, 200)]
    public int DefaultFilterPriority { get; set; } = 1;
    
    public ETwilightType AcceptableTwilight { get; set; } = ETwilightType.Astronomical;
    
    [StringLength(50)]
    public string? ReadoutMode { get; set; }
    
    [Range(-1, 100)]
    public int DitherEveryX { get; set; } = -1;
    
    [Range(-1, 90)]
    public double MinAltitude { get; set; } = -1;

    [RegularExpression("^[A-E]$")]
    public string MinGradeBand { get; set; } = "E";
    
    public bool IsActive { get; set; } = true;
    
    public Guid? ObservatoryId { get; set; }
    public Guid? EquipmentId { get; set; }
    
    /// <summary>
    /// For optimistic locking - must match server's UpdatedAt or update will be rejected
    /// </summary>
    public DateTime? LastKnownUpdatedAt { get; set; }
}
