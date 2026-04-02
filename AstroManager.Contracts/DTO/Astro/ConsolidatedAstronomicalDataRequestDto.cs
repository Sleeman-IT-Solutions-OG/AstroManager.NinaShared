using Shared.Model.DTO.Settings;
using System.Text.Json.Serialization;

namespace Shared.Model.DTO.Astro
{
    /// <summary>
    /// Request DTO for getting consolidated astronomical data with moon avoidance profiles
    /// </summary>
    public class ConsolidatedAstronomicalDataRequestDto
    {
        /// <summary>
        /// Observatory for which to calculate astronomical data
        /// </summary>
        public ObservatoryDto Observatory { get; set; } = new();

        /// <summary>
        /// Selected date for astronomical calculations
        /// </summary>
        public DateTime SelectedDate { get; set; }
    }
}
