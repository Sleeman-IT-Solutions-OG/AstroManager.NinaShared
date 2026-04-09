using Shared.Model.DTO.Settings;
using Shared.Model.Enums;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Response DTO for scheduler preview - shows what would be scheduled for a single night
/// </summary>
public class SchedulerPreviewDto
{
    /// <summary>
    /// The date this preview is for
    /// </summary>
    public DateTime PreviewDate { get; set; }
    
    /// <summary>
    /// Astronomical twilight start (dusk) - when imaging can begin
    /// </summary>
    public DateTime? AstronomicalDusk { get; set; }
    
    /// <summary>
    /// Astronomical twilight end (dawn) - when imaging must stop
    /// </summary>
    public DateTime? AstronomicalDawn { get; set; }
    
    /// <summary>
    /// Nautical twilight start (dusk) - earlier than astronomical, for filters that accept nautical twilight
    /// </summary>
    public DateTime? NauticalDusk { get; set; }
    
    /// <summary>
    /// Nautical twilight end (dawn) - later than astronomical, for filters that accept nautical twilight
    /// </summary>
    public DateTime? NauticalDawn { get; set; }
    
    /// <summary>
    /// Total available imaging time in minutes (astronomical twilight window)
    /// </summary>
    public double TotalImagingMinutes { get; set; }
    
    /// <summary>
    /// Extended imaging time including nautical twilight periods (for nautical-ok filters)
    /// </summary>
    public double ExtendedImagingMinutes { get; set; }
    
    /// <summary>
    /// Moon phase percentage (0-100)
    /// </summary>
    public double MoonPhasePercent { get; set; }
    
    /// <summary>
    /// Moon rise time (if applicable)
    /// </summary>
    public DateTime? MoonRise { get; set; }
    
    /// <summary>
    /// Moon set time (if applicable)
    /// </summary>
    public DateTime? MoonSet { get; set; }
    
    /// <summary>
    /// Scheduled sessions for this night
    /// </summary>
    public List<SchedulerPreviewSessionDto> Sessions { get; set; } = new();
    
    /// <summary>
    /// Targets that were considered but not scheduled, with reasons
    /// </summary>
    public List<SchedulerPreviewSkippedTargetDto> SkippedTargets { get; set; } = new();
    
    /// <summary>
    /// Summary statistics for this preview
    /// </summary>
    public SchedulerPreviewStatisticsDto Statistics { get; set; } = new();
    
    /// <summary>
    /// Gap at the start of night (from dusk to first session) in minutes
    /// </summary>
    public double StartGapMinutes { get; set; }
    
    /// <summary>
    /// Gap at the end of night (from last session to dawn) in minutes
    /// </summary>
    public double EndGapMinutes { get; set; }
    
    /// <summary>
    /// Display string for start gap (local time)
    /// </summary>
    public string StartGapDisplay => StartGapMinutes >= 1 && AstronomicalDusk.HasValue && Sessions.Any()
        ? $"⏸ Gap {AstronomicalDusk.Value.ToLocalTime():HH:mm}-{Sessions.OrderBy(s => s.StartTimeUtc).First().StartTimeLocal:HH:mm} ({StartGapMinutes:F0}min)"
        : string.Empty;
    
    /// <summary>
    /// Display string for end gap (local time)
    /// </summary>
    public string EndGapDisplay => EndGapMinutes >= 1 && AstronomicalDawn.HasValue && Sessions.Any()
        ? $"⏸ Gap {Sessions.OrderBy(s => s.EndTimeUtc).Last().EndTimeLocal:HH:mm}-{AstronomicalDawn.Value.ToLocalTime():HH:mm} ({EndGapMinutes:F0}min)"
        : string.Empty;
    
    /// <summary>
    /// Whether there is a start gap
    /// </summary>
    public bool HasStartGap => StartGapMinutes >= 1;
    
    /// <summary>
    /// Whether there is an end gap
    /// </summary>
    public bool HasEndGap => EndGapMinutes >= 1;
    
    /// <summary>
    /// List of unscheduled time slots with reasons why no target was selected
    /// </summary>
    public List<UnscheduledSlotDto> UnscheduledSlots { get; set; } = new();
    
