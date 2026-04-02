using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings
{
    /// <summary>
    /// Data Transfer Object for Moon Avoidance Profile configuration
    /// </summary>
    public class MoonAvoidanceProfileDto
    {
        /// <summary>
        /// Unique identifier for the profile
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User-friendly name for the profile
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the profile's purpose or characteristics
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Maximum avoidance distance at full moon (100% illumination) in degrees
        /// </summary>
        [Range(0.0, 180.0)]
        public double FullMoonDistanceDegrees { get; set; }

        /// <summary>
        /// Width parameter controlling the curve shape in days
        /// This affects how quickly the avoidance distance drops off as the moon gets dimmer
        /// </summary>
        [Range(0.1, 60.0)]
        public double WidthInDays { get; set; }

        /// <summary>
        /// Minimum moon altitude in degrees below which moon avoidance is ignored
        /// If the moon is below this altitude, it's considered safe to observe regardless of phase/distance
        /// </summary>
        [Range(-90.0, 90.0)]
        public double MinMoonAltitudeDegrees { get; set; } = 0.0;

        /// <summary>
        /// Whether this is a system-provided default profile (cannot be deleted)
        /// </summary>
        public bool IsSystemDefault { get; set; }



        /// <summary>
        /// User ID who owns this profile (null for system defaults)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// When the profile was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the profile was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }
        
        /// <summary>
        /// For optimistic locking - client sends last known ModifiedAt when updating
        /// Server rejects if record was modified since this timestamp
        /// </summary>
        public DateTime? LastKnownModifiedAt { get; set; }

        /// <summary>
        /// Calculates the moon avoidance distance for a given moon illumination fraction
        /// using the Lorentzian algorithm
        /// </summary>
        /// <param name="illuminationFraction">Moon illumination fraction (0.0 to 1.0)</param>
        /// <returns>Required avoidance distance in degrees</returns>
        public double CalculateAvoidanceDistance(double illuminationFraction)
        {
            // Lorentzian function: f(x) = A / (1 + ((x - x0) / γ)^2)
            // Where:
            // - A = amplitude (FullMoonDistanceDegrees)
            // - x = illumination fraction (0 to 1)
            // - x0 = center point (1.0 for full moon)
            // - γ = width parameter (related to WidthInDays)
            
            // Convert width in days to appropriate gamma parameter
            // Assuming lunar cycle is ~29.5 days, so width in days / 29.5 gives fraction of cycle
            var gamma = WidthInDays / 29.5;
            
            // Center the Lorentzian at full moon (illumination = 1.0)
            var x0 = 1.0;
            
            // Calculate Lorentzian value
            var denominator = 1.0 + Math.Pow((illuminationFraction - x0) / gamma, 2);
            var avoidanceDistance = FullMoonDistanceDegrees / denominator;
            
            // Ensure minimum distance is never negative
            return Math.Max(0.0, avoidanceDistance);
        }


        /// <summary>
        /// Gets a collection of avoidance distances for different moon phases (0% to 100% in 5% steps)
        /// </summary>
        /// <returns>Dictionary with illumination percentage as key and avoidance distance as value</returns>
        public Dictionary<int, double> GetPhaseDistanceTable()
        {
            var table = new Dictionary<int, double>();
            
            for (int phase = 0; phase <= 100; phase += 5)
            {
                var illuminationFraction = phase / 100.0;
                var distance = CalculateAvoidanceDistance(illuminationFraction);
                table[phase] = distance;
            }
            
            return table;
        }
    }
}
