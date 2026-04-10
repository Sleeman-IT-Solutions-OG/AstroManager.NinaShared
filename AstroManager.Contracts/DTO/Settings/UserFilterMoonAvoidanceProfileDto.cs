using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings
{
    /// <summary>
    /// Data Transfer Object for User Filter Moon Avoidance Profile mapping
    /// </summary>
    public class UserFilterMoonAvoidanceProfileDto
    {
        /// <summary>
        /// Unique identifier for the mapping
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User ID who owns this filter-profile mapping
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Camera filter for this mapping
        /// </summary>
        [Required]
        public ECameraFilter Filter { get; set; }

        /// <summary>
        /// Optional user filter definition ID for additive custom-filter support.
        /// </summary>
        public Guid? FilterDefinitionId { get; set; }

        /// <summary>
        /// Optional user-visible filter name.
        /// </summary>
        public string? FilterName { get; set; }

        /// <summary>
        /// Optional additive standard filter reference.
        /// </summary>
        public ECameraFilter? StandardFilter { get; set; }

        /// <summary>
        /// Moon avoidance profile ID assigned to this filter
        /// </summary>
        [Required]
        public Guid MoonAvoidanceProfileId { get; set; }

        /// <summary>
        /// Moon avoidance profile details (populated when needed)
        /// </summary>
        public MoonAvoidanceProfileDto? MoonAvoidanceProfile { get; set; }

        /// <summary>
        /// When the mapping was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the mapping was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for setting a user's filter-specific moon avoidance profile
    /// </summary>
    public class SetUserFilterProfileRequestDto
    {
        /// <summary>
        /// Camera filter to assign profile to
        /// </summary>
        [Required]
        public ECameraFilter Filter { get; set; }

        /// <summary>
        /// Moon avoidance profile ID to assign to the filter
        /// </summary>
        [Required]
        public Guid MoonAvoidanceProfileId { get; set; }
    }

    /// <summary>
    /// Response DTO containing all filter-profile mappings for a user
    /// </summary>
    public class UserFilterProfileMappingsResponseDto
    {
        /// <summary>
        /// Dictionary mapping each filter to its assigned profile
        /// </summary>
        public Dictionary<ECameraFilter, MoonAvoidanceProfileDto> FilterProfiles { get; set; } = new();

        /// <summary>
        /// List of available filters that don't have profiles assigned
        /// </summary>
        public List<ECameraFilter> UnassignedFilters { get; set; } = new();

        /// <summary>
        /// When the mappings were last retrieved
        /// </summary>
        public DateTime RetrievedAt { get; set; }
    }
}