    /// <summary>
    /// Whether the preview was generated successfully
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Error message if preview generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents an unscheduled time slot with reason
/// </summary>
public class UnscheduledSlotDto
{
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? BestCandidateTargetName { get; set; }
    public double? BestCandidateScore { get; set; }
    public List<SchedulerScoreContributionDto>? BestCandidateScoreBreakdown { get; set; }
    
    /// <summary>
    /// Start time in local time for display
    /// </summary>
    public DateTime StartTimeLocal => StartTimeUtc.ToLocalTime();
    
    /// <summary>
    /// End time in local time for display
    /// </summary>
    public DateTime EndTimeLocal => EndTimeUtc.ToLocalTime();
}

/// <summary>
/// A single scheduled session in the preview
/// </summary>
public class SchedulerPreviewSessionDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Target ID
    /// </summary>
    public Guid TargetId { get; set; }
    
    /// <summary>
    /// Target name for display
    /// </summary>
    public string TargetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Panel ID if this is a mosaic panel
    /// </summary>
    public Guid? PanelId { get; set; }
    
    /// <summary>
    /// Panel number for display
    /// </summary>
    public int? PanelNumber { get; set; }
    
    /// <summary>
    /// Scheduled start time (UTC)
    /// </summary>
    public DateTime StartTimeUtc { get; set; }
    
    /// <summary>
    /// Scheduled end time (UTC)
    /// </summary>
    public DateTime EndTimeUtc { get; set; }
    
    /// <summary>
    /// Scheduled start time (local time for display)
    /// </summary>
    public DateTime StartTimeLocal => StartTimeUtc.ToLocalTime();
    
    /// <summary>
    /// Scheduled end time (local time for display)
    /// </summary>
    public DateTime EndTimeLocal => EndTimeUtc.ToLocalTime();
    
    /// <summary>
    /// Duration in minutes
    /// </summary>
    public double DurationMinutes => (EndTimeUtc - StartTimeUtc).TotalMinutes;
    
    /// <summary>
    /// Filter to use
    /// </summary>
    public ECameraFilter Filter { get; set; }
    
    /// <summary>
    /// Planned effective imaging time (accounting for efficiency)
    /// </summary>
    public double PlannedImagingMinutes { get; set; }
    
    /// <summary>
    /// Estimated number of exposures
    /// </summary>
    public int EstimatedExposures { get; set; }
    
    /// <summary>
    /// Exposure time per frame in seconds
    /// </summary>
    public int ExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Average altitude during session
    /// </summary>
    public double AverageAltitude { get; set; }
    
    /// <summary>
    /// Transit time (meridian crossing) for this target (UTC)
    /// </summary>
    public DateTime? TransitTimeUtc { get; set; }
    
    /// <summary>
    /// Transit time in local time for display
    /// </summary>
    public DateTime? TransitTimeLocal => TransitTimeUtc?.ToLocalTime();
    
    /// <summary>
    /// Whether the session spans the meridian flip window
    /// </summary>
    public bool HasMeridianFlip { get; set; }
    
    /// <summary>
    /// Meridian flip window start (UTC) - pause imaging before this time
    /// </summary>
    public DateTime? MeridianFlipStartUtc { get; set; }
    
    /// <summary>
    /// Meridian flip window end (UTC) - can resume imaging after this time
    /// </summary>
    public DateTime? MeridianFlipEndUtc { get; set; }
    
    /// <summary>
    /// Average moon distance during session (degrees)
    /// </summary>
    public double MoonDistance { get; set; }
    
    /// <summary>
    /// Required minimum moon distance for this filter (from moon avoidance profile)
    /// </summary>
    public double? RequiredMoonDistance { get; set; }
    
    /// <summary>
    /// Moon illumination percentage at session midpoint
    /// </summary>
    public double MoonIllumination { get; set; }
    
    /// <summary>
    /// Target coordinates - RA in hours
    /// </summary>
    public double RaHours { get; set; }
    
    /// <summary>
    /// Target coordinates - Dec in degrees
    /// </summary>
    public double DecDegrees { get; set; }
    
