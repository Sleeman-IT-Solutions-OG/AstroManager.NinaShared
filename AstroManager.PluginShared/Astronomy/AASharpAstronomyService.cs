using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AASharp;
using Microsoft.Extensions.Logging;
using Shared.Model.Common;
using Shared.Model.DTO.Astro;
using Shared.Model.DTO.Common;
using Shared.Model.DTO.Settings;
using Shared.Services.Astronomy.Interfaces;

namespace Shared.Services.Astronomy
{
    /// <summary>
    /// Implementation of IAstronomyService using AASharp library
    /// </summary>
    public class AASharpAstronomyService : IAstronomyService
    {
        private const double AstronomicalUnit = 149597870.691; // km
        private const double EarthRadius = 6371.0; // km
        private const double StandardPressure = 1010.0; // mbar
        private const double StandardTemperature = 15.0; // °C
        private const double StandardRefraction = 34.0 / 60.0; // degrees (standard atmospheric refraction at horizon)
        private const double StandardOzone = 0.35; // cm (standard ozone layer thickness)
        private const double StandardWaterVapor = 1.0; // cm (standard water vapor content)
        private const double StandardAerosolOpticalDepth = 0.1; // standard aerosol optical depth

        private const double MoonPhaseNormalizeDivisor = 180.0;
        private const double SynodicMonth = 29.530588853;  // Calculate the synodic month in days (average time between new moons)

        private const double SunH0 = -0.8333;  // degrees
        private const double MoonH0 = 0.125;   // degrees
        private const double MoonCalcStepInterval = 0.007;   // about 10 minutes

        // Civil twilight: Sun's center is 6 degrees below the horizon
        private const double CivilTwilightAltitude = -6.0;

        // Nautical twilight: Sun's center is 12 degrees below the horizon
        private const double NauticalTwilightAltitude = -12.0;

        // Astronomical twilight: Sun's center is 18 degrees below the horizon
        private const double AstronomicalTwilightAltitude = -18.0;



        private readonly ILogger<AASharpAstronomyService> _logger;

        // Performance caches for time-based calculations (thread-safe)
        private readonly ConcurrentDictionary<DateTime, double> _julianDateCache = new();
        private readonly ConcurrentDictionary<(DateTime time, double latitude, double longitude, double height), MoonPositionDto> _moonPositionCache = new();
        private readonly ConcurrentDictionary<(double azimuth, double latitude, double longitude, double elevation), double> _horizonAltitudeCache = new();
        // DSO position and moon distance caching with coordinate clustering
        public readonly ConcurrentDictionary<DsoPositionCacheKey, DsoPositionCacheValue> _dsoPositionCache = new();

        /// <summary>
        /// Cache key for DSO position and moon distance using rounded coordinates
        /// </summary>
        public struct DsoPositionCacheKey : IEquatable<DsoPositionCacheKey>
        {
            public double RoundedRA { get; set; }      // Round(ra, 1) - 0.1° precision
            public double RoundedDEC { get; set; }     // Round(dec, 1) - 0.1° precision
            public DateTime RoundedTime { get; set; }  // Round to 15-minute intervals
            public string ObservatoryKey { get; set; } // Lat/Lon rounded to 0.01°

            public bool Equals(DsoPositionCacheKey other) =>
                RoundedRA == other.RoundedRA &&
                RoundedDEC == other.RoundedDEC &&
                RoundedTime == other.RoundedTime &&
                ObservatoryKey == other.ObservatoryKey;

            public override bool Equals(object obj) => obj is DsoPositionCacheKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(RoundedRA, RoundedDEC, RoundedTime, ObservatoryKey);
        }

        /// <summary>
        /// Cached DSO position and moon distance data
        /// </summary>
        public class DsoPositionCacheValue
        {
            public double Altitude { get; set; }       // DSO altitude in degrees
            public double Azimuth { get; set; }        // DSO azimuth in degrees
            public double MoonDistance { get; set; }   // Angular distance to moon in degrees
            public DateTime CachedAt { get; set; }
            public TimeSpan ValidFor { get; set; } = GetCacheValidityPeriod();

            public bool IsValid => DateTime.UtcNow - CachedAt < ValidFor;

            private static TimeSpan GetCacheValidityPeriod()
            {
                // Longer cache validity for desktop/non-mobile platforms
                // Mobile platforms benefit from shorter cache to preserve memory
#if ANDROID || IOS
                return TimeSpan.FromHours(1);   // Mobile: 1 hour
#else
                return TimeSpan.FromHours(2);   // Desktop: 2 hours
#endif
            }
        }

