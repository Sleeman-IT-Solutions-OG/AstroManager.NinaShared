using System;
using System.Collections.Generic;
using Shared.Model.Common;
using Shared.Model.DTO.Settings;

namespace Shared.Model.DTO.Astro
{
    public class CelestialPositionRequestDto
    {
        public double RightAscension { get; set; }
        public double Declination { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class MoonPositionRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Height { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class LocationDateRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Date { get; set; }
    }

    public class NextPhaseRequestDto
    {
        public DateTime FromDate { get; set; }
        public double TargetPhase { get; set; } = 0;
    }

    public class NextOppositionRequestDto
    {
        public double RightAscension { get; set; }
        public double Declination { get; set; }
        public DateTime FromDate { get; set; }
    }

    public class BatchChartDataRequestDto
    {
        public Guid? Id { get; set; }
        public double? RightAscension { get; set; }
        public double? Declination { get; set; }
        public ObservatoryDto Observatory { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeStep { get; set; }
    }

    public class ChartDataPointDto
    {
        public DateTime Time { get; set; }
        public CelestialPositionDto DsoPosition { get; set; }
        public MoonPositionDto MoonPosition { get; set; }
        public double AngularDistanceToMoon { get; set; }

        public double? ObservatoryHorizonAltitude { get; set; }
    }

    public class BatchChartDataResponseDto
    {
        public List<ChartDataPointDto> DataPoints { get; set; } = new();
    }
}