    /// <summary>
    /// Filter shooting method (Loop, Batch, Sequential)
    /// </summary>
    public string? FilterShootMethod { get; set; }
    
    /// <summary>
    /// Batch size if using batch method
    /// </summary>
    public int? BatchSize { get; set; }
    
    /// <summary>
    /// Filter segments with planned exposures for this session in batch order.
    /// Format: "L:10,R:10,Ha:10,L:10,R:4" shows the actual shooting sequence.
    /// </summary>
    public string? FilterSegments { get; set; }
    
    /// <summary>
    /// Summarized filter exposures (totals per filter) for list/inline display.
    /// Parses FilterSegments and sums by filter: "L:10,R:10,Ha:10,L:10,R:4" becomes "L:20 R:14 Ha:10"
    /// </summary>
    public string FilterSummary
    {
        get
        {
            if (string.IsNullOrEmpty(FilterSegments))
                return $"{Filter}:{PlannedExposures}";
            
            // Sum exposures by filter from the segment list
            var filterTotals = new Dictionary<string, int>();
            foreach (var segment in FilterSegments.Split(','))
            {
                var parts = segment.Trim().Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out var count))
                {
                    var filter = parts[0];
                    if (filterTotals.ContainsKey(filter))
                        filterTotals[filter] += count;
                    else
                        filterTotals[filter] = count;
                }
            }
            
            if (filterTotals.Count == 0)
                return $"{Filter}:{PlannedExposures}";
            
