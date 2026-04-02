using Shared.Model.DTO.Settings;

namespace Shared.Model.DTO.Astro
{
    /// <summary>
    /// Combined DTO containing astronomical data and moon avoidance profiles
    /// </summary>
    public class AstronomicalDataWithMoonAvoidanceDto
    {
        /// <summary>
        /// The astronomical data (twilight times, moon data, etc.)
        /// </summary>
        public AstronomicalData AstronomicalData { get; set; }

        /// <summary>
        /// User's filter-specific moon avoidance profile assignments
        /// </summary>
        public List<UserFilterMoonAvoidanceProfileDto> FilterAssignments { get; set; } = new List<UserFilterMoonAvoidanceProfileDto>();

        /// <summary>
        /// All available moon avoidance profiles for the user
        /// </summary>
        public List<MoonAvoidanceProfileDto> AvailableProfiles { get; set; } = new List<MoonAvoidanceProfileDto>();
    }
}
