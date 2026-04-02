namespace Shared.Model.DTO.DSO;

/// <summary>
/// DTO for nearby object search functionality
/// </summary>
public class SearchNearbyDto
{
    /// <summary>
    /// Whether nearby search is active
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Center Right Ascension in hours (0-24)
    /// </summary>
    public double CenterRA { get; set; }

    /// <summary>
    /// Center Declination in degrees (-90 to +90)
    /// </summary>
    public double CenterDec { get; set; }

    /// <summary>
    /// Search radius in degrees
    /// </summary>
    public double SearchRadiusDegrees { get; set; } = 5.0;

    /// <summary>
    /// Whether to sort results by distance ascending (closest first)
    /// </summary>
    public bool SortByDistanceAscending { get; set; } = true;

    /// <summary>
    /// Creates a deep copy of this SearchNearbyDto
    /// </summary>
    public SearchNearbyDto DeepCopy()
    {
        return new SearchNearbyDto
        {
            IsActive = this.IsActive,
            CenterRA = this.CenterRA,
            CenterDec = this.CenterDec,
            SearchRadiusDegrees = this.SearchRadiusDegrees,
            SortByDistanceAscending = this.SortByDistanceAscending
        };
    }
}