            return string.Join(" ", filterTotals.Select(kv => $"{kv.Key}:{kv.Value}"));
        }
    }
    
    /// <summary>
    /// Total planned exposures across all filters in this session.
    /// </summary>
    public int PlannedExposures { get; set; }
    
    /// <summary>
    /// Per-filter progress info: Filter -> (Current, Planned, Goal)
    /// </summary>
    public List<FilterProgressInfo>? FilterProgressList { get; set; }
    
    /// <summary>
    /// Timed filter segments showing batch order with start/end times
    /// </summary>
    public List<TimedFilterSegment>? TimedFilterSegments { get; set; }
    
    /// <summary>
    /// Compact display string for filters - shows first filter only with count
    /// </summary>
    public string FilterDisplay
    {
        get
        {
            if (FilterProgressList != null && FilterProgressList.Count > 0)
            {
                var first = FilterProgressList[0];
                if (FilterProgressList.Count == 1)
                    return $"{first.Filter}";
                return $"{first.Filter} +{FilterProgressList.Count - 1}";
            }
            return Filter.ToString();
        }
    }
    
    /// <summary>
    /// Multi-line filter progress display for details view
    /// Format: "L: 5→15/20" per line
    /// </summary>
    public string FilterProgressDisplay
    {
        get
        {
            if (FilterProgressList == null || FilterProgressList.Count == 0)
                return $"{Filter}: {CurrentShots}→{EstimatedEndShots}/{TotalRequiredShots}";
            
            var lines = FilterProgressList.Select(f => 
                $"{f.Filter}: {f.CurrentExposures}→{f.EstimatedEndExposures}/{f.GoalExposures}");
            return string.Join("\n", lines);
        }
    }
    
    /// <summary>
    /// Compact inline filter progress for session list (single line)
    /// Format: "L:0→10/20 R:0→10/20 Ha:0→6/20"
    /// </summary>
    public string FilterProgressCompact
    {
        get
        {
            if (FilterProgressList == null || FilterProgressList.Count == 0)
                return $"{Filter}:{CurrentShots}→{EstimatedEndShots}/{TotalRequiredShots}";
            
            var parts = FilterProgressList.Select(f => 
                $"{f.Filter}:{f.CurrentExposures}→{f.EstimatedEndExposures}/{f.GoalExposures}");
            return string.Join(" ", parts);
        }
    }
    
    /// <summary>
    /// Detailed exposure plan showing what will be shot in this session with timing.
    /// Example: "17:20-18:10: 10x L (300s)\n18:10-19:00: 10x R (300s)"
    /// </summary>
    public string ExposurePlan
    {
        get
        {
            // Use TimedFilterSegments if available (shows batch order with timing)
            if (TimedFilterSegments != null && TimedFilterSegments.Count > 0)
            {
                var lines = TimedFilterSegments.Select(s =>
                    s.Filter == "Meridian Flip" 
                        ? $"{s.StartTimeLocal:HH:mm}-{s.EndTimeLocal:HH:mm}: Meridian Flip"
                        : $"{s.StartTimeLocal:HH:mm}-{s.EndTimeLocal:HH:mm}: {s.Count}x {s.Filter} ({s.ExposureTimeSeconds}s)");
                return string.Join("\n", lines);
            }
            
            // Fallback to FilterProgressList (summed totals, no timing)
            if (FilterProgressList != null && FilterProgressList.Count > 0)
            {
                var lines = FilterProgressList.Select(f =>
                {
                    var ditherInfo = f.DitherEveryX == 0 ? " [no dither]" : 
                                     f.DitherEveryX > 0 ? $" [dither every {f.DitherEveryX}]" : "";
                    return $"{f.PlannedExposures}x {f.Filter} ({f.ExposureTimeSeconds}s){ditherInfo}";
                });
                return string.Join("\n", lines);
            }
            
            if (string.IsNullOrEmpty(FilterSegments))
            {
                return $"{EstimatedExposures}x {Filter} ({ExposureTimeSeconds}s)";
            }
            
            var fallbackLines = new List<string>();
            foreach (var segment in FilterSegments.Split(','))
            {
                var parts = segment.Split(':');
                if (parts.Length == 2)
                {
                    var filter = parts[0];
                    var count = parts[1];
                    fallbackLines.Add($"{count}x {filter} ({ExposureTimeSeconds}s)");
                }
            }
            return string.Join("\n", fallbackLines);
        }
    }
    
    /// <summary>
    /// Priority score that determined selection
    /// </summary>
    public double PriorityScore { get; set; }

    public List<SchedulerScoreContributionDto>? ScoreBreakdown { get; set; }
    
    /// <summary>
    /// Reason this target/session was selected
    /// </summary>
    public string SelectionReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Altitude data points for graphing and tooltip (every 5-10 minutes) - SESSION TIME ONLY
    /// </summary>
    public List<AltitudeDataPoint>? AltitudeData { get; set; }
    
    /// <summary>
    /// Full night altitude data for filter observability calculations (dusk to dawn)
    /// </summary>
    public List<AltitudeDataPoint>? FullNightAltitudeData { get; set; }
    
    /// <summary>
    /// Gap in minutes before this session (from previous session end or from dusk)
    /// </summary>
    public double GapBeforeMinutes { get; set; }
    
    /// <summary>
    /// Start time of the gap (previous session end or dusk)
    /// </summary>
    public DateTime? GapStartTime { get; set; }
    
    /// <summary>
    /// Display string for gap before this session (empty if no significant gap) - local time
    /// </summary>
    public string GapDisplay => GapBeforeMinutes >= 1 && GapStartTime.HasValue 
        ? $"⏸ Gap {GapStartTime.Value.ToLocalTime():HH:mm}-{StartTimeLocal:HH:mm} ({GapBeforeMinutes:F0}min)" 
        : string.Empty;
    
    /// <summary>
    /// Whether there is a significant gap before this session (>= 1 minute)
    /// </summary>
    public bool HasGapBefore => GapBeforeMinutes >= 1;
    
    /// <summary>
    /// Current number of shots already taken for this imaging goal
    /// </summary>
    public int CurrentShots { get; set; }
    
    /// <summary>
    /// Estimated shots after this session completes
    /// </summary>
    public int EstimatedEndShots { get; set; }
    
    /// <summary>
    /// Total required shots for this imaging goal
    /// </summary>
    public int TotalRequiredShots { get; set; }
}

/// <summary>
/// Per-filter progress information for a session (summed totals per filter)
/// </summary>
public class FilterProgressInfo
{
    public string Filter { get; set; } = string.Empty;
    public int CurrentExposures { get; set; }
    public int PlannedExposures { get; set; }
    public int EstimatedEndExposures { get; set; }
    public int GoalExposures { get; set; }
    public int ExposureTimeSeconds { get; set; }
    public int DitherEveryX { get; set; } = -1;
}