        public AASharpAstronomyService(ILogger<AASharpAstronomyService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Converts Alt/Az coordinates to RA/Dec coordinates for a given observer location and time
        /// </summary>
        public async Task<(double Ra, double Dec)> ConvertAltAzToRaDecAsync(
            double altitude, double azimuth,
            double latitude, double longitude, DateTime dateTime)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Convert DateTime to Julian Day
                    var julianDay = GetJulianDateAsync(dateTime).Result;

                    // Calculate Local Sidereal Time
                    var lst = AASCoordinateTransformation.DegreesToHours(
                        AASSidereal.MeanGreenwichSiderealTime(julianDay) * 15.0 + longitude);

                    // Convert Alt/Az to Hour Angle and Declination
                    var equatorial = AASCoordinateTransformation.Horizontal2Equatorial(
                        azimuth + 180, altitude, latitude);

                    var hourAngle = equatorial.X;
                    var declination = equatorial.Y;

                    // Convert Hour Angle to Right Ascension
                    var rightAscension = lst - hourAngle;

                    // Normalize RA to 0-24 hours
                    while (rightAscension < 0) rightAscension += 24;
                    while (rightAscension >= 24) rightAscension -= 24;

                    // Convert to degrees
                    var raDegrees = rightAscension * 15.0; // 1 hour = 15 degrees

                    return (raDegrees, declination);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting Alt/Az to RA/Dec: Alt={Altitude}, Az={Azimuth}, Lat={Latitude}, Lon={Longitude}, DateTime={DateTime}",
                        altitude, azimuth, latitude, longitude, dateTime);
                    throw;
                }
            });
        }

        public async Task<object[]> GetHorizonLineAsync(ObservatoryDto observatory, DateTime dateTime)
        {
            try
            {
                List<AltAzCoordDto>? customHorizonPoints = observatory.CustomHorizonPoints;

                var latitude = observatory.Latitude;
                var longitude = observatory.Longitude;


                var localDate = new DateTime(dateTime.Ticks, DateTimeKind.Unspecified);

                localDate = observatory.ConvertObservatoryTimeToUtc(localDate);


                var horizonPoints = new List<object>();

                if (customHorizonPoints != null && customHorizonPoints.Any())
                {
                    // Sort custom points by azimuth for interpolation
                    var sortedPoints = customHorizonPoints.OrderBy(p => p.Azimuth).ToList();

                    // Generate 360 points with interpolated altitudes
                    for (int i = 0; i < 360; i++)
                    {
                        double azimuth = i;
                        double altitude = InterpolateAltitudeAtAzimuth(azimuth, sortedPoints);

                        var (ra, dec) = await ConvertAltAzToRaDecAsync(
                            altitude, azimuth, latitude, longitude, localDate);

                        horizonPoints.Add(new { ra = ra, dec = dec });
                    }
                }
                else
                {
                    // Generate default horizon line at 20 degrees altitude
                    const double defaultAltitude = 20.0;
                    const int numPoints = 360; // Every 1 degree for precise horizon line

                    for (int i = 0; i < numPoints; i++)
                    {
                        double azimuth = i * (360.0 / numPoints);
                        var (ra, dec) = await ConvertAltAzToRaDecAsync(
                            defaultAltitude, azimuth, latitude, longitude, localDate);

                        horizonPoints.Add(new { ra = ra, dec = dec });
                    }
                }

                return horizonPoints.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating horizon line for Lat={Latitude}, Lon={Longitude}, DateTime={DateTime}",
                    observatory.Latitude, observatory.Longitude, dateTime);

                // Return fallback horizon points
                return new object[]
                {
                    new { ra = 0.0, dec = 20.0 },
                    new { ra = 90.0, dec = 20.0 },
                    new { ra = 180.0, dec = 20.0 },
                    new { ra = 270.0, dec = 20.0 }
                };
            }
        }

        /// <summary>
        /// Interpolates altitude at a given azimuth using linear interpolation between custom horizon points
        /// Handles wraparound at 360/0 degrees boundary
        /// </summary>
        /// <param name="targetAzimuth">Target azimuth in degrees (0-359)</param>
        /// <param name="customPoints">Sorted list of custom horizon points by azimuth</param>
        /// <returns>Interpolated altitude in degrees</returns>
        private double InterpolateAltitudeAtAzimuth(double targetAzimuth, List<AltAzCoordDto> customPoints)
        {
            if (customPoints == null || !customPoints.Any())
                return 20.0; // Default altitude

            if (customPoints.Count == 1)
                return customPoints[0].Altitude;

            // Normalize target azimuth to 0-360 range
            targetAzimuth = targetAzimuth % 360.0;
            if (targetAzimuth < 0) targetAzimuth += 360.0;

            // Find the two points to interpolate between
            AltAzCoordDto? lowerPoint = null;
            AltAzCoordDto? upperPoint = null;

            for (int i = 0; i < customPoints.Count; i++)
            {
                var currentPoint = customPoints[i];
                var currentAz = currentPoint.Azimuth % 360.0;
                if (currentAz < 0) currentAz += 360.0;

                if (currentAz <= targetAzimuth)
                {
                    lowerPoint = currentPoint;
                }
                else if (upperPoint == null)
                {
                    upperPoint = currentPoint;
                    break;
                }
            }

            // Handle wraparound case (target is beyond last point or before first point)
            if (lowerPoint == null)
            {
                // Target is before first point, use last and first points
                lowerPoint = customPoints.Last();
                upperPoint = customPoints.First();
            }
            else if (upperPoint == null)
            {
                // Target is after last point, use last and first points
                upperPoint = customPoints.First();
            }

            // Perform linear interpolation
            var lowerAz = lowerPoint.Azimuth % 360.0;
            if (lowerAz < 0) lowerAz += 360.0;

            var upperAz = upperPoint.Azimuth % 360.0;
            if (upperAz < 0) upperAz += 360.0;

            // Handle wraparound in azimuth difference
            double azDiff;
            if (upperAz < lowerAz)
            {
                // Wraparound case
                if (targetAzimuth >= lowerAz)
                {
                    azDiff = (360.0 - lowerAz) + upperAz;
                    var targetOffset = targetAzimuth - lowerAz;
                    var ratio = targetOffset / azDiff;
                    return lowerPoint.Altitude + ratio * (upperPoint.Altitude - lowerPoint.Altitude);
                }
                else
                {
                    azDiff = (360.0 - lowerAz) + upperAz;
                    var targetOffset = (360.0 - lowerAz) + targetAzimuth;
                    var ratio = targetOffset / azDiff;
                    return lowerPoint.Altitude + ratio * (upperPoint.Altitude - lowerPoint.Altitude);
                }
            }
            else
            {
                // Normal case (no wraparound)
                azDiff = upperAz - lowerAz;
                if (azDiff == 0) return lowerPoint.Altitude;

                var ratio = (targetAzimuth - lowerAz) / azDiff;
                return lowerPoint.Altitude + ratio * (upperPoint.Altitude - lowerPoint.Altitude);
            }
        }

        #region Basic Position Calculations

        public async Task<(double Altitude, double Azimuth)> CalculateAltitudeAzimuthAsync(
            double rightAscension, double declination,
            double latitude, double longitude, DateTime dateTime)
        {
            try
            {
                var position = await GetCelestialPositionAsync(rightAscension, declination, latitude, longitude * -1, dateTime);
                return (position.Altitude, position.Azimuth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating altitude/azimuth");
                throw;
            }
        }

        public async Task<CelestialPositionDto> GetCelestialPositionAsync(
            double rightAscension /* in degrees */, double declination,
            double latitude, double longitude, DateTime dateTime)
        {
            try
            {
                double jd = await GetJulianDateAsync(dateTime);   // TODO Cache!

                double LocalHourAngle = await CalcLocalHourAngleAsync(jd, longitude, AASCoordinateTransformation.DegreesToHours(rightAscension));


                // Convert RA/Dec to Alt/Az
                var horizontal = AASCoordinateTransformation.Equatorial2Horizontal(LocalHourAngle, declination, latitude);

                /*
                AAS2DCoordinate horizontal = AASCoordinateTransformation.Equatorial2Horizontal(LocalHourAngle, rightAscension, latitude);
                horizontal.Y += AASRefraction.RefractionFromTrue(horizontal.Y, AASharpAstronomyService.StandardPressure, AASharpAstronomyService.StandardTemperature);
                */

                var azimuth = horizontal.X - 180;

                if (azimuth < 0)
                {
                    azimuth += 360;
                }

                return new CelestialPositionDto
                {
                    RightAscension = rightAscension,
                    Declination = declination,
                    Altitude = horizontal.Y,
                    Azimuth = azimuth,
                    Distance = 0, // Not calculated here
                    Magnitude = 0, // Not calculated here
                    IlluminatedFraction = 1, // Default to fully illuminated
                    AngularSize = null // Not calculated here
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating celestial position");
                throw;
            }
        }
        #endregion


        #region Twilight Calculations
        public async Task<AllTwilightTimesDto> GetTwilightTimesAsync(double latitude, double longitude, DateTime date)
        {
            try
            {
                double jd = await GetJulianDateAsync(date);
                double endJD = jd + 1; // Calculate for the next 24 hours


                // Single call to get all events
                var results = AASRiseTransitSet2.Calculate(jd, endJD, AASRiseSetObject.SUN, longitude * -1, latitude, SunH0, 0, 0.007, true);

                var twilightTimes = new AllTwilightTimesDto();

                foreach (var result in results)
                {
                    var eventTime = await FromJulianDateAsync(result.JD);

                    switch (result.type)
                    {
                        case AASRiseTransitSetDetails2.Type.StartCivilTwilight:
                            twilightTimes.Civil.Dawn = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.EndCivilTwilight:
                            twilightTimes.Civil.Dusk = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.StartNauticalTwilight:
                            twilightTimes.Nautical.Dawn = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.EndNauticalTwilight:
                            twilightTimes.Nautical.Dusk = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.StartAstronomicalTwilight:
                            twilightTimes.Astronomical.Dawn = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.EndAstronomicalTwilight:
                            twilightTimes.Astronomical.Dusk = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.Rise:
                            twilightTimes.Sunrise = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.Set:
                            twilightTimes.Sunset = eventTime;
                            break;
                        case AASRiseTransitSetDetails2.Type.SouthernTransit:
                        case AASRiseTransitSetDetails2.Type.NorthernTransit:
                            twilightTimes.SolarNoon = eventTime;
                            break;
                    }
                }

                return twilightTimes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating twilight times");
                return new AllTwilightTimesDto();
            }
        }
        #endregion


        #region Moon Calculations

        public async Task<MoonPositionDto> GetMoonPositionAsync(double latitude, double longitude, double height, DateTime dateTime)
        {
            try
            {
                var moonCacheKey = (
                    dateTime,
                    Math.Round(latitude, 4),
                    Math.Round(longitude, 4),
                    Math.Round(height, 0));

                // Check cache first
                if (_moonPositionCache.TryGetValue(moonCacheKey, out var cachedPosition))
                {
                    return cachedPosition;
                }

                // Convert to AASharp date format (Julian date)
                double jdMoon = GetCachedJulianDate(dateTime);

                // get moon long and lat
                double MoonLong = AASMoon.EclipticLongitude(jdMoon);
                double MoonLat = AASMoon.EclipticLatitude(jdMoon);

                AAS2DCoordinate moonEquatorial = AASCoordinateTransformation.Ecliptic2Equatorial(MoonLong, MoonLat, AASNutation.TrueObliquityOfEcliptic(jdMoon));

                double moonDistance = AASMoon.RadiusVector(jdMoon);
                moonDistance /= AstronomicalUnit; //Convert KM to AU

                AAS2DCoordinate MoonTopo = AASParallax.Equatorial2Topocentric(moonEquatorial.X, moonEquatorial.Y, moonDistance, longitude, latitude, height, jdMoon);
                double LocalHourAngle = await CalcLocalHourAngleAsync(jdMoon, longitude, MoonTopo.X);

                AAS2DCoordinate MoonHorizontal = AASCoordinateTransformation.Equatorial2Horizontal(LocalHourAngle, MoonTopo.Y, latitude);
                MoonHorizontal.Y += AASRefraction.RefractionFromTrue(MoonHorizontal.Y, StandardPressure, StandardTemperature);


                double MoonPhaseAngle = await GetMoonPhaseAsync(dateTime);
                double meanElongation = AASMoon.MeanElongation(jdMoon);
                double MoonIlluminatedFraction = AASMoonIlluminatedFraction.IlluminatedFraction(MoonPhaseAngle);

                var moonPosition = new MoonPositionDto
                {
                    RightAscension = moonEquatorial.X,
                    Declination = moonEquatorial.Y,
                    Altitude = MoonHorizontal.Y,
                    Azimuth = MoonHorizontal.X,
                    Phase = MoonPhaseAngle / MoonPhaseNormalizeDivisor, // Normalize to 0-1
                    Distance = moonDistance * AstronomicalUnit, // Convert AU to km
                    Elongation = meanElongation,
                    IlluminatedFraction = MoonIlluminatedFraction,
                    Age = 0 // not implemented
                };

                // Cache the result
                _moonPositionCache.TryAdd(moonCacheKey, moonPosition);
                return moonPosition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Moon position");
                throw;
            }
        }

        /// <summary>
        /// Gets cached Julian Date or calculates and caches it
        /// </summary>
        private double GetCachedJulianDate(DateTime dateTime)
        {
            // Ensure we're working with UTC time for astronomical calculations
            DateTime utcDateTime = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : dateTime.Kind == DateTimeKind.Local
                    ? dateTime.ToUniversalTime()
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

            if (!_julianDateCache.TryGetValue(utcDateTime, out var jd))
            {
                AASDate dateAS = new AASDate(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day, utcDateTime.Hour, utcDateTime.Minute, utcDateTime.Second, true);
                jd = dateAS.Julian;
                _julianDateCache.TryAdd(utcDateTime, jd);
            }
            return jd;
        }

        /// <summary>
        /// Gets cached horizon altitude or calculates and caches it
        /// </summary>
        private double GetCachedHorizonAltitude(double azimuth, double latitude, double longitude, double elevation, ObservatoryDto observatory)
        {
            var key = (Math.Round(azimuth, 1), latitude, longitude, elevation); // Round azimuth to 0.1 degree precision

            if (!_horizonAltitudeCache.TryGetValue(key, out var altitude))
            {
                altitude = observatory.GetHorizonAltitudeForAzimuth(azimuth);
                _horizonAltitudeCache.TryAdd(key, altitude);
            }

            return altitude;
        }

        /// <summary>
        /// Clears performance caches - call when starting new calculation session
        /// </summary>
        public void ClearCaches()
        {
            _julianDateCache.Clear();
            _moonPositionCache.Clear();
            _horizonAltitudeCache.Clear();
            _dsoPositionCache.Clear();
        }

        public async Task<double> GetMoonPhaseAsync(DateTime dateTime)
        {
            try
            {
                double jd = GetCachedJulianDate(dateTime);

                double sunDistance = await GetSunDistanceInKMAsync(dateTime);

                double moonDistance = AASMoon.RadiusVector(jd);
                moonDistance /= AstronomicalUnit; //Convert KM to AU

                double MoonLong = AASMoon.EclipticLongitude(jd);
                double MoonLat = AASMoon.EclipticLatitude(jd);
                AAS2DCoordinate moonEquatorial = AASCoordinateTransformation.Ecliptic2Equatorial(MoonLong, MoonLat, AASNutation.TrueObliquityOfEcliptic(jd));

                double sunLat = AASSun.GeometricEclipticLatitude(jd, true);
                double sunLong = AASSun.GeometricEclipticLongitude(jd, true);
                var sunEquatorial = AASCoordinateTransformation.Ecliptic2Equatorial(sunLong, sunLat, AASNutation.TrueObliquityOfEcliptic(jd));


                double MoonGeocentricElongation = AASMoonIlluminatedFraction.GeocentricElongation(moonEquatorial.X, moonEquatorial.Y, sunEquatorial.X, sunEquatorial.Y);
                double MoonPhaseAngle = AASMoonIlluminatedFraction.PhaseAngle(MoonGeocentricElongation, moonDistance, sunDistance);

                return MoonPhaseAngle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Moon phase");
                throw;
            }
        }

        private async Task<double> GetMoonPhaseNormalizedAsync(DateTime dateTime)
        {
            try
            {
                return await GetMoonPhaseAsync(dateTime) / MoonPhaseNormalizeDivisor; // Normalize to 0-1 range
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Moon phase");
                throw;
            }
        }


        private async Task<double> GetSunDistanceInKMAsync(DateTime dateTime)
        {
            double jd = await GetJulianDateAsync(dateTime);
            double sunDistance = AASEarth.RadiusVector(jd, false);

            return sunDistance;
        }

        public async Task<DateTime> GetNextMoonPhaseAsync(DateTime fromDate, double targetPhase = 0)
        {
            try
            {
                // Convert target phase to AASharp phase angle (0-180°)
                double targetPhaseAngle = targetPhase * MoonPhaseNormalizeDivisor;

                // Get the current Julian date
                double jd = await GetJulianDateAsync(fromDate);

                // Get the current Moon phase
                double currentPhase = await GetMoonPhaseAsync(fromDate);

                // Estimate the next occurrence of the target phase
                double phaseDiff = (targetPhaseAngle - currentPhase) % MoonPhaseNormalizeDivisor;
                if (phaseDiff < 0) phaseDiff += MoonPhaseNormalizeDivisor;

                // Convert phase difference to days
                double daysToPhase = phaseDiff / MoonPhaseNormalizeDivisor * SynodicMonth;

                // Add a small buffer to ensure we're past the current time
                if (daysToPhase < 0.1) daysToPhase += SynodicMonth;

                // Calculate the Julian date of the next phase
                double jdTarget = jd + daysToPhase;

                // Refine the calculation using AASharp's more accurate method
                double jdPhase = AASMoonPhases.TruePhase(jdTarget);

                // Convert Julian date back to DateTime
                var calendarDate = await FromJulianDateAsync(jdPhase);

                return calendarDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next Moon phase");
                throw;
            }
        }

        public async Task<Dictionary<DateTime, string>> GetLunarCalendar(DateTime month)
        {
            var calendar = new Dictionary<DateTime, string>();

            try
            {
                DateTime startDate = new DateTime(month.Year, month.Month, 1);
                DateTime endDate = startDate.AddMonths(1);

                for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
                {
                    double phase = await GetMoonPhaseAsync(date);
                    string phaseName = phase switch
                    {
                        < 0.03 or > 0.97 => "New Moon",
                        < 0.22 => "Waxing Crescent",
                        < 0.28 => "First Quarter",
                        < 0.47 => "Waxing Gibbous",
                        < 0.53 => "Full Moon",
                        < 0.72 => "Waning Gibbous",
                        < 0.78 => "Last Quarter",
                        _ => "Waning Crescent"
                    };

                    calendar[date] = phaseName;
                }

                return calendar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating lunar calendar");
                return calendar;
            }
        }

        public async Task<IEnumerable<(DateTime Date, double MoonPhase)>> GetMoonPhasesForMonth(DateTime month)
        {
            var phases = new List<(DateTime, double)>();

            try
            {
                DateTime startDate = new DateTime(month.Year, month.Month, 1);
                DateTime endDate = startDate.AddMonths(1);

                // Check for major phases in this month
                double jd = await GetJulianDateAsync(month);

                double newMoonJD = AASMoonPhases.TruePhase(jd);

                // Add phases for each day of the month
                for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
                {
                    double phase = await GetMoonPhaseAsync(date);
                    phases.Add((date, phase));
                }

                return phases;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting moon phases for month");
                return phases;
            }
        }



        public async Task<RiseTransitSetTimesDto> GetMoonRiseTransitSetTimesAsyncV2(double latitude, double longitude, DateTime date)
        {
            try
            {
                // Convert date to Julian Day
                double jd = await GetJulianDateAsync(date);
                double endJD = jd + 1; // Calculate for the next 24 hours

                // Use a smaller step interval for better accuracy (e.g., 0.1 hours = 6 minutes)
                double stepInterval = 0.1; // hours
                double stepInDays = stepInterval / 24.0;

                // Calculate moon positions with smaller steps
                var results = AASRiseTransitSet2.CalculateMoon(jd, endJD, longitude * -1, latitude,
                    Height: 0, // Use default height
                    StepInterval: stepInDays); // Use smaller steps

                // Find all events and sort by JD
                var events = results
                    .Where(r => r.type == AASRiseTransitSetDetails2.Type.Rise ||
                               r.type == AASRiseTransitSetDetails2.Type.SouthernTransit ||
                               r.type == AASRiseTransitSetDetails2.Type.Set)
                    .OrderBy(r => r.JD)
                    .ToList();

                // Find the first rise, transit, and set after the start time
                var riseEvent = events.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.Rise && r.JD >= jd);
                var transitEvent = events.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.SouthernTransit && r.JD >= jd);
                var setEvent = events.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.Set && r.JD >= jd);

                // Check for polar day/night conditions
                bool isPolarDay = events.Any(r => r.type == AASRiseTransitSetDetails2.Type.NorthernTransit ||
                                                 r.type == AASRiseTransitSetDetails2.Type.SouthernTransit);

                // Check if the moon is always above or below the horizon
                bool alwaysAbove = !events.Any(r => r.type == AASRiseTransitSetDetails2.Type.Rise ||
                                                  r.type == AASRiseTransitSetDetails2.Type.Set);
                bool alwaysBelow = alwaysAbove && events.Any() &&
                                  events.All(r => !r.bAboveHorizon);

                /*
                // If no rise/set events, check moon's position at start and end
                if (riseEvent != null && setEvent != null && !riseEvent && !setEvent)
                {
                    double altitudeStart = AASMoon.Altitude(jd, longitude, latitude);
                    double altitudeEnd = AASMoon.Altitude(endJD, longitude, latitude);

                    if (altitudeStart > 0 && altitudeEnd > 0)
                        alwaysAbove = true;
                    else if (altitudeStart <= 0 && altitudeEnd <= 0)
                        alwaysBelow = true;
                }*/

                return new RiseTransitSetTimesDto
                {
                    Rise = riseEvent != null ? await FromJulianDateAsync(riseEvent.JD) : null,
                    Transit = transitEvent != null ? await FromJulianDateAsync(transitEvent.JD) : null,
                    Set = setEvent != null ? await FromJulianDateAsync(setEvent.JD) : null,
                    IsCircumpolar = alwaysAbove,
                    NeverRises = alwaysBelow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating moon rise/transit/set times");
                return new RiseTransitSetTimesDto();
            }
        }

        public async Task<RiseTransitSetTimesDto> GetMoonRiseTransitSetTimesAsync(double latitude, double longitude, DateTime date)
        {
            try
            {
                double jd = await GetJulianDateAsync(date);
                double endJD = jd + 1; // Calculate for the next 24 hours


                // Calculate moon rise, transit, and set times using the new AASRiseTransitSet2
                var results = AASRiseTransitSet2.CalculateMoon(jd, endJD, longitude, latitude, 0, MoonCalcStepInterval);

                // Find the relevant events
                var riseEvent = results.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.Rise);
                var transitEvent = results.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.SouthernTransit);
                var setEvent = results.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.Set);

                // Check for polar day/night conditions
                bool isPolarDay = results.Any(r => r.type == AASRiseTransitSetDetails2.Type.NorthernTransit ||
                                                 r.type == AASRiseTransitSetDetails2.Type.SouthernTransit);


                bool isPolarNight = riseEvent != null && setEvent != null ? !riseEvent.bAboveHorizon && !setEvent.bAboveHorizon : false;

                return new RiseTransitSetTimesDto
                {
                    Rise = riseEvent != null ? await FromJulianDateAsync(riseEvent.JD) : null,
                    Transit = transitEvent != null ? await FromJulianDateAsync(transitEvent.JD) : null,
                    Set = setEvent != null ? await FromJulianDateAsync(setEvent.JD) : null,
                    IsCircumpolar = isPolarDay,
                    NeverRises = isPolarNight
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating moon rise/transit/set times");
                return new RiseTransitSetTimesDto();
            }
        }

        #endregion



        #region Sun Calculations

        public async Task<(double Ra, double Dec)> GetSunPositionAsync(DateTime dateTime)
        {
            try
            {
                double jd = await GetJulianDateAsync(dateTime);

                // Get Sun's ecliptic coordinates
                double sunLong = AASSun.GeometricEclipticLongitude(jd, false);
                double sunLat = AASSun.GeometricEclipticLatitude(jd, false);

                // Convert to equatorial coordinates
                var sunEquatorial = AASCoordinateTransformation.Ecliptic2Equatorial(
                    sunLong, // Long (Lambda)
                    sunLat,  // Lat (Beta)
                    AASNutation.TrueObliquityOfEcliptic(jd));

                return (sunEquatorial.X, sunEquatorial.Y);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Sun position");
                throw;
            }
        }

        public async Task<DateTime> GetSunriseAsync(double latitude, double longitude, DateTime date)
        {
            try
            {
                var times = await GetSunRiseTransitSetTimesAsync(latitude, longitude, date);
                if (times.Rise == null)
                    throw new InvalidOperationException("Sun does not rise on this date at this location");

                return times.Rise.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating sunrise");
                throw;
            }
        }


        public async Task<DateTime> GetSunsetAsync(double latitude, double longitude, DateTime date)
        {
            try
            {
                var times = await GetSunRiseTransitSetTimesAsync(latitude, longitude, date);
                if (times.Set == null)
                    throw new InvalidOperationException("Sun does not set on this date at this location");

                return times.Set.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating sunset");
                throw;
            }
        }

        public async Task<TimeSpan> GetDaylightDurationAsync(double latitude, DateTime date)
        {
            try
            {
                // For locations near the poles, we need to handle polar day/night
                var times = await GetSunRiseTransitSetTimesAsync(latitude, 0, date);

                if (times.NeverRises)
                    return TimeSpan.Zero;

                if (times.Rise == null || times.Set == null)
                    return TimeSpan.FromHours(24); // Polar day

                return times.Set.Value - times.Rise.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating daylight duration");
                throw;
            }
        }


        public async Task<RiseTransitSetTimesDto> GetSunRiseTransitSetTimesAsync(double latitude, double longitude, DateTime date)
        {
            try
            {
                double jd = await GetJulianDateAsync(date);
                double endJD = jd + 1; // Calculate for the next 24 hours


                // Calculate sun rise, transit, and set times using the new AASRiseTransitSet2
                var results = AASRiseTransitSet2.Calculate(jd, endJD, AASRiseSetObject.SUN, longitude, latitude, SunH0);

                // Find the relevant events
                var riseEvent = results.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.Rise);
                var transitEvent = results.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.SouthernTransit);
                var setEvent = results.FirstOrDefault(r => r.type == AASRiseTransitSetDetails2.Type.Set);

                // Check for polar day/night conditions
                bool isPolarDay = results.Any(r => r.type == AASRiseTransitSetDetails2.Type.NorthernTransit ||
                                                 r.type == AASRiseTransitSetDetails2.Type.SouthernTransit);
                bool isPolarNight = !riseEvent.bAboveHorizon && !setEvent.bAboveHorizon;


                return new RiseTransitSetTimesDto
                {
                    Rise = riseEvent != null && riseEvent.bAboveHorizon ? await FromJulianDateAsync(riseEvent.JD) : null,
                    Transit = transitEvent != null ? await FromJulianDateAsync(transitEvent.JD) : null,
                    Set = setEvent != null && setEvent.bAboveHorizon ? await FromJulianDateAsync(setEvent.JD) : null,
                    IsCircumpolar = isPolarDay,
                    NeverRises = isPolarNight
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating sun rise/transit/set times");
                return new RiseTransitSetTimesDto();
            }
        }
        #endregion

        public async Task<double> CalculateAngularDistanceAsync(
            double ra1 /* in degrees */, double dec1, double ra2 /* in hours */, double dec2)
        {
            try
            {
                // Calculate angular separation
                double distance = AASAngularSeparation.Separation(AASCoordinateTransformation.DegreesToHours(ra1), dec1, ra2, dec2);

                return distance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating angular distance");
                throw;
            }
        }


        public async Task<DateTime?> CalculateNextOppositionDateAsync(
    double rightAscension, double declination, DateTime fromDate)
        {
            try
            {
                // Convert RA from degrees to hours
                double raHours = rightAscension / 15.0;

                // Find when the Sun is at (RA - 12h) to put the object on the meridian at midnight
                double targetSunRA = AASCoordinateTransformation.MapTo0To24Range(raHours - 12.0);

                // Start from the input date
                double jdCurrent = await GetJulianDateAsync(fromDate);

                // Find the next date when Sun is at targetSunRA
                double jdBestView = await FindNextSunRA(jdCurrent, targetSunRA);

                return await FromJulianDateAsync(jdBestView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating best viewing date");
                return null;
            }
        }

        private async Task<double> FindNextSunRA(double startJD, double targetRA)
        {
            double jd = startJD;
            double lastRA = GetSunPositionAsync(await FromJulianDateAsync(jd)).Result.Ra;
            double lastDiff = AASCoordinateTransformation.MapTo0To24Range(lastRA - targetRA);

            // Move forward in time until we find when Sun's RA matches targetRA
            while (true)
            {
                jd += 1.0; // Move forward one day

                double currentRA = GetSunPositionAsync(await FromJulianDateAsync(jd)).Result.Ra;
                double currentDiff = AASCoordinateTransformation.MapTo0To24Range(currentRA - targetRA);

                // Check if we've passed the target RA
                if (lastDiff > 23.0 && currentDiff < 1.0 ||  // Crossed 24h boundary
                    lastDiff > 0 && currentDiff <= 0)        // Crossed target RA
                {
                    // Do a binary search to find the exact moment
                    double low = jd - 1.0;
                    double high = jd;

                    for (int i = 0; i < 10; i++) // 10 iterations gives precision of ~0.001 days
                    {
                        double mid = (low + high) / 2.0;
                        double midRA = GetSunPositionAsync(await FromJulianDateAsync(mid)).Result.Ra;
                        double diff = AASCoordinateTransformation.MapTo0To24Range(midRA - targetRA);

                        if (diff > 12.0) // Handle 24h boundary
                        {
                            diff -= 24.0;
                        }

                        if (diff > 0)
                        {
                            high = mid;
                        }
                        else
                        {
                            low = mid;
                        }
                    }

                    return (low + high) / 2.0;
                }

                lastRA = currentRA;
                lastDiff = currentDiff;
            }
        }


        public async Task<DateTime?> CalculateNextConjunctionDateAsync(
            double ra1, double dec1, double ra2, double dec2, DateTime fromDate)
        {
            try
            {
                // Convert input date to Julian date
                double jdStart = await GetJulianDateAsync(fromDate);

                // Step size for searching (in days)
                double step = 1.0;
                double currentJd = jdStart;

                // Maximum number of iterations to prevent infinite loops
                const int maxIterations = 1000;
                int iterations = 0;

                // Search for the next conjunction
                while (iterations++ < maxIterations)
                {
                    // Get positions of both objects at current JD
                    var pos1 = await GetCelestialPositionAsync(ra1, dec1, 0, 0, await FromJulianDateAsync(currentJd));
                    var pos2 = await GetCelestialPositionAsync(ra2, dec2, 0, 0, await FromJulianDateAsync(currentJd));

                    // Calculate angular separation
                    double separation = await CalculateAngularDistanceAsync(
                        pos1.RightAscension, pos1.Declination,
                        pos2.RightAscension, pos2.Declination);

                    // If separation is small enough, we've found a conjunction
                    if (separation < 1.0) // 1 degree threshold for conjunction
                    {
                        return await FromJulianDateAsync(currentJd);
                    }

                    // Move forward in time
                    currentJd += step;

                    // If we've gone too far without finding a conjunction, return null
                    if (currentJd > jdStart + 365.25 * 10) // 10 year limit
                    {
                        return null;
                    }
                }

                return null; // No conjunction found within max iterations
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next conjunction date");
                return null;
            }
        }

        #region Time and Calendar Methods
        public async Task<double> GetJulianDateAsync(DateTime dateTime)
        {
            // Ensure we're working with UTC time for astronomical calculations
            // If the input is Local or Unspecified, convert to UTC
            DateTime utcDateTime = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : dateTime.Kind == DateTimeKind.Local
                    ? dateTime.ToUniversalTime()
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc); // Assume Unspecified is UTC

            AASDate dateAS = new AASDate(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day, utcDateTime.Hour, utcDateTime.Minute, utcDateTime.Second, true);
            double jd = dateAS.Julian;

            return jd;
        }


        public async Task<DateTime> FromJulianDateAsync(double jd)
        {
            long Year = 0, Month = 0, Day = 0, Hours = 0, Minutes = 0;
            double Sec = 0;

            AASDate date = new AASDate(jd, true);
            date.Get(ref Year, ref Month, ref Day, ref Hours, ref Minutes, ref Sec);

            int iSec = (int)Sec;

            // Julian dates are in UTC, so specify DateTimeKind.Utc to ensure proper timezone handling
            DateTime dateTime = new DateTime(
                (int)Year, (int)Month, (int)Day,
                (int)Hours, (int)Minutes, iSec,
                DateTimeKind.Utc);

            return dateTime;
        }

        private async Task<double> CalcLocalHourAngleAsync(double jd, double longitude, double rightAscensionHourAngle)
        {
            double AST = AASSidereal.ApparentGreenwichSiderealTime(jd);
            double LongtitudeAsHourAngle = AASCoordinateTransformation.DegreesToHours(longitude);
            double LocalHourAngle = AST - LongtitudeAsHourAngle - rightAscensionHourAngle;

            return LocalHourAngle;
        }


        public async Task<double> GetSiderealTime(double longitude, DateTime dateTime)
        {
            try
            {
                double jd = await GetJulianDateAsync(dateTime);

                // Get Greenwich Apparent Sidereal Time (GAST) in degrees
                double gast = AASSidereal.ApparentGreenwichSiderealTime(jd);

                // Convert to hours and add longitude correction
                double lst = AASCoordinateTransformation.DegreesToHours(gast) + longitude / 15.0;

                // Normalize to 0-24 hours
                return AASCoordinateTransformation.MapTo0To24Range(lst);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating sidereal time");
                throw;
            }
        }
        #endregion

        #region Astronomy Utilities

        public async Task<double> CalculatePositionAngle(double ra1, double dec1, double ra2, double dec2)
        {
            try
            {
                // Convert to radians
                double ra1Rad = AASCoordinateTransformation.DegreesToRadians(ra1 * 15); // Convert hours to degrees
                double dec1Rad = AASCoordinateTransformation.DegreesToRadians(dec1);
                double ra2Rad = AASCoordinateTransformation.DegreesToRadians(ra2 * 15);
                double dec2Rad = AASCoordinateTransformation.DegreesToRadians(dec2);

                // Calculate position angle (in radians)
                double y = Math.Sin(ra2Rad - ra1Rad) * Math.Cos(dec2Rad);
                double x = Math.Cos(dec1Rad) * Math.Sin(dec2Rad) -
                          Math.Sin(dec1Rad) * Math.Cos(dec2Rad) * Math.Cos(ra2Rad - ra1Rad);

                double pa = Math.Atan2(y, x);

                // Convert to degrees and normalize to 0-360
                return AASCoordinateTransformation.MapTo0To360Range(AASCoordinateTransformation.RadiansToDegrees(pa));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating position angle");
                throw;
            }
        }

        public async Task<double> GetAirMass(double altitudeDegrees)
        {
            try
            {
                if (altitudeDegrees <= 0)
                    return double.PositiveInfinity;

                double z = 90 - altitudeDegrees; // Zenith angle in degrees
                double zRad = AASCoordinateTransformation.DegreesToRadians(z);

                // Simple plane-parallel approximation
                double airmass = 1.0 / Math.Cos(zRad);

                // Better approximation that accounts for Earth's curvature
                airmass = 1.0 / (Math.Cos(zRad) + 0.025 * Math.Exp(-11 * Math.Cos(zRad)));

                return airmass;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating air mass");
                throw;
            }
        }

        public async Task<double> GetAtmosphericRefraction(double apparentAltitude, double pressure = 1010, double temperature = 15)
        {
            try
            {
                // Convert to radians
                double h = AASCoordinateTransformation.DegreesToRadians(apparentAltitude);

                // Bennett's formula (simplified)
                double p = pressure / 1010.0; // Normalize pressure
                double t = 283.0 / (273.0 + temperature); // Temperature correction
                double r = 1.02 / Math.Tan(AASCoordinateTransformation.DegreesToRadians(apparentAltitude + 10.3 / (apparentAltitude + 5.11))) * p * t;

                // Convert to degrees
                return r / 60.0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating atmospheric refraction");
                throw;
            }
        }

        public async Task<double> GetLimitingMagnitude(
            double telescopeAperture, double fRatio, double pixelSize,
            double exposureTime, double skyBrightness)
        {
            try
            {
                // Simplified formula for estimating limiting magnitude
                double apertureArea = Math.PI * Math.Pow(telescopeAperture / 2.0, 2);
                double pixelScale = 206.265 * pixelSize / (telescopeAperture * fRatio);

                // This is a very rough estimate
                double lm = 2.0 + 5.0 * Math.Log10(apertureArea) +
                            2.5 * Math.Log10(exposureTime / 60.0) -
                            0.5 * skyBrightness;

                return lm;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating limiting magnitude");
                throw;
            }
        }

        #endregion


        #region Celestial Body Calculations
        public async Task<DateTime?> CalculateNextTransitDateAsync(double targetRa, double observerLongitude, DateTime fromDate)
        {
            try
            {
                // Convert fromDate to Julian date
                double jd = await GetJulianDateAsync(fromDate);

                // Get GMST0 (Greenwich Mean Sidereal Time at 0h UT)
                double gmst0 = AASSidereal.ApparentGreenwichSiderealTime(jd - AASDynamicalTime.DeltaT(jd) / 86400.0);

                // Calculate local sidereal time at 0h UT
                double lst0 = AASCoordinateTransformation.DegreesToHours(gmst0) + observerLongitude / 15.0;

                // Normalize to 0-24 hours
                lst0 = AASCoordinateTransformation.MapTo0To24Range(lst0);

                // Calculate transit time (when RA = LST)
                double transitTime = (targetRa - lst0) / 24.0;

                // If transit is in the past, add 1 day
                if (transitTime < 0)
                    transitTime += 1.0;

                // Convert to DateTime
                return fromDate.AddDays(transitTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next transit date");
                return null;
            }
        }

        public async Task<IEnumerable<(string Name, DateTime BestTime)>> GetBestObservingTimes(
            double latitude, double longitude, DateTime startDate, int daysAhead)
        {
            var result = new List<(string, DateTime)>();

            try
            {
                // For each day in the range
                for (int i = 0; i <= daysAhead; i++)
                {
                    DateTime currentDate = startDate.AddDays(i);

                    // Get sunset and sunrise times
                    var sunTimes = await GetSunRiseTransitSetTimesAsync(latitude, longitude, currentDate);

                    // Best observing is typically around astronomical twilight
                    // which is when the sun is 18° below the horizon
                    DateTime eveningTwilight = sunTimes.Set?.AddHours(1.5) ?? currentDate.Date.AddHours(22);
                    DateTime morningTwilight = sunTimes.Rise?.AddHours(-1.5) ?? currentDate.Date.AddDays(1).AddHours(4);

                    if (eveningTwilight < currentDate) eveningTwilight = currentDate;
                    if (morningTwilight > currentDate.AddDays(1)) morningTwilight = currentDate.AddDays(1);

                    // Add both evening and morning slots
                    result.Add(($"Evening {currentDate:ddd}", eveningTwilight));
                    result.Add(($"Morning {currentDate:ddd}", morningTwilight));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating best observing times");
                return result;
            }
        }

        #endregion

        #region Helper Methods

        private MoonPhase GetMoonPhaseEnum(double phase)
        {
            // Convert phase (0.0 to 1.0) to enum
            return phase switch
            {
                >= 0.0 and < 0.125 => MoonPhase.NewMoon,
                >= 0.125 and < 0.375 => MoonPhase.WaxingCrescent,
                >= 0.375 and < 0.625 => MoonPhase.FirstQuarter,
                >= 0.625 and < 0.875 => MoonPhase.WaxingGibbous,
                >= 0.875 and <= 1.0 => MoonPhase.FullMoon,
                _ => MoonPhase.NewMoon
            };
        }

        #endregion

        #region Batch Chart Data

        public async Task<AstronomicalData> GetAstronomicalDataAsync(ObservatoryDto observatory, DateTime selectedDate)
        {
            try
            {
                // Determine if it's before or after 8:00 AM in the observatory's timezone
                var isBeforeNoon = selectedDate.Hour < 8;

                // Set the localDate based on the conditions
                var localDate = isBeforeNoon
                    ? selectedDate.AddDays(-1).Date.AddHours(16)  // 16:00 PM of the previous day
                    : selectedDate.Date.AddHours(12);             // 12:00 PM of the current day

                /*
                if (localDate.Date != DateTime.Today)
                {
                    localDate = selectedDate.Date.AddHours(16);
                }
                */

                // localDate = selectedDate.Date.AddHours(16);

                /* we need to convert the start time for the check from observatory time to UTC */
                localDate = observatory.ConvertObservatoryTimeToUtc(localDate);

                // Get astronomical data in parallel (these don't use DbContext)
                var moonTimesTask = GetMoonRiseTransitSetTimesAsync(
                    observatory.Latitude,
                    observatory.Longitude * -1,
                    localDate.AddHours(10));

                var dateMoonPhase = selectedDate.Date.AddHours(23);

                var moonPositionTask = GetMoonPositionAsync(
                    observatory.Latitude,
                    observatory.Longitude * -1,
                    0, // height above sea level
                    dateMoonPhase);

                var twilightTimesTask = GetTwilightTimesAsync(
                    observatory.Latitude,
                    observatory.Longitude,
                    localDate);

                // Wait for astronomical calculations to complete
                await Task.WhenAll(moonTimesTask, moonPositionTask, twilightTimesTask);

                // Get results from astronomical calculations
                var moonTimes = await moonTimesTask;
                var moonPosition = await moonPositionTask;
                var twilightTimes = await twilightTimesTask;


                // Create astronomical data
                var astroData = new AstronomicalData
                {
                    Date = selectedDate,
                    DateMoonPhase = dateMoonPhase,
                    ObservatoryName = observatory.Name,
                    MoonPhase = GetMoonPhaseEnum(moonPosition.Phase),
                    MoonRise = moonTimes.Rise,
                    MoonSet = moonTimes.Set,
                    MoonIlluminatedFraction = moonPosition.IlluminatedFraction,
                    CivilBegin = twilightTimes.Civil.Dawn,
                    CivilEnd = twilightTimes.Civil.Dusk,
                    NauticalBegin = twilightTimes.Nautical.Dawn,
                    NauticalEnd = twilightTimes.Nautical.Dusk,
                    AstronomicalBegin = twilightTimes.Astronomical.Dawn,
                    AstronomicalEnd = twilightTimes.Astronomical.Dusk
                };

                return astroData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consolidated astronomical data with moon avoidance");
                throw;
            }
        }

        private double CalculateLorentzianAvoidanceDistance(double illuminationFraction, double fullMoonDistanceDegrees, double widthInDays)
        {
            // Convert illumination fraction (0.0 to 1.0) to phase angle in radians
            // Phase angle 0 = new moon, π = full moon
            var phaseAngle = Math.Acos(2 * illuminationFraction - 1);

            // Convert phase angle to days from new moon (0 to ~14.77 days)
            var daysFromNewMoon = phaseAngle * 14.77 / Math.PI;

            // Lorentzian function: y = A / (1 + ((x - x0) / w)^2)
            // Where:
            // - A = fullMoonDistanceDegrees (amplitude)
            // - x = daysFromNewMoon
            // - x0 = 14.77 (center at full moon)
            // - w = widthInDays (width parameter)

            var x = daysFromNewMoon;
            var x0 = 14.77; // Full moon occurs at ~14.77 days
            var w = widthInDays;
            var A = fullMoonDistanceDegrees;

            var distance = A / (1 + Math.Pow((x - x0) / w, 2));

            return Math.Max(0, distance); // Ensure non-negative
        }

        public async Task<BatchChartDataResponseDto> GetBatchChartDataAsync(BatchChartDataRequestDto request)
        {
            try
            {
                var response = new BatchChartDataResponseDto();
                var currentTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Unspecified);
                var endTime = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Unspecified);

                while (currentTime <= endTime)
                {
                    var currentTimeUtc = request.Observatory.ConvertObservatoryTimeToUtc(currentTime);

                    // Calculate DSO celestial position
                    var dsoPosition = await GetCelestialPositionAsync(
                        (double)request.RightAscension,
                        (double)request.Declination,
                        request.Observatory.Latitude,
                        request.Observatory.Longitude * -1,
                        currentTimeUtc);

                    // Calculate moon position
                    var moonPosition = await GetMoonPositionAsync(
                        request.Observatory.Latitude,
                        request.Observatory.Longitude * -1,
                        request.Observatory.Elevation, // Use observatory elevation
                        currentTimeUtc);

                    // Calculate angular distance between DSO and moon
                    var angularDistance = await CalculateAngularDistanceAsync(
                        (double)request.RightAscension,
                        (double)request.Declination,
                        moonPosition.RightAscension,
                        moonPosition.Declination);

                    // Add data point
                    response.DataPoints.Add(new ChartDataPointDto
                    {
                        Time = DateTime.SpecifyKind(currentTime, DateTimeKind.Unspecified),
                        DsoPosition = dsoPosition,
                        MoonPosition = moonPosition,
                        AngularDistanceToMoon = angularDistance
                    });

                    currentTime = currentTime.Add(request.TimeStep);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating batch chart data for DSO {Id}", request.Id);
                throw;
            }
        }


        public async Task<List<ChartDataPointDto>> LoadChartDataForSpecificTimes(
            double dsoRA,
            double dsoDEC,
            ObservatoryDto observatory,
            IEnumerable<DateTime> times,
            bool? moonDistanceCheckRequired,
            CancellationToken cancellationToken = default,
            double? precisionDegrees = null)
        {
            var dataPoints = new List<ChartDataPointDto>();

            foreach (var time in times)
            {
                try
                {
                    // Check cache first for DSO position and moon distance
                    var observatoryKey = $"{Math.Round(observatory.Latitude, 2)},{Math.Round(observatory.Longitude, 2)}";

                    // Use configurable precision for rounding (default 0.1 degrees for 1 decimal place)
                    var roundingPrecision = precisionDegrees.HasValue ? precisionDegrees.Value : 0.1;
                    var decimalPlaces = roundingPrecision >= 1.0 ? 0 : (roundingPrecision >= 0.1 ? 1 : 2);

                    var roundedRA = Math.Round(dsoRA / roundingPrecision) * roundingPrecision;
                    var roundedDEC = Math.Round(dsoDEC / roundingPrecision) * roundingPrecision;
                    roundedRA = Math.Round(roundedRA, decimalPlaces);
                    roundedDEC = Math.Round(roundedDEC, decimalPlaces);
                    var roundedTime = new DateTime(time.Year, time.Month, time.Day,
                        time.Hour, (time.Minute / 15) * 15, 0); // Round to 15-minute intervals

                    var cacheKey = new DsoPositionCacheKey
                    {
                        RoundedRA = roundedRA,
                        RoundedDEC = roundedDEC,
                        RoundedTime = roundedTime,
                        ObservatoryKey = observatoryKey
                    };

                    if (_dsoPositionCache.TryGetValue(cacheKey, out var cachedPosition) && cachedPosition.IsValid)
                    {
                        // Use cached position data
                        var horizonAltitude = GetCachedHorizonAltitude(cachedPosition.Azimuth, observatory.Latitude, observatory.Longitude, observatory.Elevation, observatory);

                        var dataPoint = new ChartDataPointDto
                        {
                            Time = time,
                            DsoPosition = new CelestialPositionDto
                            {
                                Altitude = cachedPosition.Altitude,
                                Azimuth = cachedPosition.Azimuth
                            },
                            ObservatoryHorizonAltitude = horizonAltitude,
                            AngularDistanceToMoon = cachedPosition.MoonDistance
                        };

                        // Add moon position if required (use cached moon position)
                        if (moonDistanceCheckRequired == null || moonDistanceCheckRequired == true)
                        {
                            dataPoint.MoonPosition = await GetMoonPositionAsync(observatory.Latitude, observatory.Longitude * -1, observatory.Elevation, time);
                        }

                        dataPoints.Add(dataPoint);
                        continue; // Skip expensive calculations
                    }

                    // Cache miss - perform full calculations
                    CelestialPositionDto dsoPosition = null;
                    MoonPositionDto moonPosition = null;
                    double angularMoonDistance = 0, observatoryHorizonAltitude = 0;

                    dsoPosition = await GetCelestialPositionAsync(dsoRA, dsoDEC, observatory.Latitude, observatory.Longitude * -1, time);


                    // if (moonDistanceCheckRequired == null || moonDistanceCheckRequired == true)
                    // always calculate it on cache miss => if user sets a filter later on, it is already in cache!
                    moonPosition = await GetMoonPositionAsync(observatory.Latitude, observatory.Longitude * -1, observatory.Elevation, time);
                    angularMoonDistance = await CalculateAngularDistanceAsync(dsoRA, dsoDEC, moonPosition.RightAscension, moonPosition.Declination);

                    observatoryHorizonAltitude = GetCachedHorizonAltitude(dsoPosition.Azimuth, observatory.Latitude, observatory.Longitude, observatory.Elevation, observatory);

                    // Cache the calculated data for future use
                    var newCacheValue = new DsoPositionCacheValue
                    {
                        Altitude = dsoPosition.Altitude,
                        Azimuth = dsoPosition.Azimuth,
                        MoonDistance = angularMoonDistance,
                        CachedAt = DateTime.UtcNow
                    };
                    _dsoPositionCache.TryAdd(cacheKey, newCacheValue);

                    dataPoints.Add(new ChartDataPointDto
                    {
                        Time = time,
                        DsoPosition = dsoPosition,
                        MoonPosition = moonPosition ?? new MoonPositionDto(),
                        AngularDistanceToMoon = angularMoonDistance,
                        ObservatoryHorizonAltitude = observatoryHorizonAltitude
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading chart data at time {Time}", time);
                    // Continue with next time point instead of breaking the entire calculation
                    continue;
                }
            }

            return dataPoints;
        }


        public async Task<Dictionary<Guid, BatchChartDataResponseDto>> GetBatchChartDataForMultipleDsosAsync(
            List<Guid> dsoIds,
            List<double> rightAscensions,
            List<double> declinations,
            ObservatoryDto observatory,
            DateTime startTime,
            DateTime endTime,
            TimeSpan timeStep)
        {
            try
            {
                if (dsoIds.Count != rightAscensions.Count || dsoIds.Count != declinations.Count)
                {
                    throw new ArgumentException("DSO IDs, right ascensions, and declinations lists must have the same length");
                }

                var result = new Dictionary<Guid, BatchChartDataResponseDto>();

                // Process each DSO
                for (int i = 0; i < dsoIds.Count; i++)
                {
                    var request = new BatchChartDataRequestDto
                    {
                        Id = dsoIds[i],
                        RightAscension = rightAscensions[i],
                        Declination = declinations[i],
                        Observatory = observatory,
                        StartTime = startTime,
                        EndTime = endTime,
                        TimeStep = timeStep
                    };

                    var chartData = await GetBatchChartDataAsync(request);
                    result[dsoIds[i]] = chartData;
                }

                _logger.LogInformation("Generated batch chart data for {Count} DSOs from {StartTime} to {EndTime}",
                    dsoIds.Count, startTime, endTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating batch chart data for multiple DSOs");
                throw;
            }
        }

        #endregion

        #region Field of View Calculations

        /// <summary>
        /// Calculates the field of view in degrees from pixel scale and image dimensions
        /// </summary>
        /// <param name="pixelScaleArcsecPerPixel">Pixel scale in arcseconds per pixel</param>
        /// <param name="widthPixels">Image width in pixels</param>
        /// <param name="heightPixels">Image height in pixels</param>
        /// <returns>Tuple containing (widthDegrees, heightDegrees)</returns>
        public async Task<(double WidthDegrees, double HeightDegrees)> CalculateFieldOfViewAsync(
            double pixelScaleArcsecPerPixel, int widthPixels, int heightPixels)
        {
            try
            {
                // Convert arcseconds to degrees (3600 arcseconds = 1 degree)
                const double arcsecondsPerDegree = 3600.0;

                // Calculate FOV in degrees
                double widthDegrees = (pixelScaleArcsecPerPixel * widthPixels) / arcsecondsPerDegree;
                double heightDegrees = (pixelScaleArcsecPerPixel * heightPixels) / arcsecondsPerDegree;

                _logger.LogDebug("Calculated FOV: {WidthDegrees:F3}° x {HeightDegrees:F3}° from pixel scale {PixelScale} arcsec/px and dimensions {Width}x{Height} px",
                    widthDegrees, heightDegrees, pixelScaleArcsecPerPixel, widthPixels, heightPixels);

                return await Task.FromResult((widthDegrees, heightDegrees));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating field of view from pixel scale {PixelScale} and dimensions {Width}x{Height}",
                    pixelScaleArcsecPerPixel, widthPixels, heightPixels);
                throw;
            }
        }

        #endregion

        #region Moon Avoidance Calculations

        /// <summary>
        /// Calculates RA/Dec points for moon avoidance circles using spherical trigonometry
        /// </summary>
        /// <param name="moonRa">Moon's right ascension in degrees</param>
        /// <param name="moonDec">Moon's declination in degrees</param>
        /// <param name="avoidanceDistances">List of avoidance distances in degrees</param>
        /// <param name="numPoints">Number of points to calculate per circle (default 100)</param>
        /// <returns>Dictionary mapping distance to array of [RA, Dec] points</returns>
        public async Task<Dictionary<double, double[][]>> CalculateMoonAvoidanceCirclePointsAsync(
            double moonRa, double moonDec, List<double> avoidanceDistances, int numPoints = 100)
        {
            try
            {
                moonRa = moonRa * 15;

                var result = new Dictionary<double, double[][]>();

                // Convert moon position to radians
                var moonRaRad = moonRa * Math.PI / 180.0;
                var moonDecRad = moonDec * Math.PI / 180.0;

                foreach (var distance in avoidanceDistances)
                {
                    var points = new double[numPoints][];
                    var distanceRad = distance * Math.PI / 180.0;

                    for (int i = 0; i < numPoints; i++)
                    {
                        var angle = (i / (double)numPoints) * 2 * Math.PI;

                        // Use spherical trigonometry to calculate actual celestial coordinates
                        var pointDecRad = Math.Asin(
                            Math.Sin(moonDecRad) * Math.Cos(distanceRad) +
                            Math.Cos(moonDecRad) * Math.Sin(distanceRad) * Math.Cos(angle)
                        );

                        var pointRaRad = moonRaRad + Math.Atan2(
                            Math.Sin(angle) * Math.Sin(distanceRad) * Math.Cos(moonDecRad),
                            Math.Cos(distanceRad) - Math.Sin(moonDecRad) * Math.Sin(pointDecRad)
                        );

                        // Convert back to degrees and normalize RA to 0-360 range
                        var pointRa = (pointRaRad * 180.0 / Math.PI + 360.0) % 360.0;
                        var pointDec = pointDecRad * 180.0 / Math.PI;

                        points[i] = new double[] { pointRa, pointDec };
                    }

                    result[distance] = points;
                }

                _logger.LogDebug("Calculated moon avoidance circle points for {Count} distances around moon at RA {MoonRa:F2}°, Dec {MoonDec:F2}°",
                    avoidanceDistances.Count, moonRa, moonDec);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating moon avoidance circle points for moon at RA {MoonRa}, Dec {MoonDec}", moonRa, moonDec);
                throw;
            }
        }

        #endregion

    }
}
