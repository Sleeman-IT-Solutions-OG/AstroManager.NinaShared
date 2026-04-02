using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings
{
    /// <summary>
    /// Request DTO for calculating moon avoidance distance
    /// </summary>
    public class MoonAvoidanceCalculationRequestDto
    {
        /// <summary>
        /// Moon illumination fraction (0.0 to 1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double IlluminationFraction { get; set; }

        /// <summary>
        /// Profile ID to use for calculation (optional - uses active profile if not specified)
        /// </summary>
        public Guid? ProfileId { get; set; }

        /// <summary>
        /// Full moon distance in degrees (optional - overrides profile setting)
        /// </summary>
        [Range(0.0, 180.0)]
        public double? FullMoonDistanceDegrees { get; set; }

        /// <summary>
        /// Width in days (optional - overrides profile setting)
        /// </summary>
        [Range(0.1, 60.0)]
        public double? WidthInDays { get; set; }
    }

    /// <summary>
    /// Response DTO for moon avoidance distance calculation
    /// </summary>
    public class MoonAvoidanceCalculationResponseDto
    {
        /// <summary>
        /// Calculated avoidance distance in degrees
        /// </summary>
        public double AvoidanceDistanceDegrees { get; set; }

        /// <summary>
        /// Moon illumination fraction used in calculation
        /// </summary>
        public double IlluminationFraction { get; set; }

        /// <summary>
        /// Profile used for calculation
        /// </summary>
        public MoonAvoidanceProfileDto? Profile { get; set; }

        /// <summary>
        /// Full moon distance used in calculation
        /// </summary>
        public double FullMoonDistanceDegrees { get; set; }

        /// <summary>
        /// Width in days used in calculation
        /// </summary>
        public double WidthInDays { get; set; }

        /// <summary>
        /// Timestamp when calculation was performed
        /// </summary>
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for getting moon avoidance phase table
    /// </summary>
    public class MoonAvoidancePhaseTableRequestDto
    {
        /// <summary>
        /// Profile ID to use for calculation (optional - uses active profile if not specified)
        /// </summary>
        public Guid? ProfileId { get; set; }

        /// <summary>
        /// Step size for phase table (default 5% increments)
        /// </summary>
        [Range(1, 25)]
        public int StepSize { get; set; } = 5;

        /// <summary>
        /// Full moon distance in degrees (optional - overrides profile setting)
        /// </summary>
        [Range(0.0, 180.0)]
        public double? FullMoonDistanceDegrees { get; set; }

        /// <summary>
        /// Width in days (optional - overrides profile setting)
        /// </summary>
        [Range(0.1, 30.0)]
        public double? WidthInDays { get; set; }
    }

    /// <summary>
    /// Response DTO for moon avoidance phase table
    /// </summary>
    public class MoonAvoidancePhaseTableResponseDto
    {
        /// <summary>
        /// Dictionary mapping moon phase percentage to avoidance distance in degrees
        /// </summary>
        public Dictionary<int, double> PhaseDistanceTable { get; set; } = new();

        /// <summary>
        /// Profile used for calculation
        /// </summary>
        public MoonAvoidanceProfileDto? Profile { get; set; }

        /// <summary>
        /// Full moon distance used in calculation
        /// </summary>
        public double FullMoonDistanceDegrees { get; set; }

        /// <summary>
        /// Width in days used in calculation
        /// </summary>
        public double WidthInDays { get; set; }

        /// <summary>
        /// Step size used for phase table
        /// </summary>
        public int StepSize { get; set; }

        /// <summary>
        /// Timestamp when calculation was performed
        /// </summary>
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for checking if a DSO is observable given moon conditions
    /// </summary>
    public class MoonAvoidanceObservabilityRequestDto
    {
        /// <summary>
        /// DSO Right Ascension in hours
        /// </summary>
        [Range(0.0, 24.0)]
        public double DsoRightAscension { get; set; }

        /// <summary>
        /// DSO Declination in degrees
        /// </summary>
        [Range(-90.0, 90.0)]
        public double DsoDeclination { get; set; }

        /// <summary>
        /// Moon Right Ascension in hours
        /// </summary>
        [Range(0.0, 24.0)]
        public double MoonRightAscension { get; set; }

        /// <summary>
        /// Moon Declination in degrees
        /// </summary>
        [Range(-90.0, 90.0)]
        public double MoonDeclination { get; set; }

        /// <summary>
        /// Moon illumination fraction (0.0 to 1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double MoonIlluminationFraction { get; set; }

        /// <summary>
        /// Profile ID to use for calculation (optional - uses active profile if not specified)
        /// </summary>
        public Guid? ProfileId { get; set; }
        
        /// <summary>
        /// Observer latitude in degrees
        /// </summary>
        [Range(-90.0, 90.0)]
        public double ObserverLatitude { get; set; }
        
        /// <summary>
        /// Observer longitude in degrees
        /// </summary>
        [Range(-180.0, 180.0)]
        public double ObserverLongitude { get; set; }
        
        /// <summary>
        /// Observation date and time (UTC)
        /// </summary>
        public DateTime ObservationTime { get; set; }
    }

    /// <summary>
    /// Response DTO for moon avoidance observability check
    /// </summary>
    public class MoonAvoidanceObservabilityResponseDto
    {
        /// <summary>
        /// Whether the DSO is observable given moon conditions
        /// </summary>
        public bool IsObservable { get; set; }

        /// <summary>
        /// Angular distance between DSO and Moon in degrees
        /// </summary>
        public double AngularDistanceDegrees { get; set; }

        /// <summary>
        /// Required avoidance distance for current moon phase in degrees
        /// </summary>
        public double RequiredAvoidanceDistance { get; set; }

        /// <summary>
        /// Moon illumination fraction used in calculation
        /// </summary>
        public double MoonIlluminationFraction { get; set; }

        /// <summary>
        /// Profile used for calculation
        /// </summary>
        public MoonAvoidanceProfileDto? Profile { get; set; }

        /// <summary>
        /// Observability status description
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when calculation was performed
        /// </summary>
        public DateTime CalculatedAt { get; set; }
    }
}