/// <summary>
/// Timed filter segment for sequence display (shows batch order with timing)
/// </summary>
public class TimedFilterSegment
{
    public string Filter { get; set; } = string.Empty;
    public int Count { get; set; }
    public int ExposureTimeSeconds { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Start time in local time for display
    /// </summary>
    public DateTime StartTimeLocal => StartTime.ToLocalTime();
    
    /// <summary>
    /// End time in local time for display
    /// </summary>
    public DateTime EndTimeLocal => EndTime.ToLocalTime();
}

/// <summary>
/// A target that was skipped during scheduling
/// </summary>
public class SchedulerPreviewSkippedTargetDto
{
    public Guid TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public Guid? PanelId { get; set; }
    public int? PanelNumber { get; set; }
    public string Reason { get; set; } = string.Empty;
    public SkipReason ReasonCode { get; set; }
    public double? PriorityScore { get; set; }
    public List<SchedulerScoreContributionDto>? ScoreBreakdown { get; set; }
    public string? WinningTargetName { get; set; }
    public double? WinningTargetScore { get; set; }
    
    /// <summary>
    /// Detailed explanation of why the target was skipped (human-readable)
    /// </summary>
    public string? DetailedExplanation { get; set; }
    
    /// <summary>
    /// Target coordinates - RA in hours
    /// </summary>
    public double RaHours { get; set; }
    
    /// <summary>
    /// Target coordinates - Dec in degrees
    /// </summary>
    public double DecDegrees { get; set; }
    
    /// <summary>
    /// Maximum altitude reached during the night (degrees)
    /// </summary>
    public double? MaxAltitude { get; set; }
    
    /// <summary>
    /// Time when max altitude is reached (UTC)
    /// </summary>
    public DateTime? MaxAltitudeTime { get; set; }
    
    /// <summary>
    /// Observable window start (if any) - when target rises above min altitude
    /// </summary>
    public DateTime? ObservableStart { get; set; }
    
    /// <summary>
    /// Observable window end (if any) - when target sets below min altitude
    /// </summary>
    public DateTime? ObservableEnd { get; set; }
    
    /// <summary>
    /// Minimum altitude configured for this target (degrees)
    /// </summary>
    public double? MinAltitudeRequired { get; set; }
    
    /// <summary>
    /// Moon distance at transit (degrees)
    /// </summary>
    public double? MoonDistanceAtTransit { get; set; }
    
    /// <summary>
    /// Minimum moon distance required (degrees)
    /// </summary>
    public double? MinMoonDistanceRequired { get; set; }
    
    /// <summary>
    /// Altitude data points for graphing (optional, only if IncludeAltitudeData is true)
    /// </summary>
    public List<AltitudeDataPoint>? AltitudeData { get; set; }
}

/// <summary>
/// Altitude data point for graphing
/// </summary>
public class AltitudeDataPoint
{
    public DateTime TimeUtc { get; set; }
    public double Altitude { get; set; }
    public double Azimuth { get; set; }
    public double? MoonDistance { get; set; }
    public double? MoonAltitude { get; set; }
    
    /// <summary>
    /// Filter being used at this time point (for tooltip display)
    /// </summary>
    public string? Filter { get; set; }
    
