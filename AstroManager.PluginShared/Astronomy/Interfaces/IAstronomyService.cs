using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Model.Common;
using Shared.Model.DTO.Astro;
using Shared.Model.DTO.Common;
using Shared.Model.DTO.Settings;

namespace Shared.Services.Astronomy.Interfaces
{
    /// <summary>
    /// Service for performing astronomical calculations using AASharp
    /// </summary>
    public interface IAstronomyService
    {
        // Basic Position Calculations
        Task<(double Altitude, double Azimuth)> CalculateAltitudeAzimuthAsync(
            double rightAscension, double declination,
            double latitude, double longitude, DateTime dateTime);

        Task<CelestialPositionDto> GetCelestialPositionAsync(
            double rightAscension, double declination,
            double latitude, double longitude, DateTime dateTime);

        #region Twilight Calculations
        Task<AllTwilightTimesDto> GetTwilightTimesAsync(double latitude, double longitude, DateTime date);
        #endregion

        // Moon Calculations
        Task<MoonPositionDto> GetMoonPositionAsync(double latitude, double longitude, double height, DateTime dateTime);
        Task<double> GetMoonPhaseAsync(DateTime dateTime);
        Task<DateTime> GetNextMoonPhaseAsync(DateTime fromDate, double targetPhase = 0);

        // Sun Calculations
        Task<(double Ra, double Dec)> GetSunPositionAsync(DateTime dateTime);
        Task<DateTime> GetSunriseAsync(double latitude, double longitude, DateTime date);
        Task<DateTime> GetSunsetAsync(double latitude, double longitude, DateTime date);
        Task<TimeSpan> GetDaylightDurationAsync(double latitude, DateTime date);

        // Planet Calculations
        /*
        Task<PlanetPosition> GetPlanetPositionAsync(string planetName, double latitude, double longitude, DateTime dateTime);
        Task<IEnumerable<PlanetPosition>> GetPlanetPositionsAsync(double latitude, double longitude, DateTime dateTime);
        Task<Dictionary<string, (double Ra, double Dec)>> GetPlanetPositionsAsync(DateTime dateTime);
        */

        // Rise/Transit/Set Calculations
        /*
        Task<RiseTransitSetTimes> GetRiseTransitSetTimesAsync(
            double rightAscension, double declination,
            double latitude, double longitude, DateTime date,
            double altitude = 0);
        */
        Task<RiseTransitSetTimesDto> GetMoonRiseTransitSetTimesAsync(
            double latitude, double longitude, DateTime date);

        Task<RiseTransitSetTimesDto> GetSunRiseTransitSetTimesAsync(
            double latitude, double longitude, DateTime date);

        // Time and Calendar
        Task<double> GetJulianDateAsync(DateTime dateTime);
        Task<DateTime> FromJulianDateAsync(double jd);
        Task<double> GetSiderealTime(double longitude, DateTime dateTime);
        
        // Angular Calculations
        Task<double> CalculateAngularDistanceAsync(
            double ra1, double dec1, double ra2, double dec2);
            
        // Batch Chart Data
        Task<BatchChartDataResponseDto> GetBatchChartDataAsync(BatchChartDataRequestDto request);
        
        /// <summary>
        /// Gets astronomical data combined with user's moon avoidance profiles in a single API call
        /// </summary>
        /// <param name="observatory">Observatory information</param>
        /// <param name="selectedDate">Selected date for calculations</param>
        /// <returns>Combined astronomical data and moon avoidance profiles</returns>
        Task<AstronomicalData> GetAstronomicalDataAsync(ObservatoryDto observatory, DateTime selectedDate);
        

        /// <summary>
        /// Gets batch chart data for multiple DSOs using the same observatory and time parameters
        /// </summary>
        /// <param name="dsoIds">List of DSO IDs to get chart data for</param>
        /// <param name="rightAscensions">List of right ascensions corresponding to the DSO IDs</param>
        /// <param name="declinations">List of declinations corresponding to the DSO IDs</param>
        /// <param name="observatory">Observatory information</param>
        /// <param name="startTime">Start time for chart data</param>
        /// <param name="endTime">End time for chart data</param>
        /// <param name="timeStep">Time step between data points</param>
        /// <returns>Dictionary mapping DSO ID to chart data</returns>
        Task<Dictionary<Guid, BatchChartDataResponseDto>> GetBatchChartDataForMultipleDsosAsync(
            List<Guid> dsoIds,
            List<double> rightAscensions,
            List<double> declinations,
            ObservatoryDto observatory,
            DateTime startTime,
            DateTime endTime,
            TimeSpan timeStep);

