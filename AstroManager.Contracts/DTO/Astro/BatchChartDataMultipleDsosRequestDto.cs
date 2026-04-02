using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Model.DTO.Settings;

namespace Shared.Model.DTO.Astro
{
    /// <summary>
    /// Request DTO for getting batch chart data for multiple DSOs with the same observatory and time parameters
    /// </summary>
    public class BatchChartDataMultipleDsosRequestDto
    {
        /// <summary>
        /// Gets or sets the list of DSO IDs to get chart data for
        /// </summary>
        [Required]
        public List<Guid> DsoIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the list of right ascensions (in degrees) corresponding to the DSO IDs
        /// </summary>
        [Required]
        public List<double> RightAscensions { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the list of declinations (in degrees) corresponding to the DSO IDs
        /// </summary>
        [Required]
        public List<double> Declinations { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the observatory information for calculations
        /// </summary>
        [Required]
        public ObservatoryDto Observatory { get; set; } = null!;

        /// <summary>
        /// Gets or sets the start time for chart data calculation
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for chart data calculation
        /// </summary>
        [Required]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the time step between data points
        /// </summary>
        [Required]
        public TimeSpan TimeStep { get; set; }
    }
}
