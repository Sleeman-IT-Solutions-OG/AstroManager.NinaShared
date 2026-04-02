using System.ComponentModel.DataAnnotations;
using Shared.Model.Enums;

namespace Shared.Model.DTO.DSO
{
    /// <summary>
    /// Data Transfer Object for user-saved deep sky object filters
    /// </summary>
    public class SavedFilterDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the saved filter
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user-defined name for the filter
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filter configuration (can be DeepSkyObjectSearchFilterDto or TargetSearchFilterDto)
        /// </summary>
        [Required]
        public object Filter { get; set; } = new DeepSkyObjectSearchFilterDto();

        /// <summary>
        /// Gets or sets when the filter was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the filter was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// Gets or sets the user ID who owns this filter
        /// </summary>
        public Guid UserId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the type of filter (DSO or Target)
        /// </summary>
        public QuickFilterType FilterType { get; set; } = QuickFilterType.DeepSkyObject;
    }

    /// <summary>
    /// Request DTO for creating a new saved filter
    /// </summary>
    public class CreateSavedFilterDto
    {
        /// <summary>
        /// Gets or sets the user-defined name for the filter
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filter configuration (can be DeepSkyObjectSearchFilterDto or TargetSearchFilterDto)
        /// </summary>
        [Required]
        public object Filter { get; set; } = new DeepSkyObjectSearchFilterDto();

        /// <summary>
        /// Gets or sets the type of filter (DSO or Target)
        /// </summary>
        public QuickFilterType FilterType { get; set; } = QuickFilterType.DeepSkyObject;
    }

    /// <summary>
    /// Request DTO for updating an existing saved filter
    /// </summary>
    public class UpdateSavedFilterDto
    {
        /// <summary>
        /// Gets or sets the user-defined name for the filter
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filter configuration (can be DeepSkyObjectSearchFilterDto or TargetSearchFilterDto)
        /// </summary>
        [Required]
        public object Filter { get; set; } = new DeepSkyObjectSearchFilterDto();

        /// <summary>
        /// Gets or sets the type of filter (DSO or Target)
        /// </summary>
        public QuickFilterType FilterType { get; set; } = QuickFilterType.DeepSkyObject;
    }

    /// <summary>
    /// DTO for constellation display with full name and abbreviation
    /// </summary>
    public class ConstellationDisplayDto
    {
        /// <summary>
        /// Gets or sets the constellation abbreviation (e.g., "And")
        /// </summary>
        public string Abbreviation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full constellation name (e.g., "Andromeda")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the display text for the constellation (e.g., "Andromeda (And)")
        /// </summary>
        public string DisplayText => $"{Name} ({Abbreviation})";

        /// <summary>
        /// Gets the value to use for filtering (abbreviation)
        /// </summary>
        public string Value => Abbreviation;
    }

    /// <summary>
    /// DTO for object type display options
    /// </summary>
    public class ObjectTypeDisplayDto
    {
        /// <summary>
        /// Gets or sets the object type identifier
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display label
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a base type
        /// </summary>
        public bool IsBaseType { get; set; }

        /// <summary>
        /// Gets or sets the base type category (for grouping)
        /// </summary>
        public string? BaseType { get; set; }
    }
}
