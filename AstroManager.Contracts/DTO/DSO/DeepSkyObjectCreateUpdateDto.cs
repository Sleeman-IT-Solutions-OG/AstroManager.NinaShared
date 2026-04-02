using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.DSO;

/// <summary>
/// DTO for creating or updating a Deep Sky Object
/// </summary>
public class DeepSkyObjectCreateUpdateDto
{
    /// <summary>
    /// Primary identifier (e.g., M45, NGC1234)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Right Ascension in hours (0-24)
    /// </summary>
    [Required]
    [Range(0, 24)]
    public double RightAscension { get; set; }

    /// <summary>
    /// Declination in degrees (-90 to +90)
    /// </summary>
    [Required]
    [Range(-90, 90)]
    public double Declination { get; set; }

    /// <summary>
    /// Constellation IAU abbreviation (e.g., Ori, And)
    /// </summary>
    [MaxLength(10)]
    public string? Constellation { get; set; }

    /// <summary>
    /// SIMBAD object type ID
    /// </summary>
    public int? SimbadObjectTypeId { get; set; }

    /// <summary>
    /// Visual magnitude
    /// </summary>
    public double? MagnitudeV { get; set; }

    /// <summary>
    /// Photographic magnitude
    /// </summary>
    public double? MagnitudeP { get; set; }

    /// <summary>
    /// Distance in light years
    /// </summary>
    public double? Distance { get; set; }

    /// <summary>
    /// Major axis size in arcminutes
    /// </summary>
    public double? SizeMaxArcmin { get; set; }

    /// <summary>
    /// Minor axis size in arcminutes
    /// </summary>
    public double? SizeMinArcmin { get; set; }

    /// <summary>
    /// Position angle in degrees
    /// </summary>
    public double? PositionAngle { get; set; }

    /// <summary>
    /// Morphological type
    /// </summary>
    public string? MorphType { get; set; }

    /// <summary>
    /// Popularity score
    /// </summary>
    public int? Popularity { get; set; }

    /// <summary>
    /// Notes about the object
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Alternative names for the object (comma-separated)
    /// </summary>
    public string? AlternativeNames { get; set; }
}
