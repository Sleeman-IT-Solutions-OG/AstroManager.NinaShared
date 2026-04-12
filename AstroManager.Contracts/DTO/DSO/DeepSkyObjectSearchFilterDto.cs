using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Model.DTO.Astro;

namespace Shared.Model.DTO.DSO
{
    /// <summary>
    /// Data Transfer Object for deep sky object search filters
    /// </summary>
    public class DeepSkyObjectSearchFilterDto
    {
        /// <summary>
        /// Gets or sets the search term to filter by object name or identifier
        /// </summary>
        [MaxLength(100)]
        public string SearchTerm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of catalog identifiers to filter by (e.g., ["M", "NGC"])
        /// </summary>
        public List<string> Catalogs { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of object types to filter by (e.g., ["Galaxy", "Nebula"])
        /// </summary>
        public List<string> ObjectTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of constellation abbreviations to filter by (e.g., ["AND", "ORI"])
        /// </summary>
        public List<string> Constellations { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the right ascension range (0-24 hours)
        /// </summary>
        public double[] RaRange { get; set; } = new double[2] { 0, 24 };

        /// <summary>
        /// Gets or sets the declination range (-90 to 90 degrees)
        /// </summary>
        public double[] DecRange { get; set; } = new double[2] { -90, 90 };

        /// <summary>
        /// Gets or sets the minimum apparent magnitude (brighter objects have smaller numbers)
        /// </summary>
        public double? MinMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the maximum apparent magnitude (dimmer objects have larger numbers)
        /// </summary>
        public double? MaxMagnitude { get; set; }


        /// <summary>
        /// Gets or sets the minimum size in arcminutes
        /// </summary>
        [Range(0, double.MaxValue)]
        public double? MinSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size in arcminutes
        /// </summary>
        [Range(0, double.MaxValue)]
        public double? MaxSize { get; set; }

        /// <summary>
        /// Gets or sets whether to include objects with unknown/null magnitude values
        /// </summary>
        public bool IncludeUnknownMagnitude { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include objects with unknown/null size values
        /// </summary>
        public bool IncludeUnknownSize { get; set; } = true;

        public ObservationWindowDto ObservationWindow { get; set; } = new();



        /// <summary>
        /// Gets or sets the list of required filters for observability (L, R, G, B, Ha, OIII, SII)
        /// </summary>
        public List<string> RequiredObservabilityFilters { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the minimum duration in minutes that the object must be observable
        /// </summary>
        [Range(1, 1440)] // 1 minute to 24 hours
        public int? MinObservabilityDurationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the seasonal observability aggregation period.
        /// Deprecated in favor of <see cref="SeasonalObservabilityDays"/> but retained for backwards compatibility.
        /// </summary>
        public SeasonalObservabilityPeriod? SeasonalObservabilityPeriod { get; set; }

        /// <summary>
        /// Gets or sets the number of future days to aggregate seasonal observability for.
        /// When set together with <see cref="MinSeasonalObservabilityHours"/>, the search filters by
        /// total observable time across the selected future period.
        /// </summary>
        [Range(1, 365)]
        public int? SeasonalObservabilityDays { get; set; }

        /// <summary>
        /// Gets or sets the minimum total seasonal observability in hours.
        /// </summary>
        [Range(1, 8760)]
        public int? MinSeasonalObservabilityHours { get; set; }

        /// <summary>
        /// Gets or sets whether to use faster but less precise observability calculations.
        /// When enabled, coordinates are rounded to 0.4 degrees for caching which improves performance but reduces precision.
        /// Defaults to true on mobile devices, false on desktop.
        /// </summary>
        public bool UseFastObservabilitySearch { get; set; } = false;


        /// <summary>
        /// Gets or sets the popularity filter level
        /// </summary>
        public PopularityLevel? PopularityFilter { get; set; }

        // Sorting options
        public bool OrderByPopularity { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the sort field for ordering results
        /// </summary>
        public SortField SortBy { get; set; } = SortField.Popularity;
        
        /// <summary>
        /// Gets or sets the sort direction (true for ascending, false for descending)
        /// </summary>
        public bool SortAscending { get; set; } = false;

        /// <summary>
        /// Gets or sets the chart data request parameters for preloading altitude chart data.
        /// When provided, chart data will be included in the search response for performance optimization.
        /// </summary>
        public BatchChartDataRequestDto? ChartDataRequest { get; set; } = null;

        /// <summary>
        /// Gets or sets whether to include image data in the search response.
        /// When true, image data will be included for each DSO to avoid separate API calls.
        /// </summary>
        public bool IncludeImageData { get; set; } = true;

        /// <summary>
        /// Gets or sets the page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of items per page
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets the list of DSO IDs to skip (for progressive pagination)
        /// This allows the client to maintain pagination state and avoid re-processing objects
        /// </summary>
        public List<Guid> SkippedDsoIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets whether to calculate and return the total count of matching objects
        /// When false, improves performance by avoiding full dataset processing
        /// </summary>
        public bool RequireTotalCount { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include chart data in the response
        /// When false, improves performance by skipping expensive chart data calculations
        /// Used for background preloading of pages without chart data
        /// </summary>
        public bool IncludeChartData { get; set; } = true;

        /// <summary>
        /// Indicates if this is a preloading request (for performance optimization)
        /// </summary>
        public bool IsPreloadingRequest { get; set; } = false;

        /// <summary>
        /// List of specific DSO IDs to load (for optimized paging - subsequent pages)
        /// When populated, skips search and loads only these specific DSOs with full data
        /// </summary>
        public List<Guid> LoadByDsoIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Indicates if the client wants to use server-side cached metadata for subsequent pages
        /// Should only be true for page navigation (not initial search)
        /// </summary>
        public bool UseServerCache { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to enable clustering of nearby DSOs to reduce UI clutter
        /// </summary>
        public bool EnableClustering { get; set; } = false;

        /// <summary>
        /// Gets or sets the clustering distance threshold in degrees
        /// DSOs within this distance will be considered for clustering
        /// </summary>
        public double ClusteringDistanceDegrees { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the maximum number of DSOs to return per cluster
        /// The most popular DSOs within each cluster will be selected
        /// </summary>
        public int MaxDSOsPerCluster { get; set; } = 3;

        /// <summary>
        /// Gets or sets the search identifier for SignalR updates and server-side caching.
        /// Used to maintain consistency across paginated requests and background processing.
        /// </summary>
        public string? SearchId { get; set; }

        /// <summary>
        /// Forces the search to use online API instead of offline database.
        /// Used for Android performance optimization where offline database is too slow.
        /// </summary>
        public bool ForceOnlineSearch { get; set; } = false;

        /// <summary>
        /// Gets or sets the nearby search configuration for coordinate-based proximity searches
        /// </summary>
        public SearchNearbyDto? SearchNearby { get; set; } = null;


        // Moon distance filter (in degrees)
        public double? MinDistanceFromMoon { get; set; }
        public double? MaxDistanceFromMoon { get; set; }



        public void ResetNullLists()
        {
            if (Constellations == null)
            {
                Constellations = new List<string>();
            }
            if (Catalogs == null)
            {
                Catalogs = new List<string>();
            }
            if (ObjectTypes == null)
            {
                ObjectTypes = new List<string>();
            }
            if (RequiredObservabilityFilters == null)
            {
                RequiredObservabilityFilters = new List<string>();
            }
            if (SkippedDsoIds == null)
            {
                SkippedDsoIds = new List<Guid>();
            }
        }


        /// <summary>
        /// Resets all search filters to their default values
        /// </summary>
        public void Reset()
        {
            ResetNullLists();

            SearchTerm = "";
            Catalogs.Clear();
            ObjectTypes.Clear();
            Constellations.Clear();
            RaRange = new double[2] { 0, 24 };
            DecRange = new double[2] { -90, 90 };
            MinMagnitude = null;
            MaxMagnitude = null;
            MinSize = null;
            MaxSize = null;
            IncludeUnknownMagnitude = true;
            IncludeUnknownSize = true;
            IncludeImageData = true;
            MinDistanceFromMoon = null;
            MaxDistanceFromMoon = null;
            RequiredObservabilityFilters.Clear();
            MinObservabilityDurationMinutes = null;
            SeasonalObservabilityPeriod = null;
            SeasonalObservabilityDays = null;
            MinSeasonalObservabilityHours = null;
            UseFastObservabilitySearch = false;
            OrderByPopularity = false;
            ObservationWindow = new ObservationWindowDto();
            SkippedDsoIds.Clear();
            RequireTotalCount = true;
            PopularityFilter = null;
            SearchNearby = null;
        }
        
        /// <summary>
        /// Determines whether any filters are currently active
        /// </summary>
        /// <returns>True if any filter is active; otherwise, false</returns>
        public bool HasActiveFilters()
        {
            return GetActiveFilterCount() > 0;
        }
        
        /// <summary>
        /// Gets the count of active filters
        /// </summary>
        /// <returns>The number of active filters</returns>
        public int GetActiveFilterCount()
        {
            int count = 0;

            ResetNullLists();

            // Check each filter
            if (Catalogs.Count > 0) count++;
            if (ObjectTypes.Count > 0) count++;
            if (Constellations.Count > 0) count++;
            if (RaRange.Count() >= 2 && (RaRange[0] > 0 || RaRange[1] < 24)) count++;
            if (DecRange.Count() >= 2 && (DecRange[0] > 0 || DecRange[1] < 24)) count++;
            if (MinMagnitude.HasValue) count++;
            if (MaxMagnitude.HasValue) count++;
            if (MinSize.HasValue) count++;
            if (MaxSize.HasValue) count++;
            if (MinDistanceFromMoon.HasValue) count++;
            if (MaxDistanceFromMoon.HasValue) count++;
            if (RequiredObservabilityFilters?.Any() == true) count++;
            if (MinObservabilityDurationMinutes.HasValue) count++;
            if (GetSeasonalObservabilityDays().HasValue || MinSeasonalObservabilityHours.HasValue) count++;
            if (ObservationWindow != null && 
                (ObservationWindow.StartTime.HasValue || 
                 ObservationWindow.EndTime.HasValue || 
                 ObservationWindow.MinAltitude.HasValue || 
                 ObservationWindow.MinDuration.HasValue)) count++;
            if (!IncludeUnknownMagnitude) count++; // Default is true, so false means it's an active filter
            if (!IncludeUnknownSize) count++; // Default is true, so false means it's an active filter
            if (PopularityFilter.HasValue) count++;
            if (SearchNearby?.IsActive == true) count++;

            return count;
        }

        /// <summary>
        /// Creates a deep copy of the current filter to prevent mutation of the original
        /// </summary>
        /// <returns>A new instance with all properties copied</returns>
        public DeepSkyObjectSearchFilterDto DeepCopy()
        {
            var copy = new DeepSkyObjectSearchFilterDto
            {
                SearchTerm = this.SearchTerm,
                Catalogs = new List<string>(this.Catalogs ?? new List<string>()),
                ObjectTypes = new List<string>(this.ObjectTypes ?? new List<string>()),
                Constellations = new List<string>(this.Constellations ?? new List<string>()),
                RaRange = new double[] { this.RaRange[0], this.RaRange[1] },
                DecRange = new double[] { this.DecRange[0], this.DecRange[1] },
                MinMagnitude = this.MinMagnitude,
                MaxMagnitude = this.MaxMagnitude,
                MinSize = this.MinSize,
                MaxSize = this.MaxSize,
                IncludeUnknownMagnitude = this.IncludeUnknownMagnitude,
                IncludeUnknownSize = this.IncludeUnknownSize,
                MinDistanceFromMoon = this.MinDistanceFromMoon,
                MaxDistanceFromMoon = this.MaxDistanceFromMoon,
                RequiredObservabilityFilters = new List<string>(this.RequiredObservabilityFilters ?? new List<string>()),
                MinObservabilityDurationMinutes = this.MinObservabilityDurationMinutes,
                SeasonalObservabilityPeriod = this.SeasonalObservabilityPeriod,
                SeasonalObservabilityDays = this.SeasonalObservabilityDays,
                MinSeasonalObservabilityHours = this.MinSeasonalObservabilityHours,
                UseFastObservabilitySearch = this.UseFastObservabilitySearch,
                PopularityFilter = this.PopularityFilter,
                OrderByPopularity = this.OrderByPopularity,
                ChartDataRequest = this.ChartDataRequest, // Note: This is a reference copy, but typically not modified by UI
                IncludeImageData = this.IncludeImageData,
                Page = this.Page,
                PageSize = this.PageSize,
                SkippedDsoIds = new List<Guid>(this.SkippedDsoIds ?? new List<Guid>()),
                RequireTotalCount = this.RequireTotalCount,
                IncludeChartData = this.IncludeChartData,
                IsPreloadingRequest = this.IsPreloadingRequest,
                LoadByDsoIds = new List<Guid>(this.LoadByDsoIds ?? new List<Guid>()),
                UseServerCache = this.UseServerCache,
                SearchId = this.SearchId,
                ForceOnlineSearch = this.ForceOnlineSearch,
                SearchNearby = this.SearchNearby?.DeepCopy(),
                ObservationWindow = this.ObservationWindow?.DeepCopy() ?? new ObservationWindowDto(),
                SortBy = this.SortBy,
                SortAscending = this.SortAscending
            };

            return copy;
        }


        public bool HasObservabilityFilter()
        {
            return HasPointInTimeObservabilityFilter() || HasSeasonalObservabilityFilter();
        }

        public bool HasPointInTimeObservabilityFilter()
        {
            return MinObservabilityDurationMinutes > 0 ||
                   (SortBy == SortField.ObservabilityDuration && !HasSeasonalObservabilityFilter());
        }

        public bool HasSeasonalObservabilityFilter()
        {
            return GetSeasonalObservabilityDays().HasValue &&
                   MinSeasonalObservabilityHours.HasValue &&
                   MinSeasonalObservabilityHours.Value > 0;
        }

        public int? GetSeasonalObservabilityDays()
        {
            if (SeasonalObservabilityDays is >= 1 and <= 365)
            {
                return SeasonalObservabilityDays;
            }

            return SeasonalObservabilityPeriod switch
            {
                DTO.DSO.SeasonalObservabilityPeriod.Next365Days => 365,
                DTO.DSO.SeasonalObservabilityPeriod.Next30Days => 30,
                _ => null
            };
        }

        /// <summary>
        /// Generates a hash code representing the current filter state for change detection
        /// </summary>
        /// <returns>Hash code representing all filter-relevant properties</returns>
        public string GetFilterHash()
        {
            var hashCode = new HashCode();
            
            // Core search filters
            hashCode.Add(SearchTerm ?? "");
            hashCode.Add(string.Join(",", Catalogs == null ? "" : Catalogs.OrderBy(x => x)));
            hashCode.Add(string.Join(",", ObjectTypes == null ? "" : ObjectTypes.OrderBy(x => x)));
            hashCode.Add(string.Join(",", Constellations == null ? "" : Constellations.OrderBy(x => x)));
            
            // Coordinate ranges
            hashCode.Add(RaRange?[0] ?? 0);
            hashCode.Add(RaRange?[1] ?? 24);
            hashCode.Add(DecRange?[0] ?? -90);
            hashCode.Add(DecRange?[1] ?? 90);
            
            // Magnitude and size filters
            hashCode.Add(MinMagnitude);
            hashCode.Add(MaxMagnitude);
            hashCode.Add(MinSize);
            hashCode.Add(MaxSize);
            hashCode.Add(IncludeUnknownMagnitude);
            hashCode.Add(IncludeUnknownSize);
            
            // Observability filters
            hashCode.Add(string.Join(",", RequiredObservabilityFilters == null ? "" : RequiredObservabilityFilters.OrderBy(x => x)));
            hashCode.Add(MinObservabilityDurationMinutes);
            hashCode.Add(GetSeasonalObservabilityDays());
            hashCode.Add(MinSeasonalObservabilityHours);
            hashCode.Add(UseFastObservabilitySearch);
            hashCode.Add(ObservationWindow?.StartTime);
            hashCode.Add(ObservationWindow?.EndTime);
            hashCode.Add(ObservationWindow?.MinAltitude);
            hashCode.Add(ObservationWindow?.MinDuration);
            hashCode.Add(ObservationWindow?.MinAltitudeOverride);
            
            // Other filters
            hashCode.Add(PopularityFilter);
            hashCode.Add(SortBy);
            hashCode.Add(SortAscending);
            hashCode.Add(MinDistanceFromMoon);
            hashCode.Add(MaxDistanceFromMoon);
            
            // SearchNearby
            if (SearchNearby?.IsActive == true)
            {
                hashCode.Add(SearchNearby.CenterRA);
                hashCode.Add(SearchNearby.CenterDec);
                hashCode.Add(SearchNearby.SearchRadiusDegrees);
                hashCode.Add(SearchNearby.SortByDistanceAscending);
            }
            
            // Clustering properties
            hashCode.Add(EnableClustering);
            hashCode.Add(ClusteringDistanceDegrees);
            hashCode.Add(MaxDSOsPerCluster);
            
            return hashCode.ToHashCode().ToString();
        }

        /// <summary>
        /// Determines if the filter has changed compared to another filter instance
        /// </summary>
        /// <param name="other">The other filter to compare against</param>
        /// <returns>True if filters are different; otherwise, false</returns>
        public bool HasChangedFrom(DeepSkyObjectSearchFilterDto? other)
        {
            if (other == null) return true;
            return GetFilterHash() != other.GetFilterHash();
        }
    }

    public enum SeasonalObservabilityPeriod
    {
        Next30Days = 30,
        Next365Days = 365
    }


    // Observation window filter
    public class ObservationWindowDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? MinAltitude { get; set; } // in degrees
        public int? MinDuration { get; set; } // in minutes
        
        /// <summary>
        /// User override for the observatory's effective minimum altitude.
        /// If set, this value will be used instead of observatory.GetEffectiveMinimumAltitude()
        /// </summary>
        public double? MinAltitudeOverride { get; set; } // in degrees

        /// <summary>
        /// Creates a deep copy of the current observation window
        /// </summary>
        /// <returns>A new instance with all properties copied</returns>
        public ObservationWindowDto DeepCopy()
        {
            return new ObservationWindowDto
            {
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                MinAltitude = this.MinAltitude,
                MinDuration = this.MinDuration,
                MinAltitudeOverride = this.MinAltitudeOverride
            };
        }
    }

    /// <summary>
    /// Enumeration of available sort fields for deep sky object search results
    /// </summary>
    public enum SortField
    {
        /// <summary>
        /// Sort by popularity/relevance score (default)
        /// </summary>
        Popularity,
        
        /// <summary>
        /// Sort by Right Ascension (RA) in hours
        /// </summary>
        RightAscension,
        
        /// <summary>
        /// Sort by Declination (DEC) in degrees
        /// </summary>
        Declination,
        
        /// <summary>
        /// Sort by object type (Galaxy, Nebula, etc.)
        /// </summary>
        ObjectType,
        
        /// <summary>
        /// Sort by object name alphabetically
        /// </summary>
        ObjectName,
        
        /// <summary>
        /// Sort by apparent size in arcminutes
        /// </summary>
        Size,
        
        /// <summary>
        /// Sort by apparent magnitude (brightness)
        /// </summary>
        Magnitude,
        
        /// <summary>
        /// Sort by observability duration in minutes
        /// </summary>
        ObservabilityDuration
    }

    /// <summary>
    /// Enumeration of popularity levels for filtering deep sky objects
    /// </summary>
    public enum PopularityLevel
    {
        /// <summary>
        /// Low popularity objects (less commonly observed)
        /// </summary>
        Low,
        
        /// <summary>
        /// Medium popularity objects (moderately observed)
        /// </summary>
        Mid,
        
        /// <summary>
        /// High popularity objects (commonly observed)
        /// </summary>
        High
    }
}
