using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Shared.Model.DTO.Common;

namespace Shared.Model.DTO.Settings
{
    /// <summary>
    /// Data transfer object for Observatory entities.
    /// </summary>
    public class ObservatoryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the observatory.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID that owns this observatory.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the observatory.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the latitude of the observatory in decimal degrees.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the observatory in decimal degrees.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the display format for latitude (e.g., "45° 30' N").
        /// </summary>
        public string LatitudeDisplay { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display format for longitude (e.g., "73° 30' W").
        /// </summary>
        public string LongitudeDisplay { get; set; } = string.Empty;

        public double Elevation { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the timezone ID for the observatory in IANA format (e.g., "America/New_York").
        /// Always stored as IANA ID for cross-platform compatibility.
        /// </summary>
        public string Timezone { get; set; } = "UTC";

        /// <summary>
        /// Gets or sets the list of equipment assigned to this observatory.
        /// </summary>
        public List<EquipmentDto> AssignedEquipment { get; set; } = new();

        /// <summary>
        /// Gets or sets the minimum altitude for observations in degrees.
        /// </summary>
        public double MinAltitude { get; set; } = 20.0;

        /// <summary>
        /// Gets or sets the custom horizon points for the observatory.
        /// Each point defines the horizon altitude at a specific azimuth.
        /// </summary>
        public List<AltAzCoordDto> CustomHorizonPoints { get; set; } = new List<AltAzCoordDto>();

        /// <summary>
        /// Gets or sets a value indicating whether this is the default observatory for the user.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets any additional notes about the observatory.
        /// </summary>
        public string Notes { get; set; } = string.Empty;


        // not really needed but the FormItem needs it..
        public string CoordFormat { get; set; } = string.Empty;

        public TimeZoneInfo ObservatoryTimeZoneInfo 
        {
            get
            {
                try
                {
                    // Use TimeZoneConverter for cross-platform timezone resolution
                    return TimeZoneConverter.TZConvert.GetTimeZoneInfo(Timezone);
                }
                catch
                {
                    // Try direct lookup as fallback
                    try
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(Timezone);
                    }
                    catch
                    {
                        // Final fallback to UTC if timezone is not found
                        return TimeZoneInfo.Utc;
                    }
                }
            }
        }



        /// <summary>
        /// Checks if the observatory has a custom horizon defined.
        /// </summary>
        public bool HasCustomHorizon()
        {
            return CustomHorizonPoints?.Count > 0;
        }


        /// <summary>
        /// Converts a UTC DateTime to the observatory's local timezone.
        /// </summary>
        /// <param name="utcDateTime">The UTC DateTime to convert</param>
        /// <returns>DateTime converted to observatory's timezone, or UTC if timezone is invalid</returns>
        public DateTime ConvertUtcToObservatoryTime(DateTime utcDateTime)
        {
            try
            {
                // Single conversion from UTC to observatory timezone
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, ObservatoryTimeZoneInfo);
            }
            catch
            {
                // Fallback to UTC if timezone is invalid
                return utcDateTime;
            }
        }


        public DateTime ConvertObservatoryTimeToUtc(DateTime dateTime)
        {
            try
            {
                if (dateTime.Kind != DateTimeKind.Unspecified)
                {
                    dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Unspecified);
                }

                return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, ObservatoryTimeZoneInfo.Id, TimeZoneInfo.Utc.Id);
            }
            catch
            {
                // Fallback to UTC if timezone is invalid
                return dateTime;
            }
        }


        /// <summary>
        /// Gets the horizon altitude for a specific azimuth by interpolating the custom horizon data.
        /// </summary>
        /// <returns>Horizon altitude in degrees</returns>
        public double GetHorizonAltitudeForAzimuth(double azimuth)
        {
            if (!HasCustomHorizon())
            {
                return MinAltitude;
            }

            try
            {
                var horizonPoints = CustomHorizonPoints;
                if (horizonPoints == null || horizonPoints.Count == 0)
                {
                    return MinAltitude;
                }

                // Sort by azimuth to ensure proper interpolation
                var sortedPoints = horizonPoints.OrderBy(p => p.Azimuth).ToList();

                // Normalize azimuth to 0-360 range
                azimuth = azimuth % 360;
                if (azimuth < 0) azimuth += 360;

                // Find the two points to interpolate between
                for (int i = 0; i < sortedPoints.Count; i++)
                {
                    var currentPoint = sortedPoints[i];
                    var nextPoint = sortedPoints[(i + 1) % sortedPoints.Count];

                    // Handle wrap-around case (e.g., from 350° to 10°)
                    double currentAz = currentPoint.Azimuth;
                    double nextAz = nextPoint.Azimuth;

                    if (nextAz < currentAz) // Wrap around case
                    {
                        nextAz += 360;
                    }

                    // Check if azimuth is between current and next point
                    if (azimuth >= currentAz && azimuth <= nextAz)
                    {
                        // Linear interpolation
                        double ratio = (azimuth - currentAz) / (nextAz - currentAz);
                        var interpolatedAlt = currentPoint.Altitude + ratio * (nextPoint.Altitude - currentPoint.Altitude);
                        // Enforce MinAltitude as floor value
                        return Math.Max(interpolatedAlt, MinAltitude);
                    }

                    // Handle wrap-around case for azimuth > 360
                    if (azimuth + 360 >= currentAz && azimuth + 360 <= nextAz)
                    {
                        double ratio = ((azimuth + 360) - currentAz) / (nextAz - currentAz);
                        var interpolatedAlt = currentPoint.Altitude + ratio * (nextPoint.Altitude - currentPoint.Altitude);
                        // Enforce MinAltitude as floor value
                        return Math.Max(interpolatedAlt, MinAltitude);
                    }
                }

                // If no interpolation found, return the closest point (with MinAltitude floor)
                var closestPoint = sortedPoints.OrderBy(p => Math.Abs(p.Azimuth - azimuth)).First();
                return Math.Max(closestPoint.Altitude, MinAltitude);
            }
            catch
            {
                return MinAltitude;
            }
        }

        /// <summary>
        /// Gets the effective minimum altitude for the observatory, considering custom horizon if available.
        /// The effective minimum is always at least MinAltitude, even when custom horizon is set.
        /// </summary>
        /// <returns>Effective minimum altitude in degrees</returns>
        public double GetEffectiveMinimumAltitude()
        {
            try
            {
                // If observatory has custom horizon data, use the minimum of custom horizon points
                // but enforce MinAltitude as a floor value
                if (HasCustomHorizon() && CustomHorizonPoints?.Any() == true)
                {
                    var minHorizonAltitude = CustomHorizonPoints.Min(h => h.Altitude);
                    // Use the higher of: custom horizon minimum, MinAltitude, or 10.0 (absolute floor)
                    return Math.Max(Math.Max(minHorizonAltitude, MinAltitude), 10.0);
                }
                else
                {
                    // Use the observatory's configured minimum altitude or default
                    return Math.Max(MinAltitude, 10.0);
                }
            }
            catch
            {
                // Fail-safe: return default minimum altitude
                return Math.Max(MinAltitude, 10.0);
            }
        }
    }
}
