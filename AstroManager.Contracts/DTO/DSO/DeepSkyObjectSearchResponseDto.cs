using System.Collections.Generic;
using Shared.Model.DTO.Astro;

namespace Shared.Model.DTO.DSO
{
    /// <summary>
    /// Data Transfer Object for deep sky object search results
    /// </summary>
    public class DeepSkyObjectSearchResponseDto
    {
        /// <summary>
        /// Gets or sets the list of deep sky objects matching the search criteria
        /// </summary>
        public List<DeepSkyObjectSearchResultDto> Results { get; set; } = new List<DeepSkyObjectSearchResultDto>();

        /// <summary>
        /// Gets or sets the total number of objects matching the search criteria (before pagination)
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets lightweight metadata for all matching DSOs (for optimized paging).
        /// Only populated on the initial search (page 1) to enable efficient subsequent page loading.
        /// Contains only Guid, MatchingName, and MatchPriority - no full DSO data.
        /// Will be empty if server-side caching is used (UseServerCache = true).
        /// </summary>
        public List<DeepSkyObjectMetadataDto> AllMatchingMetadata { get; set; } = new List<DeepSkyObjectMetadataDto>();

        /// <summary>
        /// Indicates if the server has cached the metadata for this search result set.
        /// When true, subsequent page requests should use UseServerCache = true.
        /// </summary>
        public bool HasServerCachedMetadata { get; set; } = false;

        /// <summary>
        /// Gets or sets the preloaded chart data for DSOs in the current page.
        /// Only populated when PreloadChartData is true in the search filter.
        /// Key is the DSO ID, Value is the chart data.
        /// </summary>
        public Dictionary<Guid, BatchChartDataResponseDto> ChartData { get; set; } = new Dictionary<Guid, BatchChartDataResponseDto>();

        /// <summary>
        /// Gets or sets the unique search identifier for SignalR updates.
        /// Used by the client to join SignalR groups for real-time total count updates.
        /// Only populated for observability-filtered searches with background processing.
        /// </summary>
        public string? SearchId { get; set; }
    }
}