        Task<List<ChartDataPointDto>> LoadChartDataForSpecificTimes(
            double dsoRA,
            double dsoDEC,
            ObservatoryDto observatory,
            IEnumerable<DateTime> times,
            bool? moonDistanceCheckRequired,
            CancellationToken cancellationToken = default,
            double? precisionDegrees = null);
        

            Task<double> CalculatePositionAngle(double ra1, double dec1, double ra2, double dec2);
        
        // Celestial Events
        Task<DateTime?> CalculateNextOppositionDateAsync(
            double rightAscension, double declination, DateTime fromDate);
            
        Task<DateTime?> CalculateNextConjunctionDateAsync(
            double ra1, double dec1, double ra2, double dec2, DateTime fromDate);
            
        Task<DateTime?> CalculateNextTransitDateAsync(
            double targetRa, double observerLongitude, DateTime fromDate);
            
        // Coordinate Transformations
        Task<(double Ra, double Dec)> ConvertAltAzToRaDecAsync(
            double altitude, double azimuth,
            double latitude, double longitude, DateTime dateTime);
        /*
        Task<(double Ra, double Dec)> ConvertEquatorialToGalactic(double ra, double dec);
        Task<(double Ra, double Dec)> ConvertGalacticToEquatorial(double l, double b);
        Task<(double Ra, double Dec)> ConvertEclipticToEquatorial(double lambda, double beta, DateTime dateTime);
        Task<(double Lambda, double Beta)> ConvertEquatorialToEcliptic(double ra, double dec, DateTime dateTime);
        */

        // Observation Planning
        Task<IEnumerable<(DateTime Date, double MoonPhase)>> GetMoonPhasesForMonth(DateTime month);
        Task<Dictionary<DateTime, string>> GetLunarCalendar(DateTime month);
        Task<IEnumerable<(string Name, DateTime BestTime)>> GetBestObservingTimes(
            double latitude, double longitude, DateTime startDate, int daysAhead);
            
        // Horizon Line Calculation
        Task<object[]> GetHorizonLineAsync(ObservatoryDto observatory, DateTime dateTime);
        
        // Utility Methods
        Task<double> GetAirMass(double altitudeDegrees);
        Task<double> GetAtmosphericRefraction(double apparentAltitude, double pressure = 1010, double temperature = 15);
        Task<double> GetLimitingMagnitude(double telescopeAperture, double fRatio, double pixelSize, double exposureTime, double skyBrightness);
        
        // Field of View Calculations
        /// <summary>
        /// Calculates the field of view in degrees from pixel scale and image dimensions
        /// </summary>
        /// <param name="pixelScaleArcsecPerPixel">Pixel scale in arcseconds per pixel</param>
        /// <param name="widthPixels">Image width in pixels</param>
        /// <param name="heightPixels">Image height in pixels</param>
        /// <returns>Tuple containing (widthDegrees, heightDegrees)</returns>
        Task<(double WidthDegrees, double HeightDegrees)> CalculateFieldOfViewAsync(
            double pixelScaleArcsecPerPixel, int widthPixels, int heightPixels);

        // Moon Avoidance Calculations
        /// <summary>
        /// Calculates RA/Dec points for moon avoidance circles using spherical trigonometry
        /// </summary>
        /// <param name="moonRa">Moon's right ascension in degrees</param>
        /// <param name="moonDec">Moon's declination in degrees</param>
        /// <param name="avoidanceDistances">List of avoidance distances in degrees</param>
        /// <param name="numPoints">Number of points to calculate per circle (default 100)</param>
        /// <returns>Dictionary mapping distance to array of [RA, Dec] points</returns>
        Task<Dictionary<double, double[][]>> CalculateMoonAvoidanceCirclePointsAsync(
            double moonRa, double moonDec, List<double> avoidanceDistances, int numPoints = 100);
    }
}
