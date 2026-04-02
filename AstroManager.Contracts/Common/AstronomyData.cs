using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model.Common
{
    /// <summary>
    /// Represents a celestial body's position and visibility information
    /// </summary>
    public class CelestialPositionDto
    {
        public double RightAscension { get; set; }  // in hours (0-24)
        public double Declination { get; set; }      // in degrees (-90 to 90)
        public double Altitude { get; set; }         // in degrees (-90 to 90)
        public double Azimuth { get; set; }          // in degrees (0-360, 0 = North)
        public double Distance { get; set; }        // in AU or km
        public double Magnitude { get; set; }        // Apparent magnitude
        public double IlluminatedFraction { get; set; } // 0-1 fraction of illuminated disk
        public double? AngularSize { get; set; }     // in arcminutes
    }

    /// <summary>
    /// Represents the position and phase of the Moon
    /// </summary>
    public class MoonPositionDto : CelestialPositionDto
    {
        public double Phase { get; set; }           // 0-1 where 0/1 = New, 0.5 = Full
        public double Elongation { get; set; }       // Elongation from Sun in degrees
        public double Age { get; set; }              // Age in days since New Moon
    }

    /// <summary>
    /// Represents a planet's position and visibility information
    /// </summary>
    public class PlanetPositionDto : CelestialPositionDto
    {
        public string Name { get; set; }
        public double Phase { get; set; }           // 0-1 phase of the planet
        public double Elongation { get; set; }       // Elongation from Sun in degrees
        public double DistanceFromEarth { get; set; } // in AU
        public double DistanceFromSun { get; set; }  // in AU
    }

    /// <summary>
    /// Represents rise, transit, and set times for a celestial object
    /// </summary>
    public class RiseTransitSetTimesDto
    {
        public DateTime? Rise { get; set; }         // UTC time of rise
        public DateTime? Transit { get; set; }       // UTC time of transit (highest point)
        public DateTime? Set { get; set; }           // UTC time of set
        public bool IsCircumpolar { get; set; }      // Whether the object is always above the horizon
        public bool NeverRises { get; set; }         // Whether the object never rises
    }

    /// <summary>
    /// Represents a constellation
    /// </summary>
    public class ConstellationInfoDto
    {
        public string Abbreviation { get; set; }    // Standard 3-letter abbreviation (e.g., "ORI")
        public string Name { get; set; }            // Full name (e.g., "Orion")
        public string Genitive { get; set; }        // Genitive form (e.g., "Orionis")
        public double CenterRa { get; set; }         // Center RA in hours
        public double CenterDec { get; set; }        // Center Dec in degrees
        public List<(double Ra, double Dec)> Boundary { get; set; } = new(); // Boundary points
    }


    public class AllTwilightTimesDto
    {
        public TwilightTimeDto Civil { get; set; } = new();
        public TwilightTimeDto Nautical { get; set; } = new();
        public TwilightTimeDto Astronomical { get; set; } = new();
        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }
        public DateTime? SolarNoon { get; set; }
    }

    public class TwilightTimeDto
    {
        public DateTime? Dawn { get; set; }
        public DateTime? Dusk { get; set; }
    }
}