    /// <summary>
    /// Required minimum moon distance for the current filter at this time point
    /// </summary>
    public double? RequiredMoonDistance { get; set; }
}

/// <summary>
/// Reasons a target was skipped
/// </summary>
public enum SkipReason
{
    NotObservable,
    BelowMinAltitude,
    MoonTooClose,
    MoonPhase,
    GoalsComplete,
    MaxHoursReached,
    NotInDateRange,
    FilterBlocked,
    InsufficientTime,
    WrongEquipment,
    TargetInactive,
    LowerPriority,
    NoGoalsDefined
}

/// <summary>
/// Statistics summary for the preview
/// </summary>
public class SchedulerPreviewStatisticsDto
{
    public int TotalSessions { get; set; }
    public int UniqueTargets { get; set; }
    public double TotalPlannedMinutes { get; set; }
    public double TotalPlannedHours => TotalPlannedMinutes / 60.0;
    public double TotalNightMinutes { get; set; }
    public double UnscheduledMinutes => TotalNightMinutes - TotalPlannedMinutes;
    public double UnscheduledHours => UnscheduledMinutes / 60.0;
    public int TotalEstimatedExposures { get; set; }
    public Dictionary<string, int> SessionsByFilter { get; set; } = new();
    public Dictionary<string, double> MinutesByFilter { get; set; } = new();
    public Dictionary<Guid, double> MinutesByTarget { get; set; } = new();
    public double ImagingEfficiencyUsed { get; set; }
    public double UtilizationPercent { get; set; }
}

/// <summary>
/// Observable time window for a specific filter on a target
/// </summary>
public class FilterObservabilityWindow
{
    /// <summary>
    /// Filter name (L, R, Ha, etc.)
    /// </summary>
    public string Filter { get; set; } = string.Empty;
    
    /// <summary>
    /// Start time of this observable window (UTC)
    /// </summary>
    public DateTime StartTimeUtc { get; set; }
    
    /// <summary>
    /// End time of this observable window (UTC)
    /// </summary>
    public DateTime EndTimeUtc { get; set; }
    
    /// <summary>
    /// Start time in local time for display
    /// </summary>
    public DateTime StartTimeLocal => StartTimeUtc.ToLocalTime();
    
    /// <summary>
    /// End time in local time for display
    /// </summary>
    public DateTime EndTimeLocal => EndTimeUtc.ToLocalTime();
    
    /// <summary>
    /// Duration in minutes
    /// </summary>
    public double DurationMinutes => (EndTimeUtc - StartTimeUtc).TotalMinutes;
    
    /// <summary>
    /// Why this window ends (e.g., "Moon rises", "Below horizon", "Dawn")
    /// </summary>
    public string? EndReason { get; set; }
    
    /// <summary>
    /// Display string for this window
    /// </summary>
    public string Display => $"{StartTimeLocal:HH:mm}-{EndTimeLocal:HH:mm} ({DurationMinutes:F0}min)";
}

/// <summary>
/// Target observability info showing when each filter can be used
/// </summary>
public class TargetFilterObservabilityDto
{
    /// <summary>
    /// Target ID
    /// </summary>
    public Guid TargetId { get; set; }
    
    /// <summary>
    /// Target name
    /// </summary>
    public string TargetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Panel ID (if mosaic panel)
    /// </summary>
    public Guid? PanelId { get; set; }
    
    /// <summary>
    /// Panel number (if mosaic panel)
    /// </summary>
    public int? PanelNumber { get; set; }
    
    /// <summary>
    /// Time when target rises above horizon (local time)
    /// </summary>
    public DateTime? RiseTimeLocal { get; set; }
    
    /// <summary>
    /// Time when target sets below horizon (local time)
    /// </summary>
    public DateTime? SetTimeLocal { get; set; }
    
    /// <summary>
    /// Maximum altitude during the night
    /// </summary>
    public double MaxAltitude { get; set; }
    
    /// <summary>
    /// Time of maximum altitude (local time)
    /// </summary>
    public DateTime? MaxAltitudeTimeLocal { get; set; }
    
    /// <summary>
    /// Observable windows per filter (filter name -> list of windows)
    /// Only includes filters with active imaging goals that are not complete
    /// </summary>
    public Dictionary<string, List<FilterObservabilityWindow>> FilterWindows { get; set; } = new();
    
    /// <summary>
    /// Altitude data points for graphing
    /// </summary>
    public List<AltitudeDataPoint>? AltitudeData { get; set; }
}

/// <summary>
/// Response for scheduler run (multi-night)
/// </summary>
public class SchedulerRunResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalNights { get; set; }
    public int TotalSessionsCreated { get; set; }
    public double TotalPlannedHours { get; set; }
    public int TargetsScheduled { get; set; }
    public List<ScheduledSessionDto> Sessions { get; set; } = new();
    public Dictionary<string, int> SessionsByFilter { get; set; } = new();
    public Dictionary<Guid, double> HoursByTarget { get; set; } = new();
}
