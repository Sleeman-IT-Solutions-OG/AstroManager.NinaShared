using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Common
{
    /// <summary>
    /// Data transfer object representing an azimuth-altitude coordinate pair for horizon definitions.
    /// </summary>
    public class AltAzCoordDto
    {
        /// <summary>
        /// Gets or sets the azimuth in degrees (0-360).
        /// 0° = North, 90° = East, 180° = South, 270° = West
        /// </summary>
        [Range(0, 360, ErrorMessage = "Azimuth must be between 0° and 360°")]
        public double Azimuth { get; set; }

        /// <summary>
        /// Gets or sets the altitude in degrees (0-90).
        /// 0° = Horizon, 90° = Zenith
        /// </summary>
        [Range(0, 90, ErrorMessage = "Altitude must be between 0° and 90°")]
        public double Altitude { get; set; }

        /// <summary>
        /// Initializes a new instance of the AltAzCoordDto class.
        /// </summary>
        public AltAzCoordDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the AltAzCoordDto class with specified coordinates.
        /// </summary>
        /// <param name="azimuth">The azimuth in degrees (0-360)</param>
        /// <param name="altitude">The altitude in degrees (0-90)</param>
        public AltAzCoordDto(double azimuth, double altitude)
        {
            Azimuth = azimuth;
            Altitude = altitude;
        }

        /// <summary>
        /// Returns a string representation of the coordinate.
        /// </summary>
        /// <returns>String in format "Az°/Alt°"</returns>
        public override string ToString()
        {
            return $"{Azimuth:F1}°/{Altitude:F1}°";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object? obj)
        {
            if (obj is AltAzCoordDto other)
            {
                return Math.Abs(Azimuth - other.Azimuth) < 0.001 && 
                       Math.Abs(Altitude - other.Altitude) < 0.001;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Azimuth, Altitude);
        }
    }
}
