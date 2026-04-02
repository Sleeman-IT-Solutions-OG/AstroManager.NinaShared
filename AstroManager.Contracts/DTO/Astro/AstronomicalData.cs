using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model.DTO.Astro
{
    public class AstronomicalData
    {
        public DateTime Date { get; set; }

        public DateTime DateMoonPhase { get; set; }

        public string ObservatoryName { get; set; }

        public MoonPhase MoonPhase { get; set; }
        public DateTime? MoonRise { get; set; }
        public DateTime? MoonSet { get; set; }
        public double MoonIlluminatedFraction { get; set; }

        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }
        public DateTime? CivilBegin { get; set; }
        public DateTime? CivilEnd { get; set; }
        public DateTime? NauticalBegin { get; set; }
        public DateTime? NauticalEnd { get; set; }
        public DateTime? AstronomicalBegin { get; set; }
        public DateTime? AstronomicalEnd { get; set; }
        public DateTime? SolarNoon { get; set; }
        public double? SolarNoonAltitude { get; set; }
        public DateTime? Midnight { get; set; }
    }


    public enum MoonPhase
    {
        NewMoon,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbous,
        FullMoon,
        WaningGibbous,
        LastQuarter,
        WaningCrescent
    }
}
