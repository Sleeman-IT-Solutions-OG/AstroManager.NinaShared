using System;

namespace Shared.Model.DTO.DSO
{
    /// <summary>
    /// Lightweight DTO containing essential metadata for DSO search results including all fields needed for sorting.
    /// Used for optimized paging to cache search result metadata without full DSO data.
    /// </summary>
    public class DeepSkyObjectMetadataDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the deep sky object.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name that matched the search criteria.
        /// </summary>
        public string MatchingName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the priority of the matching name (lower = higher priority).
        /// Used for Popularity sorting.
        /// </summary>
        public int MatchPriority { get; set; }

        /// <summary>
        /// Gets or sets the right ascension in hours.
        /// Used for Right Ascension sorting.
        /// </summary>
        public double RightAscension { get; set; }

        /// <summary>
        /// Gets or sets the declination in degrees.
        /// Used for Declination sorting.
        /// </summary>
        public double Declination { get; set; }

        /// <summary>
        /// Gets or sets the object type (base type).
        /// Used for Object Type sorting.
        /// </summary>
        public string ObjectType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum size in arcminutes.
        /// Used for Size sorting. Null values are sorted last.
        /// </summary>
        public double? SizeMaxArcmin { get; set; }

        /// <summary>
        /// Gets or sets the visual magnitude.
        /// Used for Magnitude sorting. Null values are sorted last.
        /// </summary>
        public double? Magnitude { get; set; }

        /// <summary>
        /// Gets or sets the observable duration in minutes during the observation window.
        /// Used for Observability Duration sorting. Null when observability filters are not applied.
        /// </summary>
        public int? ObservableMinutes { get; set; }
    }
}
