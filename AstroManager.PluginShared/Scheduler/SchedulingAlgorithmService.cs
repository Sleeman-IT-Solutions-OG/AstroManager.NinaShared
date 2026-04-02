using Microsoft.Extensions.Logging;
using Shared.Model.Common;
using Shared.Model.DTO.Scheduler;
using Shared.Model.DTO.Settings;
using Shared.Model.Enums;
using Shared.Services.Astronomy.Interfaces;

namespace Shared.Services.Scheduler;

/// <summary>
/// Shared scheduling algorithm service - runs locally in both Blazor and NINA
/// </summary>
public class SchedulingAlgorithmService : ISchedulingAlgorithmService
{
    private readonly IAstronomyService _astronomyService;
    private readonly ILogger<SchedulingAlgorithmService> _logger;

    public SchedulingAlgorithmService(
        IAstronomyService astronomyService,
        ILogger<SchedulingAlgorithmService> logger)
    {
        _astronomyService = astronomyService;
        _logger = logger;
    }

    public async Task<SchedulerRunResult> RunSchedulerAsync(
        List<ScheduledTargetDto> targets,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        EquipmentDto equipment,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        IProgress<SchedulerProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting scheduler algorithm for {TargetCount} targets from {StartDate} to {EndDate}",
                targets.Count, configuration.StartDate, configuration.EndDate);

            var result = new SchedulerRunResult { Success = true };
            var sessions = new List<ScheduledSessionDto>();

            // Calculate date range
            var startDate = configuration.StartDate.Date;
            var endDate = configuration.EndDate.Date;
            var totalNights = (endDate - startDate).Days + 1;

            // Initialize target state tracking
            var targetStates = InitializeTargetStates(targets);

            // Process each night
            for (int nightIndex = 0; nightIndex < totalNights; nightIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var currentDate = startDate.AddDays(nightIndex);

                progressCallback?.Report(new SchedulerProgress
                {
                    CurrentNight = nightIndex + 1,
                    TotalNights = totalNights,
                    CurrentDate = currentDate,
                    SessionsGenerated = sessions.Count,
                    CurrentActivity = $"Processing night {nightIndex + 1}/{totalNights}"
                });

                _logger.LogDebug("Processing night {NightIndex}/{TotalNights}: {Date}",
                    nightIndex + 1, totalNights, currentDate);

                // Get twilight times for this night
                var twilightTimes = await _astronomyService.GetTwilightTimesAsync(
                    observatory.Latitude, observatory.Longitude, currentDate.AddHours(12));

                if (!twilightTimes.Astronomical.Dawn.HasValue ||
                    !twilightTimes.Astronomical.Dusk.HasValue)
                {
                    _logger.LogWarning("No astronomical twilight for {Date}, skipping", currentDate);
                    continue;
                }

                // Calculate observable windows for all targets
                var observableWindows = await CalculateObservableWindowsForNightAsync(
                    targets, currentDate, twilightTimes, observatory, configuration,
                    moonAvoidanceProfiles, cancellationToken);

                // Allocate time slots for this night
                var (nightSessions, _) = AllocateTimeSlots(
                    observableWindows, targetStates, configuration, equipment,
                    currentDate, twilightTimes, moonAvoidanceProfiles);

                sessions.AddRange(nightSessions);

                // Update target states based on allocated sessions
                UpdateTargetStates(targetStates, nightSessions, configuration);
            }

            result.Sessions = sessions;
            result.Statistics = CalculateStatistics(sessions, targets, totalNights);

            _logger.LogInformation("Scheduler completed: {SessionCount} sessions generated for {TargetCount} targets",
                sessions.Count, result.Statistics.TargetsScheduled);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running scheduler algorithm");
            return new SchedulerRunResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SchedulerPreviewDto> GeneratePreviewAsync(
        List<ScheduledTargetDto> targets,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        DateTime previewDate,
        MeridianFlipSettingsDto? meridianFlipSettings = null,
        DateTime? startTimeOverride = null,
        IProgress<SchedulerProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var preview = new SchedulerPreviewDto
        {
            PreviewDate = previewDate.Date,
            Success = true
        };

        try
        {
            _logger.LogInformation("Generating preview for {Date} with {TargetCount} targets, StartTimeOverride={StartTime}", 
                previewDate, targets.Count, startTimeOverride?.ToString("HH:mm") ?? "none");
            _logger.LogDebug("PreviewDate details: Date={Date:yyyy-MM-dd HH:mm:ss}, Kind={Kind}, DateForTwilight={TwilightDate:yyyy-MM-dd HH:mm:ss}",
                previewDate, previewDate.Kind, previewDate.AddHours(12));

            // Get twilight times
            var twilightTimes = await _astronomyService.GetTwilightTimesAsync(
                observatory.Latitude, observatory.Longitude, previewDate.AddHours(12));
            
            _logger.LogDebug("Raw twilight from AASharp: AstroDawn={Dawn:yyyy-MM-dd HH:mm} (Kind={DawnKind}), AstroDusk={Dusk:yyyy-MM-dd HH:mm} (Kind={DuskKind})",
                twilightTimes.Astronomical.Dawn, twilightTimes.Astronomical.Dawn?.Kind,
                twilightTimes.Astronomical.Dusk, twilightTimes.Astronomical.Dusk?.Kind);

            if (!twilightTimes.Astronomical.Dusk.HasValue || !twilightTimes.Astronomical.Dawn.HasValue)
            {
                preview.Success = false;
                preview.ErrorMessage = "Cannot calculate twilight times for this date and location";
                return preview;
            }

            // LEGACY SWAP: AASharp naming uses Dawn=evening and Dusk=morning, so we swap
            preview.AstronomicalDusk = twilightTimes.Astronomical.Dawn.Value; // Evening start (swapped)
            preview.AstronomicalDawn = twilightTimes.Astronomical.Dusk.Value; // Morning end (swapped)
            
            // Handle midnight crossing for astronomical
            if (preview.AstronomicalDawn.Value < preview.AstronomicalDusk.Value)
            {
                preview.AstronomicalDawn = preview.AstronomicalDawn.Value.AddDays(1);
            }
            
            // Nautical twilight times (earlier dusk, later dawn - extends imaging window)
            // LEGACY SWAP: Same swap as astronomical
            if (twilightTimes.Nautical.Dawn.HasValue && twilightTimes.Nautical.Dusk.HasValue)
            {
                preview.NauticalDusk = twilightTimes.Nautical.Dawn.Value; // Evening (swapped)
                preview.NauticalDawn = twilightTimes.Nautical.Dusk.Value; // Morning (swapped)
                
                // Handle midnight crossing for nautical
                if (preview.NauticalDawn.Value < preview.NauticalDusk.Value)
                {
                    preview.NauticalDawn = preview.NauticalDawn.Value.AddDays(1);
                }
                
                preview.ExtendedImagingMinutes = (preview.NauticalDawn.Value - preview.NauticalDusk.Value).TotalMinutes;
            }
            
            // Calculate total imaging minutes based on actual scheduling window
            // If startTimeOverride is provided and later than astronomical dusk, use that as the start
            var effectiveStartTime = preview.AstronomicalDusk.Value;
            if (startTimeOverride.HasValue && startTimeOverride.Value > preview.AstronomicalDusk.Value)
            {
                effectiveStartTime = startTimeOverride.Value;
            }
            preview.TotalImagingMinutes = (preview.AstronomicalDawn.Value - effectiveStartTime).TotalMinutes;
            
            _logger.LogInformation("Preview twilight times: AstroDusk={Dusk:HH:mm}, AstroDawn={Dawn:HH:mm}, EffectiveStart={Start:HH:mm}, TotalMinutes={Minutes:F0}",
                preview.AstronomicalDusk, preview.AstronomicalDawn, effectiveStartTime, preview.TotalImagingMinutes);
            _logger.LogInformation("Nautical twilight: NautDusk={Dusk:HH:mm}, NautDawn={Dawn:HH:mm}, ExtendedMinutes={Minutes:F0}",
                preview.NauticalDusk, preview.NauticalDawn, preview.ExtendedImagingMinutes);
            
            // Log moon avoidance configuration
            _logger.LogInformation("MoonAvoidance config: UseMoonAvoidance={UseMoonAvoidance}, ProfileCount={Count}",
                configuration.UseMoonAvoidance, moonAvoidanceProfiles?.Count ?? 0);
            if (moonAvoidanceProfiles?.Any() == true)
            {
                foreach (var mapping in moonAvoidanceProfiles)
                {
                    var profile = mapping.MoonAvoidanceProfile;
                    if (profile != null)
                    {
                        _logger.LogInformation("MoonAvoidance profile: Filter={Filter} -> '{ProfileName}' (FullMoonDist={Full}°, Width={Width}days, MinMoonAlt={MinAlt}°)",
                            mapping.Filter, profile.Name, profile.FullMoonDistanceDegrees, profile.WidthInDays, profile.MinMoonAltitudeDegrees);
                    }
                }
            }

            // Get moon data
            // AASharp uses west-positive longitude convention, so negate for eastern longitudes
            var moonData = await _astronomyService.GetMoonPositionAsync(
                observatory.Latitude, observatory.Longitude * -1, observatory.Elevation,
                preview.AstronomicalDusk.Value);
            preview.MoonPhasePercent = moonData.IlluminatedFraction * 100;

            progressCallback?.Report(new SchedulerProgress
            {
                CurrentNight = 1,
                TotalNights = 1,
                CurrentDate = previewDate,
                SessionsGenerated = 0,
                CurrentActivity = "Calculating observable windows..."
            });

            // Initialize target states
            var targetStates = InitializeTargetStates(targets);
            
            // Debug: Log target states
            foreach (var kvp in targetStates)
            {
                var state = kvp.Value;
                var remainingSum = state.FilterProgress.Values.Sum(fp => fp.RemainingMinutes);
                var goalCount = state.FilterProgress.Count;
                _logger.LogInformation("Target '{Name}' state: Priority={Priority}, Goals={GoalCount}, TotalRemaining={Remaining}min, Filters=[{Filters}]",
                    state.Target.Name, state.Target.Priority, goalCount, remainingSum,
                    string.Join(", ", state.FilterProgress.Select(f => $"{f.Key}:{f.Value.RemainingMinutes}min")));
            }

            // Calculate observable windows
            var observableWindows = await CalculateObservableWindowsForNightAsync(
                targets, previewDate, twilightTimes, observatory, configuration,
                moonAvoidanceProfiles, cancellationToken);
            
            // Debug: Log observable windows
            _logger.LogInformation("Observable windows: {Count} windows for {TargetCount} targets", 
                observableWindows.Count, targets.Count);
            foreach (var window in observableWindows)
            {
                var periodsCount = window.ObservablePeriods.Count;
                var observablePeriods = window.ObservablePeriods.Where(p => p.IsObservable).ToList();
                _logger.LogInformation("  Window for '{Target}': {TotalPeriods} periods, {ObservablePeriods} observable",
                    window.Target?.Name ?? "Unknown", periodsCount, observablePeriods.Count);
                
                // Update target state with observable minutes for TimeFirst strategy
                if (targetStates.TryGetValue(window.TargetId, out var state))
                {
                    state.ObservableMinutesTonight = window.TotalObservableMinutes;
                }
            }

            // Allocate time slots (reuse existing logic with dummy equipment)
            var dummyEquipment = new EquipmentDto { Id = Guid.Empty, NameOfEquipment = "Preview" };
            var (nightSessions, unscheduledSlots) = AllocateTimeSlots(
                observableWindows, targetStates, configuration, dummyEquipment,
                previewDate, twilightTimes, moonAvoidanceProfiles, observatory.Longitude, meridianFlipSettings, startTimeOverride);

            // Store unscheduled slots in preview (merge consecutive slots with same reason)
            preview.UnscheduledSlots = MergeUnscheduledSlots(unscheduledSlots);

            // Convert to preview sessions with altitude data points every 5 minutes
            preview.Sessions = nightSessions.Select(s => {
                // Find the observable window for this target/panel
                // For mosaic panels: TargetId in window is set to panel.Id, so match on PanelId
                // For non-mosaic: TargetId matches and PanelId is null
                var window = s.PanelId != null
                    ? observableWindows.FirstOrDefault(w => w.PanelId == s.PanelId)
                    : observableWindows.FirstOrDefault(w => w.TargetId == s.ScheduledTargetId && w.PanelId == null);
                
                // Extract altitude data points for this session's time range only (for chart display)
                List<AltitudeDataPoint>? altitudeData = null;
                List<AltitudeDataPoint>? fullNightAltitudeData = null;
                if (window?.ObservablePeriods != null)
                {
                    // Session-only data for chart
                    altitudeData = window.ObservablePeriods
                        .Where(p => p.StartTime >= s.StartTimeUtc && p.StartTime <= s.EndTimeUtc)
                        .Select(p => new AltitudeDataPoint
                        {
                            TimeUtc = p.StartTime,
                            Altitude = p.Altitude,
                            Azimuth = p.Azimuth,
                            MoonDistance = p.MoonDistance,
                            MoonAltitude = p.MoonAltitude
                        })
                        .ToList();
                    
                    // Full night data for filter observability calculations
                    fullNightAltitudeData = window.ObservablePeriods
                        .Select(p => new AltitudeDataPoint
                        {
                            TimeUtc = p.StartTime,
                            Altitude = p.Altitude,
                            Azimuth = p.Azimuth,
                            MoonDistance = p.MoonDistance,
                            MoonAltitude = p.MoonAltitude
                        })
                        .ToList();
                }
                
                // Find imaging goal for shot counts
                var target = targets.FirstOrDefault(t => t.Id == s.ScheduledTargetId);
                var imagingGoal = target?.ImagingGoals?.FirstOrDefault(g => g.Filter == s.Filter);
                var goalMultiplier = Math.Max(1, target?.RepeatCount ?? 1);
                int currentShots = imagingGoal?.CompletedExposures ?? 0;
                int totalRequired = (imagingGoal?.GoalExposureCount ?? 0) * goalMultiplier;
                int exposureTime = imagingGoal?.ExposureTimeSeconds ?? 300;
                int estimatedExposures = exposureTime > 0 ? (int)(s.PlannedDurationMinutes * 60 / exposureTime) : 0;
                
                // Build per-filter progress list from FilterSegments (summed by filter)
                // FilterSegments is in sequence format: "L:10,R:10,Ha:10,L:10,R:4"
                // We need to sum to get totals: L:20, R:14, Ha:10
                var filterProgressList = new List<FilterProgressInfo>();
                var timedFilterSegments = new List<TimedFilterSegment>();
                
                if (!string.IsNullOrEmpty(s.FilterSegments))
                {
                    // First build timed segments (preserving batch order with timing)
                    var currentTime = s.StartTimeUtc;
                    var segmentParts = s.FilterSegments.Split(',');
                    for (int i = 0; i < segmentParts.Length; i++)
                    {
                        var parts = segmentParts[i].Trim().Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out var count))
                        {
                            var filterName = parts[0];
                            var filterGoal = target?.ImagingGoals?.FirstOrDefault(g => g.Filter.ToString() == filterName && g.IsEnabled);
                            var expTimeSec = filterGoal?.ExposureTimeSeconds ?? 300;
                            
                            // Calculate duration for this segment (exposures * time per exposure / efficiency)
                            var segmentDurationMin = (count * expTimeSec / 60.0) / (configuration.ImagingEfficiencyPercent / 100.0);
                            var segmentEnd = currentTime.AddMinutes(segmentDurationMin);
                            
                            // For the last segment, extend to session end if there's a small gap (< 10 min)
                            // This ensures tooltip shows the last filter for the remaining session time
                            bool isLastSegment = (i == segmentParts.Length - 1);
                            if (isLastSegment && segmentEnd < s.EndTimeUtc)
                            {
                                var gapMinutes = (s.EndTimeUtc - segmentEnd).TotalMinutes;
                                if (gapMinutes < 10)
                                {
                                    segmentEnd = s.EndTimeUtc;
                                }
                            }
                            
                            timedFilterSegments.Add(new TimedFilterSegment
                            {
                                Filter = filterName,
                                Count = count,
                                ExposureTimeSeconds = expTimeSec,
                                StartTime = currentTime,
                                EndTime = segmentEnd
                            });
                            
                            currentTime = segmentEnd;
                        }
                    }
                    
                    // Sum exposures by filter for FilterProgressList
                    var filterTotals = new Dictionary<string, int>();
                    foreach (var segment in s.FilterSegments.Split(','))
                    {
                        var parts = segment.Trim().Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out var count))
                        {
                            var filterName = parts[0];
                            if (filterTotals.ContainsKey(filterName))
                                filterTotals[filterName] += count;
                            else
                                filterTotals[filterName] = count;
                        }
                    }
                    
                    // Build FilterProgressInfo for each unique filter
                    foreach (var kvp in filterTotals)
                    {
                        var filterName = kvp.Key;
                        var planned = kvp.Value;
                        var filterGoal = target?.ImagingGoals?.FirstOrDefault(g => g.Filter.ToString() == filterName && g.IsEnabled);
                        var goalWithMultiplier = (filterGoal?.GoalExposureCount ?? 0) * goalMultiplier;
                        filterProgressList.Add(new FilterProgressInfo
                        {
                            Filter = filterName,
                            CurrentExposures = filterGoal?.CompletedExposures ?? 0,
                            PlannedExposures = planned,
                            EstimatedEndExposures = Math.Min((filterGoal?.CompletedExposures ?? 0) + planned, goalWithMultiplier > 0 ? goalWithMultiplier : planned),
                            GoalExposures = goalWithMultiplier,
                            ExposureTimeSeconds = filterGoal?.ExposureTimeSeconds ?? 300,
                            DitherEveryX = filterGoal?.DitherEveryX ?? -1
                        });
                    }
                }
                
                // Add filter info to altitude data points based on timed segments
                if (altitudeData != null && timedFilterSegments.Count > 0)
                {
                    var lastSegment = timedFilterSegments.Last();
                    foreach (var point in altitudeData)
                    {
                        // Use < for most segments, but <= for the last segment to include endpoint
                        var segment = timedFilterSegments.FirstOrDefault(seg => 
                            point.TimeUtc >= seg.StartTime && 
                            (seg == lastSegment ? point.TimeUtc <= seg.EndTime : point.TimeUtc < seg.EndTime));
                        point.Filter = segment?.Filter;
                    }
                }
                
                // Get coordinates - use panel center for mosaics, target center otherwise
                var raHours = s.PanelCenterRA ?? target?.RightAscension ?? 0;
                var decDegrees = s.PanelCenterDec ?? target?.Declination ?? 0;
                
                return new SchedulerPreviewSessionDto
                {
                    Id = s.Id,
                    TargetId = s.ScheduledTargetId,
                    TargetName = target?.Name ?? "Unknown",
                    PanelId = s.PanelId,
                    PanelNumber = s.PanelNumber,
                    StartTimeUtc = s.StartTimeUtc,
                    EndTimeUtc = s.EndTimeUtc,
                    Filter = s.Filter,
                    FilterSegments = s.FilterSegments,
                    PlannedExposures = s.PlannedExposures,
                    FilterProgressList = filterProgressList.Count > 0 ? filterProgressList : null,
                    TimedFilterSegments = timedFilterSegments.Count > 0 ? timedFilterSegments : null,
                    PlannedImagingMinutes = s.PlannedDurationMinutes,
                    EstimatedExposures = s.PlannedExposures > 0 ? s.PlannedExposures : estimatedExposures,
                    ExposureTimeSeconds = exposureTime,
                    FilterShootMethod = s.FilterShootMethod,
                    BatchSize = s.BatchSize,
                    MoonDistance = s.MoonDistance ?? 0,
                    MoonIllumination = (s.MoonIllumination ?? 0) * 100, // Convert 0-1 to percentage
                    RequiredMoonDistance = s.RequiredMoonDistance,
                    AltitudeData = altitudeData,
                    FullNightAltitudeData = fullNightAltitudeData,
                    AverageAltitude = altitudeData?.Any() == true ? altitudeData.Average(a => a.Altitude) : 0,
                    CurrentShots = currentShots,
                    EstimatedEndShots = Math.Min(currentShots + (s.PlannedExposures > 0 ? s.PlannedExposures : estimatedExposures), totalRequired),
                    TotalRequiredShots = totalRequired,
                    RaHours = raHours,
                    DecDegrees = decDegrees,
                    // Transit time and meridian flip info - use panel RA for mosaics
                    // Each panel has its own transit time based on its center coordinates
                    TransitTimeUtc = CalculateTransitTime(
                        raHours,
                        observatory.Longitude,
                        preview.AstronomicalDusk ?? DateTime.UtcNow,
                        preview.AstronomicalDawn ?? DateTime.UtcNow.AddHours(10)),
                    HasMeridianFlip = false, // Will be set below
                    MeridianFlipStartUtc = null, // Will be set below
                    MeridianFlipEndUtc = null // Will be set below
                };
            }).ToList();
            
            // Calculate meridian flip windows for each session
            foreach (var session in preview.Sessions)
            {
                if (session.TransitTimeUtc.HasValue && meridianFlipSettings?.Enabled == true)
                {
                    var (flipStart, flipEnd) = CalculateMeridianFlipWindow(session.TransitTimeUtc, meridianFlipSettings);
                    session.MeridianFlipStartUtc = flipStart;
                    session.MeridianFlipEndUtc = flipEnd;
                    
                    // Check if this session spans the flip window
                    if (flipStart.HasValue && flipEnd.HasValue)
                    {
                        session.HasMeridianFlip = 
                            (session.StartTimeUtc <= flipEnd.Value && session.EndTimeUtc >= flipStart.Value);
                        
                        // Insert "Meridian Flip" segment into TimedFilterSegments if flip occurs during session
                        if (session.HasMeridianFlip && session.TimedFilterSegments != null && session.TimedFilterSegments.Count > 0)
                        {
                            var flipStartClamped = flipStart.Value < session.StartTimeUtc ? session.StartTimeUtc : flipStart.Value;
                            var flipEndClamped = flipEnd.Value > session.EndTimeUtc ? session.EndTimeUtc : flipEnd.Value;
                            
                            var newSegments = new List<TimedFilterSegment>();
                            foreach (var segment in session.TimedFilterSegments)
                            {
                                // Check if this segment overlaps with flip window
                                if (segment.EndTime <= flipStartClamped || segment.StartTime >= flipEndClamped)
                                {
                                    // No overlap - keep segment as-is
                                    newSegments.Add(segment);
                                }
                                else
                                {
                                    // Segment overlaps with flip window - split it
                                    // Part before flip
                                    if (segment.StartTime < flipStartClamped)
                                    {
                                        var beforeDuration = (flipStartClamped - segment.StartTime).TotalMinutes;
                                        var totalDuration = (segment.EndTime - segment.StartTime).TotalMinutes;
                                        var beforeCount = (int)Math.Round(segment.Count * beforeDuration / totalDuration);
                                        if (beforeCount > 0)
                                        {
                                            newSegments.Add(new TimedFilterSegment
                                            {
                                                Filter = segment.Filter,
                                                Count = beforeCount,
                                                ExposureTimeSeconds = segment.ExposureTimeSeconds,
                                                StartTime = segment.StartTime,
                                                EndTime = flipStartClamped
                                            });
                                        }
                                    }
                                    
                                    // Meridian flip segment (only add once, at the point where it starts in sequence)
                                    if (!newSegments.Any(s => s.Filter == "Meridian Flip"))
                                    {
                                        newSegments.Add(new TimedFilterSegment
                                        {
                                            Filter = "Meridian Flip",
                                            Count = 0,
                                            ExposureTimeSeconds = 0,
                                            StartTime = flipStartClamped,
                                            EndTime = flipEndClamped
                                        });
                                    }
                                    
                                    // Part after flip
                                    if (segment.EndTime > flipEndClamped)
                                    {
                                        var afterDuration = (segment.EndTime - flipEndClamped).TotalMinutes;
                                        var totalDuration = (segment.EndTime - segment.StartTime).TotalMinutes;
                                        var afterCount = (int)Math.Round(segment.Count * afterDuration / totalDuration);
                                        if (afterCount > 0)
                                        {
                                            newSegments.Add(new TimedFilterSegment
                                            {
                                                Filter = segment.Filter,
                                                Count = afterCount,
                                                ExposureTimeSeconds = segment.ExposureTimeSeconds,
                                                StartTime = flipEndClamped,
                                                EndTime = segment.EndTime
                                            });
                                        }
                                    }
                                }
                            }
                            
                            // Sort by start time and replace
                            session.TimedFilterSegments = newSegments.OrderBy(s => s.StartTime).ToList();
                            
                            // Update altitude data points to show "Meridian Flip" during flip window
                            if (session.AltitudeData != null)
                            {
                                foreach (var point in session.AltitudeData)
                                {
                                    if (point.TimeUtc >= flipStartClamped && point.TimeUtc <= flipEndClamped)
                                    {
                                        point.Filter = "Meridian Flip";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Calculate gaps between sessions, at start, and at end of night
            if (preview.Sessions.Count > 0 && preview.AstronomicalDusk.HasValue && preview.AstronomicalDawn.HasValue)
            {
                var orderedSessions = preview.Sessions.OrderBy(s => s.StartTimeUtc).ToList();
                DateTime previousEnd = preview.AstronomicalDusk.Value;
                
                // Calculate start gap (from dusk to first session)
                var firstSession = orderedSessions.First();
                preview.StartGapMinutes = Math.Max(0, (firstSession.StartTimeUtc - preview.AstronomicalDusk.Value).TotalMinutes);
                
                bool isFirst = true;
                foreach (var session in orderedSessions)
                {
                    var gapMinutes = (session.StartTimeUtc - previousEnd).TotalMinutes;
                    // Don't set GapBeforeMinutes for first session - that's already shown as StartGap
                    if (!isFirst)
                    {
                        session.GapBeforeMinutes = Math.Max(0, gapMinutes);
                        session.GapStartTime = previousEnd;
                    }
                    previousEnd = session.EndTimeUtc;
                    isFirst = false;
                }
                
                // Calculate end gap (from last session to dawn)
                var lastSession = orderedSessions.Last();
                preview.EndGapMinutes = Math.Max(0, (preview.AstronomicalDawn.Value - lastSession.EndTimeUtc).TotalMinutes);
            }

            // Calculate statistics
            // Use actual session time spans for TotalPlannedMinutes (not efficiency-adjusted)
            // This gives accurate "empty time" calculation
            var actualScheduledMinutes = preview.Sessions.Sum(s => (s.EndTimeUtc - s.StartTimeUtc).TotalMinutes);
            
            preview.Statistics = new SchedulerPreviewStatisticsDto
            {
                TotalSessions = preview.Sessions.Count,
                UniqueTargets = preview.Sessions.Select(s => s.TargetId).Distinct().Count(),
                TotalPlannedMinutes = actualScheduledMinutes,
                TotalNightMinutes = preview.TotalImagingMinutes,
                TotalEstimatedExposures = preview.Sessions.Sum(s => s.EstimatedExposures),
                SessionsByFilter = preview.Sessions.GroupBy(s => s.Filter.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                MinutesByFilter = preview.Sessions.GroupBy(s => s.Filter.ToString()).ToDictionary(g => g.Key, g => g.Sum(s => s.PlannedImagingMinutes)),
                MinutesByTarget = preview.Sessions.GroupBy(s => s.TargetId).ToDictionary(g => g.Key, g => g.Sum(s => s.PlannedImagingMinutes)),
                ImagingEfficiencyUsed = configuration.ImagingEfficiencyPercent,
                UtilizationPercent = preview.TotalImagingMinutes > 0
                    ? (actualScheduledMinutes / preview.TotalImagingMinutes) * 100
                    : 0
            };

            _logger.LogInformation("Preview generated: {SessionCount} sessions, {Hours:F1}h planned",
                preview.Sessions.Count, preview.Statistics.TotalPlannedHours);

            return preview;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview for {Date}", previewDate);
            preview.Success = false;
            preview.ErrorMessage = ex.Message;
            return preview;
        }
    }

    public async Task<SchedulerPreviewDto> GeneratePreviewAsync(
        List<ScheduledTargetDto> targets,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        DateTime previewDate,
        MeridianFlipSettingsDto? meridianFlipSettings,
        bool includeDetailedExplanations,
        bool includeAltitudeData,
        int altitudeDataIntervalMinutes = 15,
        DateTime? startTimeOverride = null,
        IProgress<SchedulerProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        // Generate base preview
        var preview = await GeneratePreviewAsync(
            targets, configuration, observatory, moonAvoidanceProfiles,
            previewDate, meridianFlipSettings, startTimeOverride, progressCallback, cancellationToken);

        if (!preview.Success || !includeDetailedExplanations)
            return preview;

        // Get scheduled target IDs
        var scheduledTargetIds = preview.Sessions.Select(s => s.TargetId).ToHashSet();
        var hasAnyScheduledSessions = preview.Sessions.Any();

        // Get twilight times for altitude calculations
        var twilightTimes = await _astronomyService.GetTwilightTimesAsync(
            observatory.Latitude, observatory.Longitude, previewDate.AddHours(12));

        if (!twilightTimes.Astronomical.Dusk.HasValue || !twilightTimes.Astronomical.Dawn.HasValue)
            return preview;

        // LEGACY SWAP: AASharp naming uses Dawn=evening and Dusk=morning, so we swap
        var nightStart = twilightTimes.Astronomical.Dawn.Value; // Evening (swapped)
        var nightEnd = twilightTimes.Astronomical.Dusk.Value;   // Morning (swapped)
        // Handle day boundary: if end is before start, it's next morning
        if (nightEnd < nightStart)
            nightEnd = nightEnd.AddDays(1);

        // Respect preview "start now" override when explaining why targets were skipped.
        var analysisWindowStart = nightStart;
        if (startTimeOverride.HasValue && startTimeOverride.Value > analysisWindowStart && startTimeOverride.Value < nightEnd)
        {
            analysisWindowStart = startTimeOverride.Value;
        }

        // Get moon position (AASharp uses west-positive longitude)
        var moonPosition = await _astronomyService.GetMoonPositionAsync(
            observatory.Latitude, observatory.Longitude * -1, observatory.Elevation, analysisWindowStart);

        // Find skipped targets and explain why
        foreach (var target in targets)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Skip if target was scheduled
            if (scheduledTargetIds.Contains(target.Id)) continue;

            var skipped = await AnalyzeSkippedTargetAsync(
                target, configuration, observatory, moonAvoidanceProfiles,
                analysisWindowStart, nightEnd, moonPosition, includeAltitudeData,
                altitudeDataIntervalMinutes, hasAnyScheduledSessions,
                BuildNoScheduleDiagnosticsForTarget(preview.UnscheduledSlots, target.Name),
                cancellationToken);

            if (skipped != null)
                preview.SkippedTargets.Add(skipped);
        }

        return preview;
    }

    private async Task<SchedulerPreviewSkippedTargetDto?> AnalyzeSkippedTargetAsync(
        ScheduledTargetDto target,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        DateTime nightStart,
        DateTime nightEnd,
        MoonPositionDto moonPosition,
        bool includeAltitudeData,
        int altitudeIntervalMinutes,
        bool hasAnyScheduledSessions,
        string? noScheduleDiagnostics,
        CancellationToken cancellationToken)
    {
        // Get target-specific effective min altitude (override → template → config)
        var effectiveMinAltitude = GetEffectiveMinAltitude(target, configuration);
        
        var skipped = new SchedulerPreviewSkippedTargetDto
        {
            TargetId = target.Id,
            TargetName = target.Name,
            RaHours = target.RightAscension,
            DecDegrees = target.Declination,
            MinAltitudeRequired = effectiveMinAltitude
        };

        // Check target status
        if (target.Status != ScheduledTargetStatus.Active)
        {
            skipped.ReasonCode = SkipReason.TargetInactive;
            skipped.Reason = $"Target status is {target.Status}";
            skipped.DetailedExplanation = $"Target '{target.Name}' is not active (status: {target.Status}). Only active targets are scheduled.";
            return skipped;
        }

        // Check if goals are complete (for mosaics, check panel goals with fallback to parent)
        // RepeatCount multiplies the goal times (e.g., 7 means shoot all goals 7 times)
        var repeatCount = Math.Max(1, target.RepeatCount);
        double totalRemaining;
        if (target.IsMosaic && target.HasPanels)
        {
            // For mosaics: check panel goals first, fall back to parent goals if panels have no goals
            var panelsWithGoals = target.Panels.Where(p => p.ImagingGoals?.Any(g => g.RemainingTimeMinutes > 0) == true).ToList();
            
            if (panelsWithGoals.Count > 0)
            {
                // Use panel-specific goals - apply RepeatCount
                // Remaining = (GoalTime * RepeatCount) - Completed
                totalRemaining = target.Panels
                    .Where(p => p.ImagingGoals != null)
                    .SelectMany(p => p.ImagingGoals)
                    .Sum(g => (g.GoalTimeMinutes * repeatCount) - g.CompletedTimeMinutes);
            }
            else if (target.ImagingGoals?.Any(g => g.RemainingTimeMinutes > 0) == true)
            {
                // Fallback: use parent goals (base goals not synced to panels yet)
                // Each panel uses parent goals, so total = (parent goals * RepeatCount) * panel count
                var parentRemaining = target.ImagingGoals.Sum(g => (g.GoalTimeMinutes * repeatCount) - g.CompletedTimeMinutes);
                totalRemaining = parentRemaining * target.Panels.Count;
            }
            else
            {
                skipped.ReasonCode = SkipReason.NoGoalsDefined;
                skipped.Reason = $"No imaging goals defined";
                skipped.DetailedExplanation = $"Mosaic target '{target.Name}' has no imaging goals defined on parent or panels.";
                return skipped;
            }
        }
        else
        {
            // Regular target - apply RepeatCount
            totalRemaining = target.ImagingGoals?.Sum(g => (g.GoalTimeMinutes * repeatCount) - g.CompletedTimeMinutes) ?? 0;
        }
        
        if (totalRemaining <= 0)
        {
            skipped.ReasonCode = SkipReason.GoalsComplete;
            skipped.Reason = "All imaging goals complete";
            skipped.DetailedExplanation = $"Target '{target.Name}' has completed all imaging goals. No additional exposure time needed.";
            return skipped;
        }

        // Calculate altitude throughout the night
        var altitudeData = new List<AltitudeDataPoint>();
        double maxAltitude = -90;
        DateTime? maxAltitudeTime = null;
        DateTime? observableStart = null;
        DateTime? observableEnd = null;
        double moonDistanceAtTransit = 0;

        var timeStep = TimeSpan.FromMinutes(includeAltitudeData ? altitudeIntervalMinutes : 30);
        var currentTime = nightStart;

        while (currentTime <= nightEnd)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Convert RA from hours (0-24) to degrees (0-360) for astronomy calculations
            var raInDegrees = target.RightAscension * 15.0;
            
            var (altitude, azimuth) = await _astronomyService.CalculateAltitudeAzimuthAsync(
                raInDegrees, target.Declination,
                observatory.Latitude, observatory.Longitude, currentTime);

            var moonDistance = await _astronomyService.CalculateAngularDistanceAsync(
                raInDegrees, target.Declination,
                moonPosition.RightAscension, moonPosition.Declination);

            if (includeAltitudeData)
            {
                altitudeData.Add(new AltitudeDataPoint
                {
                    TimeUtc = currentTime,
                    Altitude = altitude,
                    MoonDistance = moonDistance
                });
            }

            if (altitude > maxAltitude)
            {
                maxAltitude = altitude;
                maxAltitudeTime = currentTime;
                moonDistanceAtTransit = moonDistance;
            }

            // Track observable window
            if (altitude >= effectiveMinAltitude)
            {
                if (!observableStart.HasValue)
                    observableStart = currentTime;
                observableEnd = currentTime;
            }

            currentTime = currentTime.Add(timeStep);
        }

        skipped.MaxAltitude = maxAltitude;
        skipped.MaxAltitudeTime = maxAltitudeTime;
        skipped.ObservableStart = observableStart;
        skipped.ObservableEnd = observableEnd;
        skipped.MoonDistanceAtTransit = moonDistanceAtTransit;

        if (includeAltitudeData)
            skipped.AltitudeData = altitudeData;

        // Determine reason
        if (maxAltitude < effectiveMinAltitude)
        {
            skipped.ReasonCode = SkipReason.BelowMinAltitude;
            skipped.Reason = $"Max altitude {maxAltitude:F1}° below minimum {effectiveMinAltitude}°";
            // Convert times to observatory local time for display
            var localMaxAltTime = maxAltitudeTime.HasValue 
                ? TimeZoneInfo.ConvertTimeFromUtc(maxAltitudeTime.Value, observatory.ObservatoryTimeZoneInfo) 
                : (DateTime?)null;
            var tzAbbrevAlt = maxAltitudeTime.HasValue 
                ? GetTimezoneAbbreviation(observatory.ObservatoryTimeZoneInfo, maxAltitudeTime.Value) 
                : "";
            skipped.DetailedExplanation = $"Target '{target.Name}' never rises above the minimum altitude of {effectiveMinAltitude}° during this night. " +
                $"Maximum altitude reached is {maxAltitude:F1}° at {localMaxAltTime:HH:mm} {tzAbbrevAlt}.";
            return skipped;
        }

        if (!observableStart.HasValue)
        {
            skipped.ReasonCode = SkipReason.NotObservable;
            skipped.Reason = "Not observable during imaging window";
            skipped.DetailedExplanation = $"Target '{target.Name}' is not above {effectiveMinAltitude}° during the astronomical twilight period.";
            return skipped;
        }

        // Check moon avoidance - use target-specific setting
        var effectiveUseMoonAvoidance = GetEffectiveUseMoonAvoidance(target, configuration);
        if (effectiveUseMoonAvoidance && moonPosition.IlluminatedFraction > 0.1)
        {
            // Find minimum required moon distance from profiles (using CalculateAvoidanceDistance based on moon phase)
            var minMoonDistance = moonAvoidanceProfiles
                .Where(p => p.MoonAvoidanceProfile != null && target.ImagingGoals.Any(g => g.IsEnabled && g.Filter == p.Filter))
                .Select(p => p.MoonAvoidanceProfile!.CalculateAvoidanceDistance(moonPosition.IlluminatedFraction))
                .DefaultIfEmpty(30)
                .Min();

            skipped.MinMoonDistanceRequired = minMoonDistance;

            if (moonDistanceAtTransit < minMoonDistance)
            {
                skipped.ReasonCode = SkipReason.MoonTooClose;
                skipped.Reason = $"Moon {moonDistanceAtTransit:F2}° away, need {minMoonDistance:F2}°";
                skipped.DetailedExplanation = $"Target '{target.Name}' is too close to the Moon ({moonDistanceAtTransit:F2}° separation). " +
                    $"Moon avoidance requires at least {minMoonDistance:F2}° separation. Moon illumination: {moonPosition.IlluminatedFraction * 100:F0}%.";
                return skipped;
            }
        }

        // Check observable time - use target-specific min session duration
        var effectiveMinSessionDuration = GetEffectiveMinSessionDuration(target, configuration);
        var observableMinutes = observableEnd.HasValue && observableStart.HasValue
            ? (observableEnd.Value - observableStart.Value).TotalMinutes
            : 0;

        if (observableMinutes < effectiveMinSessionDuration)
        {
            skipped.ReasonCode = SkipReason.InsufficientTime;
            skipped.Reason = $"Only {observableMinutes:F0}min observable, need {effectiveMinSessionDuration}min";
            // Convert times to observatory local time for display
            var localStartInsuff = TimeZoneInfo.ConvertTimeFromUtc(observableStart!.Value, observatory.ObservatoryTimeZoneInfo);
            var localEndInsuff = TimeZoneInfo.ConvertTimeFromUtc(observableEnd!.Value, observatory.ObservatoryTimeZoneInfo);
            var tzAbbrevInsuff = GetTimezoneAbbreviation(observatory.ObservatoryTimeZoneInfo, observableStart.Value);
            skipped.DetailedExplanation = $"Target '{target.Name}' is only observable for {observableMinutes:F0} minutes " +
                $"(from {localStartInsuff:HH:mm} to {localEndInsuff:HH:mm} {tzAbbrevInsuff}), " +
                $"but the minimum session duration is {effectiveMinSessionDuration} minutes.";
            return skipped;
        }
        
        // Check moon phase constraints (target-specific only - no global config for these)
        var effectiveMinMoonPhase = GetEffectiveMinMoonPhasePercent(target);
        var effectiveMaxMoonPhase = GetEffectiveMaxMoonPhasePercent(target);
        var moonPhasePercent = moonPosition.IlluminatedFraction * 100;
        
        if (effectiveMinMoonPhase.HasValue && moonPhasePercent < effectiveMinMoonPhase.Value)
        {
            skipped.ReasonCode = SkipReason.MoonPhase;
            skipped.Reason = $"Moon phase {moonPhasePercent:F0}% below minimum {effectiveMinMoonPhase.Value:F0}%";
            skipped.DetailedExplanation = $"Target '{target.Name}' requires minimum moon phase of {effectiveMinMoonPhase.Value:F0}% " +
                $"but current moon phase is only {moonPhasePercent:F0}%.";
            return skipped;
        }
        
        if (effectiveMaxMoonPhase.HasValue && moonPhasePercent > effectiveMaxMoonPhase.Value)
        {
            skipped.ReasonCode = SkipReason.MoonPhase;
            skipped.Reason = $"Moon phase {moonPhasePercent:F0}% above maximum {effectiveMaxMoonPhase.Value:F0}%";
            skipped.DetailedExplanation = $"Target '{target.Name}' requires maximum moon phase of {effectiveMaxMoonPhase.Value:F0}% " +
                $"but current moon phase is {moonPhasePercent:F0}%.";
            return skipped;
        }

        // If we get here, target was skippable for other reasons (e.g., lower priority)
        skipped.ReasonCode = SkipReason.LowerPriority;
        
        // Build a more informative reason - use target-specific max hours
        var effectiveMaxHoursPerNight = GetEffectiveMaxHoursPerNight(target, configuration);
        var constraintInfo = new List<string>();
        if (effectiveMaxHoursPerNight > 0)
            constraintInfo.Add($"MaxPerNight={effectiveMaxHoursPerNight}h");
        // MaxTotalHoursPerTarget removed - not needed for scheduler
        
        var constraintText = constraintInfo.Any() ? $" Config: {string.Join(", ", constraintInfo)}." : "";
        
        // Convert times to observatory local time for display
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(observableStart!.Value, observatory.ObservatoryTimeZoneInfo);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(observableEnd!.Value, observatory.ObservatoryTimeZoneInfo);
        var tzAbbrev = GetTimezoneAbbreviation(observatory.ObservatoryTimeZoneInfo, observableStart.Value);
        
        // Check target's imaging goals to provide better diagnostics
        var repeatCountGoals = Math.Max(1, target.RepeatCount);
        var goalsInfo = "";
        if (target.ImagingGoals == null || !target.ImagingGoals.Any())
        {
            skipped.Reason = "No imaging goals defined";
            skipped.ReasonCode = SkipReason.NoGoalsDefined;
            goalsInfo = "Target has no imaging goals defined. ";
        }
        else
        {
            var enabledGoals = target.ImagingGoals.Where(g => g.IsEnabled).ToList();
            if (!enabledGoals.Any())
            {
                skipped.Reason = "All imaging goals are disabled";
                skipped.ReasonCode = SkipReason.NoGoalsDefined;
                goalsInfo = "All imaging goals are disabled. ";
            }
            else
            {
                var totalRemainingMin = enabledGoals.Sum(g => (g.GoalTimeMinutes * repeatCountGoals) - g.CompletedTimeMinutes);
                if (totalRemainingMin <= 0)
                {
                    skipped.Reason = "All imaging goals are complete";
                    skipped.ReasonCode = SkipReason.GoalsComplete;
                    goalsInfo = "All imaging goals are already complete. ";
                }
                else
                {
                    skipped.Reason = $"Lower priority than scheduled targets (Priority={target.Priority})";
                    var filterList = string.Join(", ", enabledGoals.Select(g => $"{g.Filter}:{g.RemainingTimeMinutes:F0}min"));
                    goalsInfo = $"Goals: {filterList}. ";
                }
            }
        }
        
        skipped.DetailedExplanation = (skipped.DetailedExplanation ?? "") + 
            $"Target '{target.Name}' was observable but not scheduled. " +
            $"Observable: {localStart:HH:mm}-{localEnd:HH:mm} {tzAbbrev} ({observableMinutes:F0}min). " +
            $"Max altitude: {maxAltitude:F1}°. Priority: {target.Priority}.{constraintText} " +
            goalsInfo +
            (skipped.ReasonCode == SkipReason.LowerPriority
                ? (hasAnyScheduledSessions
                    ? "Higher priority targets filled the available time slots."
                    : $"No targets were scheduled in this preview window with the current constraints. {(string.IsNullOrWhiteSpace(noScheduleDiagnostics) ? string.Empty : $"Likely blockers: {noScheduleDiagnostics}.")}")
                : "");

        return skipped;
    }

    private static string? BuildNoScheduleDiagnosticsForTarget(List<UnscheduledSlotDto>? unscheduledSlots, string targetName)
    {
        if (unscheduledSlots == null || unscheduledSlots.Count == 0 || string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        var targetPrefix = $"{targetName}:";
        var panelPrefix = $"{targetName} P";

        var reasons = unscheduledSlots
            .SelectMany(s => (s.Reason ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries))
            .Select(r => r.Trim())
            .Where(r => r.StartsWith(targetPrefix, StringComparison.OrdinalIgnoreCase) ||
                        r.StartsWith(panelPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(r =>
            {
                var idx = r.IndexOf(':');
                return idx >= 0 && idx < r.Length - 1 ? r[(idx + 1)..].Trim() : r;
            })
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        return reasons.Count > 0 ? string.Join("; ", reasons) : null;
    }

    private Dictionary<Guid, TargetSchedulingState> InitializeTargetStates(List<ScheduledTargetDto> targets)
    {
        var states = new Dictionary<Guid, TargetSchedulingState>();

        foreach (var target in targets)
        {
            // RepeatCount multiplies the goal times (e.g., 7 means shoot all goals 7 times)
            var repeatCount = Math.Max(1, target.RepeatCount);
            
            if (target.IsMosaic && target.HasPanels)
            {
                foreach (var panel in target.Panels)
                {
                    // Use panel ID as the key for mosaic panel states
                    // Base goals should be synced to panel.ImagingGoals, but fall back to target goals if empty
                    Dictionary<ECameraFilter, FilterProgress> filterProgress;
                    
                    if (panel.ImagingGoals?.Any() == true)
                    {
                        // Use panel-specific goals - apply RepeatCount to goals
                        filterProgress = panel.ImagingGoals
                            .Where(g => g.IsEnabled)
                            .GroupBy(g => g.Filter)
                            .ToDictionary(
                                group => group.Key,
                                group => new FilterProgress
                                {
                                    // Goal is multiplied by RepeatCount
                                    GoalMinutes = group.Sum(g => g.GoalTimeMinutes) * repeatCount,
                                    // Completed stays as actual progress
                                    CompletedMinutes = group.Sum(g => g.CompletedTimeMinutes),
                                    ScheduledMinutes = 0,
                                    // Remaining = (Goal * RepeatCount) - Completed
                                    RemainingMinutes = (group.Sum(g => g.GoalTimeMinutes) * repeatCount) - group.Sum(g => g.CompletedTimeMinutes)
                                });
                    }
                    else if (target.ImagingGoals?.Any() == true)
                    {
                        // Fallback: use parent target's goals (base goals not synced yet) - apply RepeatCount
                        filterProgress = target.ImagingGoals
                            .Where(g => g.IsEnabled)
                            .GroupBy(g => g.Filter)
                            .ToDictionary(
                                group => group.Key,
                                group => new FilterProgress
                                {
                                    GoalMinutes = group.Sum(g => g.GoalTimeMinutes) * repeatCount,
                                    CompletedMinutes = group.Sum(g => g.CompletedTimeMinutes),
                                    ScheduledMinutes = 0,
                                    RemainingMinutes = (group.Sum(g => g.GoalTimeMinutes) * repeatCount) - group.Sum(g => g.CompletedTimeMinutes)
                                });
                    }
                    else
                    {
                        filterProgress = new Dictionary<ECameraFilter, FilterProgress>();
                    }
                    
                    var panelState = new TargetSchedulingState
                    {
                        Target = target,
                        PanelId = panel.Id,
                        PanelNumber = panel.PanelNumber,
                        IsMosaicPanel = true,
                        CurrentPriority = target.Priority,
                        TotalScheduledMinutes = 0,
                        ScheduledMinutesTonight = 0,
                        CurrentFilterIndex = 0,
                        CurrentBatchCount = 0,
                        FilterProgress = filterProgress
                    };

                    states[panel.Id] = panelState;
                }
            }
            else
            {
                // Regular target (non-mosaic) - apply RepeatCount to goals
                states[target.Id] = new TargetSchedulingState
                {
                    Target = target,
                    PanelId = null,
                    PanelNumber = null,
                    IsMosaicPanel = false,
                    CurrentPriority = target.Priority,
                    TotalScheduledMinutes = 0,
                    ScheduledMinutesTonight = 0,
                    CurrentFilterIndex = 0,
                    CurrentBatchCount = 0,
                    FilterProgress = target.ImagingGoals
                        .Where(g => g.IsEnabled)
                        .GroupBy(g => g.Filter)
                        .ToDictionary(
                            group => group.Key,
                            group => new FilterProgress
                            {
                                // Goal is multiplied by RepeatCount
                                GoalMinutes = group.Sum(g => g.GoalTimeMinutes) * repeatCount,
                                // Completed stays as actual progress
                                CompletedMinutes = group.Sum(g => g.CompletedTimeMinutes),
                                ScheduledMinutes = 0,
                                // Remaining = (Goal * RepeatCount) - Completed
                                RemainingMinutes = (group.Sum(g => g.GoalTimeMinutes) * repeatCount) - group.Sum(g => g.CompletedTimeMinutes)
                            })
                };
            }
        }

        return states;
    }

    private async Task<List<TargetObservableWindow>> CalculateObservableWindowsForNightAsync(
        List<ScheduledTargetDto> targets,
        DateTime date,
        AllTwilightTimesDto twilightTimes,
        ObservatoryDto observatory,
        SchedulerConfigurationDto configuration,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        CancellationToken cancellationToken)
    {
        var windows = new List<TargetObservableWindow>();
        // LEGACY SWAP: AASharp naming uses Dawn=evening and Dusk=morning, so we swap
        var nightStart = twilightTimes.Astronomical.Dawn!.Value; // Evening (swapped)
        var nightEnd = twilightTimes.Astronomical.Dusk!.Value;   // Morning (swapped)
        if (nightEnd < nightStart)
            nightEnd = nightEnd.AddDays(1);

        // Get moon position for the night (AASharp uses west-positive longitude)
        var moonPosition = await _astronomyService.GetMoonPositionAsync(
            observatory.Latitude, observatory.Longitude * -1, observatory.Elevation, nightStart);

        foreach (var target in targets)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // For mosaic targets, create windows for each panel
            if (target.IsMosaic && target.HasPanels)
            {
                foreach (var panel in target.Panels)
                {
                    var panelWindow = await CalculatePanelObservableWindowAsync(
                        target, panel, date, nightStart, nightEnd, observatory, configuration,
                        moonPosition, moonAvoidanceProfiles, cancellationToken);
                    
                    if (panelWindow != null)
                        windows.Add(panelWindow);
                }
            }
            else
            {
                // Regular target
                var window = await CalculateTargetObservableWindowAsync(
                    target, date, nightStart, nightEnd, observatory, configuration,
                    moonPosition, moonAvoidanceProfiles, cancellationToken);
                
                if (window != null)
                    windows.Add(window);
            }
        }

        return windows;
    }

    private async Task<TargetObservableWindow?> CalculateTargetObservableWindowAsync(
        ScheduledTargetDto target,
        DateTime date,
        DateTime nightStart,
        DateTime nightEnd,
        ObservatoryDto observatory,
        SchedulerConfigurationDto configuration,
        MoonPositionDto moonPositionAtNightStart,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        CancellationToken cancellationToken)
    {
        var window = new TargetObservableWindow
        {
            TargetId = target.Id,
            Target = target,
            PanelId = null,
            PanelNumber = null,
            Date = date,
            NightStart = nightStart,
            NightEnd = nightEnd,
            ObservablePeriods = new List<ObservablePeriod>()
        };

        // Check observability throughout the night - use 5-minute intervals for granular altitude/moon data
        var timeStep = TimeSpan.FromMinutes(5);
        var currentTime = nightStart;
        
        // Convert RA from hours (0-24) to degrees (0-360) for astronomy calculations
        var raInDegrees = target.RightAscension * 15.0;

        while (currentTime < nightEnd)
        {
            // Calculate altitude and azimuth
            var (altitude, azimuth) = await _astronomyService.CalculateAltitudeAzimuthAsync(
                raInDegrees, target.Declination,
                observatory.Latitude, observatory.Longitude, currentTime);

            // Apply atmospheric refraction correction for targets near horizon
            var refractedAltitude = altitude;
            if (altitude > -1 && altitude < 15)
            {
                var refraction = await _astronomyService.GetAtmosphericRefraction(altitude);
                refractedAltitude = altitude + refraction;
            }

            // Get effective minimum altitude (custom horizon or target-specific/config minimum)
            var targetMinAlt = GetEffectiveMinAltitude(target, configuration);
            var effectiveMinAltitude = observatory.HasCustomHorizon()
                ? Math.Max(observatory.GetHorizonAltitudeForAzimuth(azimuth), targetMinAlt)
                : targetMinAlt;

            // Check if above effective minimum altitude
            if (refractedAltitude >= effectiveMinAltitude)
            {
                // Update moon position for this time slot (moon moves ~0.5°/hour)
                // AASharp uses west-positive longitude convention
                var moonPosition = await _astronomyService.GetMoonPositionAsync(
                    observatory.Latitude, observatory.Longitude * -1, observatory.Elevation, currentTime);

                // Always calculate moon distance for display purposes
                var moonDistance = await _astronomyService.CalculateAngularDistanceAsync(
                    raInDegrees, target.Declination,
                    moonPosition.RightAscension, moonPosition.Declination);

                window.ObservablePeriods.Add(new ObservablePeriod
                {
                    StartTime = currentTime,
                    EndTime = currentTime.Add(timeStep),
                    Altitude = refractedAltitude,
                    Azimuth = azimuth,
                    MoonDistance = moonDistance,
                    MoonIllumination = moonPosition.IlluminatedFraction,
                    MoonAltitude = moonPosition.Altitude,
                    IsObservable = true
                });
            }

            currentTime = currentTime.Add(timeStep);
        }

        // Calculate total observable minutes
        window.TotalObservableMinutes = window.ObservablePeriods.Sum(p =>
            (p.EndTime - p.StartTime).TotalMinutes);

        return window.TotalObservableMinutes > 0 ? window : null;
    }

    private async Task<TargetObservableWindow?> CalculatePanelObservableWindowAsync(
        ScheduledTargetDto target,
        ScheduledTargetPanelDto panel,
        DateTime date,
        DateTime nightStart,
        DateTime nightEnd,
        ObservatoryDto observatory,
        SchedulerConfigurationDto configuration,
        MoonPositionDto moonPositionAtNightStart,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        CancellationToken cancellationToken)
    {
        var window = new TargetObservableWindow
        {
            TargetId = panel.Id, // Use panel ID as the scheduling unit
            Target = target,
            PanelId = panel.Id,
            PanelNumber = panel.PanelNumber,
            Date = date,
            NightStart = nightStart,
            NightEnd = nightEnd,
            ObservablePeriods = new List<ObservablePeriod>()
        };

        // Check observability throughout the night using PANEL coordinates - 5-minute intervals for granular data
        var timeStep = TimeSpan.FromMinutes(5);
        var currentTime = nightStart;
        
        // Convert panel RA from hours (0-24) to degrees (0-360) for astronomy calculations
        var panelRaInDegrees = panel.RaHours * 15.0;

        while (currentTime < nightEnd)
        {
            // Calculate altitude for PANEL coordinates (not target center)
            var (altitude, azimuth) = await _astronomyService.CalculateAltitudeAzimuthAsync(
                panelRaInDegrees, panel.DecDegrees,
                observatory.Latitude, observatory.Longitude, currentTime);

            // Apply atmospheric refraction correction for targets near horizon
            var refractedAltitude = altitude;
            if (altitude > -1 && altitude < 15)
            {
                var refraction = await _astronomyService.GetAtmosphericRefraction(altitude);
                refractedAltitude = altitude + refraction;
            }

            // Get effective minimum altitude (custom horizon or target-specific/config minimum)
            var targetMinAlt = GetEffectiveMinAltitude(target, configuration);
            var effectiveMinAltitude = observatory.HasCustomHorizon()
                ? Math.Max(observatory.GetHorizonAltitudeForAzimuth(azimuth), targetMinAlt)
                : targetMinAlt;

            // Check if above effective minimum altitude
            if (refractedAltitude >= effectiveMinAltitude)
            {
                // Update moon position for this time slot (moon moves ~0.5°/hour)
                // AASharp uses west-positive longitude convention
                var moonPosition = await _astronomyService.GetMoonPositionAsync(
                    observatory.Latitude, observatory.Longitude * -1, observatory.Elevation, currentTime);

                // Always calculate moon distance for display purposes
                var moonDistance = await _astronomyService.CalculateAngularDistanceAsync(
                    panelRaInDegrees, panel.DecDegrees,
                    moonPosition.RightAscension, moonPosition.Declination);

                window.ObservablePeriods.Add(new ObservablePeriod
                {
                    StartTime = currentTime,
                    EndTime = currentTime.Add(timeStep),
                    Altitude = refractedAltitude,
                    Azimuth = azimuth,
                    MoonDistance = moonDistance,
                    MoonIllumination = moonPosition.IlluminatedFraction,
                    MoonAltitude = moonPosition.Altitude,
                    IsObservable = true
                });
            }

            currentTime = currentTime.Add(timeStep);
        }

        // Calculate total observable minutes
        window.TotalObservableMinutes = window.ObservablePeriods.Sum(p =>
            (p.EndTime - p.StartTime).TotalMinutes);

        return window.TotalObservableMinutes > 0 ? window : null;
    }

    private (List<ScheduledSessionDto> Sessions, List<UnscheduledSlotDto> UnscheduledSlots) AllocateTimeSlots(
        List<TargetObservableWindow> observableWindows,
        Dictionary<Guid, TargetSchedulingState> targetStates,
        SchedulerConfigurationDto configuration,
        EquipmentDto equipment,
        DateTime date,
        AllTwilightTimesDto twilightTimes,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        double observatoryLongitude = 0,
        MeridianFlipSettingsDto? meridianFlipSettings = null,
        DateTime? startTimeOverride = null)
    {
        var sessions = new List<ScheduledSessionDto>();
        
        // Batch count PER PANEL (each mosaic panel has its own batch counter)
        // Use double for fractional tracking to ensure we hit exact batch sizes
        // Initialize with existing progress (WIP targets) - calculate from total exposures taken
        var panelBatchCounts = new Dictionary<Guid, double>();
        foreach (var state in targetStates.Values)
        {
            var panelKey = GetPanelBatchKey(state);
            if (!panelBatchCounts.ContainsKey(panelKey))
            {
                // Calculate existing batch count from total exposures already taken
                // This ensures WIP targets continue from where they left off
                int totalExposuresTaken = state.Target.ImagingGoals?
                    .Where(g => g.IsEnabled)
                    .Sum(g => g.CompletedExposures) ?? 0;
                
                if (totalExposuresTaken > 0)
                {
                    panelBatchCounts[panelKey] = totalExposuresTaken;
                    _logger.LogDebug("WIP target '{Target}' P{Panel}: Starting batch count at {Count} (existing exposures)",
                        state.Target.Name, state.PanelNumber, totalExposuresTaken);
                }
            }
        }

        // Reset tonight's scheduled time
        foreach (var state in targetStates.Values)
        {
            state.ScheduledMinutesTonight = 0;
        }
        
        // Log meridian flip settings if enabled
        if (meridianFlipSettings?.Enabled == true)
        {
            _logger.LogInformation("Meridian flip enabled: {MinAfter}min after meridian, {PauseBefore}min pause before flip, {MaxToMeridian}min max to meridian",
                meridianFlipSettings.MinutesAfterMeridian, meridianFlipSettings.PauseTimeBeforeFlipMinutes, meridianFlipSettings.MaxMinutesToMeridian);
        }

        // Use 5-minute slots for frequent re-evaluation of which target is best
        // MinSessionDuration only filters short sessions AFTER scheduling, not slot granularity
        var slotInterval = TimeSpan.FromMinutes(5);
        
        // Extract twilight times for slot generation and filter blocking
        // LEGACY SWAP: AASharp naming uses Dawn=evening and Dusk=morning, so we swap
        var astronomicalDusk = twilightTimes.Astronomical.Dawn!.Value; // Evening start (swapped)
        var astronomicalDawn = twilightTimes.Astronomical.Dusk!.Value; // Morning end (swapped)
        if (astronomicalDawn < astronomicalDusk)
            astronomicalDawn = astronomicalDawn.AddDays(1);
        
        // Nautical twilight times (earlier evening, later morning)
        // LEGACY SWAP: Same swap as astronomical
        DateTime? nauticalDusk = twilightTimes.Nautical.Dawn; // Evening start (swapped)
        DateTime? nauticalDawn = twilightTimes.Nautical.Dusk; // Morning end (swapped)
        if (nauticalDusk.HasValue && nauticalDawn.HasValue && nauticalDawn.Value < nauticalDusk.Value)
            nauticalDawn = nauticalDawn.Value.AddDays(1);
        
        // Extend slot generation to include nautical twilight if we have nautical times
        // This allows filters with AcceptableTwilight=Nautical to be scheduled during nautical twilight
        var slotStart = nauticalDusk ?? astronomicalDusk;
        var slotEnd = nauticalDawn ?? astronomicalDawn;
        
        // Apply start time override if provided (e.g., "Now" functionality)
        if (startTimeOverride.HasValue && startTimeOverride.Value > slotStart && startTimeOverride.Value < slotEnd)
        {
            _logger.LogInformation("AllocateTimeSlots: Applying start time override from {Original:HH:mm} to {Override:HH:mm}",
                slotStart, startTimeOverride.Value);
            slotStart = startTimeOverride.Value;
        }
        
        var timeSlots = GenerateTimeSlots(slotStart, slotEnd, slotInterval);
        
        // Track previous target for switching penalty in AltitudeFirst strategy
        Guid? previousTargetId = null;
        
        // Track unscheduled slots with reasons
        var unscheduledSlots = new List<UnscheduledSlotDto>();
        
        _logger.LogInformation("AllocateTimeSlots: Generated {SlotCount} slots from {Start:HH:mm} to {End:HH:mm}", 
            timeSlots.Count, slotStart, slotEnd);
        
        // Debug: Check first slot against first window's periods
        if (timeSlots.Any() && observableWindows.Any())
        {
            var firstSlot = timeSlots.First();
            var firstWindow = observableWindows.First();
            _logger.LogInformation("First slot: {Start:HH:mm}-{End:HH:mm}, First window '{Target}' has {PeriodCount} periods",
                firstSlot.Start, firstSlot.End, firstWindow.Target?.Name, firstWindow.ObservablePeriods.Count);
            if (firstWindow.ObservablePeriods.Any())
            {
                var firstPeriod = firstWindow.ObservablePeriods.First();
                _logger.LogInformation("  First period: {Start:HH:mm}-{End:HH:mm}, IsObservable={IsObs}",
                    firstPeriod.StartTime, firstPeriod.EndTime, firstPeriod.IsObservable);
            }
        }

        foreach (var slot in timeSlots)
        {
            // Find observable targets for this slot (check for overlap, not containment)
            var observableTargets = observableWindows
                .Where(w => w.ObservablePeriods.Any(p =>
                    p.StartTime < slot.End && p.EndTime > slot.Start && p.IsObservable))
                .Select(w => w.TargetId)
                .ToList();

            if (!observableTargets.Any())
            {
                _logger.LogDebug("Slot {Start:HH:mm}-{End:HH:mm}: No observable targets", slot.Start, slot.End);
                unscheduledSlots.Add(new UnscheduledSlotDto
                {
                    StartTimeUtc = slot.Start,
                    EndTimeUtc = slot.End,
                    Reason = "No observable targets"
                });
                continue;
            }

            // Update blocked filters and current period for each observable target at this time slot
            foreach (var targetId in observableTargets)
            {
                var state = targetStates[targetId];
                var targetWindow = observableWindows.First(w => w.TargetId == targetId);
                var targetPeriod = targetWindow.ObservablePeriods.FirstOrDefault(p =>
                    p.StartTime < slot.End && p.EndTime > slot.Start && p.IsObservable);
                
                // Store current period for scoring calculations
                state.CurrentPeriod = targetPeriod;
                
                if (targetPeriod != null)
                {
                    // Calculate blocked filters based on twilight, altitude, and moon avoidance requirements
                    state.BlockedFilters = CalculateBlockedFilters(
                        state.Target,
                        targetPeriod.MoonDistance,
                        targetPeriod.MoonIllumination,
                        targetPeriod.MoonAltitude,
                        targetPeriod.Altitude, // Current target altitude
                        GetEffectiveMinAltitude(state.Target, configuration), // Target-specific minimum altitude
                        slot.Start, // Current slot time for twilight check
                        nauticalDusk,
                        nauticalDawn,
                        astronomicalDusk,
                        astronomicalDawn,
                        GetEffectiveUseMoonAvoidance(state.Target, configuration) ? moonAvoidanceProfiles : new List<UserFilterMoonAvoidanceProfileDto>());
                    
                    if (state.BlockedFilters.Any())
                    {
                        _logger.LogInformation("Slot {Time:HH:mm} [{Target}] P{Panel}: Alt={Alt:F1}° MoonAlt={MoonAlt:F1}° MoonDist={MoonDist:F1}° - BLOCKED: {Filters}",
                            slot.Start, state.Target.Name, state.PanelNumber, targetPeriod.Altitude, targetPeriod.MoonAltitude, 
                            targetPeriod.MoonDistance, string.Join(", ", state.BlockedFilters));
                    }
                }
                else
                {
                    state.BlockedFilters = new List<ECameraFilter>();
                }
            }
            
            // Calculate priorities for observable targets
            var targetPriorities = observableTargets
                .Select(targetId => new
                {
                    TargetId = targetId,
                    State = targetStates[targetId],
                    Priority = CalculatePriorityScore(targetStates[targetId], configuration, slot.Start)
                })
                .OrderByDescending(t => t.Priority)
                .ToList();

            // Select highest priority target that hasn't exceeded constraints
            TargetSchedulingState? selectedState = null;
            
            // Track targets that failed to produce segments (for retry with next target)
            var failedTargetsThisSlot = new HashSet<Guid>();
            
            // AltitudeFirst switching penalty: only switch if new target is 2°+ higher and not declining faster
            var previousState = previousTargetId.HasValue && targetStates.ContainsKey(previousTargetId.Value) 
                ? targetStates[previousTargetId.Value] 
                : null;
            var applyAltitudeSwitchingPenalty = configuration.PrimaryStrategy == TargetSelectionStrategy.AltitudeFirst 
                && previousState?.CurrentPeriod != null;
            
            // Log top 3 targets by priority for debugging AltitudeFirst
            if (targetPriorities.Count >= 2)
            {
                var top = targetPriorities.Take(3).ToList();
                _logger.LogInformation("Slot {Start:HH:mm}: Top targets: {Targets}",
                    slot.Start,
                    string.Join(", ", top.Select(t => $"{t.State.Target.Name} P{t.State.PanelNumber}={t.Priority:F0} alt={t.State.CurrentPeriod?.Altitude:F1}°")));
            }

            // Retry target selection loop - if selected target produces 0 segments, try next target
            // Safety: limit retries to number of observable targets to prevent infinite loops
            while (failedTargetsThisSlot.Count < observableTargets.Count)
            {
            selectedState = null;
            
            foreach (var tp in targetPriorities)
            {
                var state = tp.State;
                var targetKey = state.PanelId ?? state.Target.Id;
                
                // Skip targets that already failed to produce segments this slot
                if (failedTargetsThisSlot.Contains(targetKey))
                    continue;

                // Check constraints - use target-specific max hours per night
                var targetMaxHoursPerNight = GetEffectiveMaxHoursPerNight(state.Target, configuration);
                if (targetMaxHoursPerNight > 0 &&
                    state.ScheduledMinutesTonight >= targetMaxHoursPerNight * 60)
                    continue;

                // MaxTotalHoursPerTarget removed - not needed for scheduler

                // Check if target has incomplete goals
                if (!state.FilterProgress.Values.Any(fp => fp.RemainingMinutes > 0))
                    continue;
                
                // Check if target has at least one unblocked filter with remaining time
                // Skip targets where ALL remaining filters are blocked by moon
                var filtersWithRemainingTime = state.FilterProgress
                    .Where(fp => fp.Value.RemainingMinutes > 0)
                    .Select(fp => fp.Key)
                    .ToList();
                var unblockedFilters = filtersWithRemainingTime
                    .Where(f => !state.BlockedFilters.Contains(f))
                    .ToList();
                if (!unblockedFilters.Any())
                {
                    _logger.LogInformation("Slot {Start:HH:mm}: Skipping '{Target}' P{Panel} - all {Count} remaining filters blocked: {Blocked}",
                        slot.Start, state.Target.Name, state.PanelNumber, filtersWithRemainingTime.Count, 
                        string.Join(", ", state.BlockedFilters.Intersect(filtersWithRemainingTime)));
                    continue;
                }
                
                // Check meridian flip constraints
                if (meridianFlipSettings?.Enabled == true)
                {
                    // Get panel-specific RA if available, otherwise use target RA
                    var panel = state.IsMosaicPanel && state.Target.Panels?.Any() == true
                        ? state.Target.Panels.FirstOrDefault(p => p.Id == state.PanelId)
                        : null;
                    var panelRa = panel?.RaHours;
                    var targetRa = panelRa ?? state.Target.RightAscension;
                    
                    var inFlip = IsInMeridianFlipWindow(slot.Start, slot.End, targetRa, observatoryLongitude, meridianFlipSettings);
                    if (inFlip)
                    {
                        _logger.LogInformation("Slot {Start:HH:mm}-{End:HH:mm}: Target '{Target}' P{Panel} (RA={RA:F2}h, PanelRA={PanelRA}) SKIPPED - in meridian flip window",
                            slot.Start, slot.End, state.Target.Name, state.PanelNumber, targetRa, panelRa.HasValue ? $"{panelRa:F2}h" : "null");
                        continue;
                    }
                }
                
                // AltitudeFirst switching penalty: don't switch unless new target is 2°+ higher
                var currentTargetKey = state.PanelId ?? state.Target.Id;
                if (applyAltitudeSwitchingPenalty && previousTargetId.HasValue && currentTargetKey != previousTargetId.Value)
                {
                    var prevAlt = previousState!.CurrentPeriod!.Altitude;
                    var newAlt = state.CurrentPeriod?.Altitude ?? 0;
                    var altDiff = newAlt - prevAlt;
                    
                    // Check if new target is at least 2° higher
                    if (altDiff < 2.0)
                    {
                        // Check if previous target is still observable, passes other constraints, AND hasn't failed this slot
                        var prevTargetKey = previousState.PanelId ?? previousState.Target.Id;
                        
                        // Also check if previous target is in meridian flip window
                        var prevInFlip = false;
                        if (meridianFlipSettings?.Enabled == true)
                        {
                            var prevPanel = previousState.IsMosaicPanel && previousState.Target.Panels?.Any() == true
                                ? previousState.Target.Panels.FirstOrDefault(p => p.Id == previousState.PanelId)
                                : null;
                            var prevRa = prevPanel?.RaHours ?? previousState.Target.RightAscension;
                            prevInFlip = IsInMeridianFlipWindow(slot.Start, slot.End, prevRa, observatoryLongitude, meridianFlipSettings);
                        }
                        
                        // Use target-specific max hours for previous target
                        var prevMaxHoursPerNight = GetEffectiveMaxHoursPerNight(previousState.Target, configuration);
                        var prevTargetStillValid = !prevInFlip &&
                            observableTargets.Contains(previousTargetId.Value) &&
                            !failedTargetsThisSlot.Contains(prevTargetKey) &&
                            previousState.FilterProgress.Values.Any(fp => fp.RemainingMinutes > 0) &&
                            (prevMaxHoursPerNight <= 0 || previousState.ScheduledMinutesTonight < prevMaxHoursPerNight * 60);
                        
                        if (prevTargetStillValid)
                        {
                            // Don't switch - select previous target instead
                            _logger.LogInformation("AltitudeFirst: Staying on '{Prev}' P{PrevPanel} ({PrevAlt:F1}°) instead of '{New}' ({NewAlt:F1}°) - only {Diff:F1}° higher",
                                previousState.Target.Name, previousState.PanelNumber, prevAlt, state.Target.Name, newAlt, altDiff);
                            selectedState = previousState;
                            break;
                        }
                    }
                    
                    _logger.LogInformation("AltitudeFirst: Switching from '{Prev}' ({PrevAlt:F1}°) to '{New}' ({NewAlt:F1}°) - {Diff:F1}° higher",
                        previousState.Target.Name, prevAlt, state.Target.Name, newAlt, altDiff);
                }

                selectedState = state;
                break;
            }

            // If no target passed pre-checks, break to handle after while loop
            if (selectedState == null)
                break;
            
            _logger.LogInformation("Slot {Start:HH:mm}-{End:HH:mm}: Selected '{Target}' P{Panel} for scheduling (BlockedFilters={Blocked})",
                slot.Start, slot.End, selectedState.Target.Name, selectedState.PanelNumber,
                selectedState.BlockedFilters?.Any() == true ? string.Join(",", selectedState.BlockedFilters) : "none");

            // Get common data for session creation
            var windowTargetId = selectedState.PanelId ?? selectedState.Target.Id;
            var window = observableWindows.First(w => w.TargetId == windowTargetId);
            var period = window.ObservablePeriods.First(p =>
                p.StartTime < slot.End && p.EndTime > slot.Start && p.IsObservable);

            // Get panel coordinates if this is a mosaic panel
            double? panelCenterRA = null;
            double? panelCenterDec = null;
            if (selectedState.PanelId.HasValue && selectedState.Target.Panels?.Any() == true)
            {
                var panel = selectedState.Target.Panels.FirstOrDefault(p => p.Id == selectedState.PanelId.Value);
                if (panel != null)
                {
                    panelCenterRA = panel.RaHours;
                    panelCenterDec = panel.DecDegrees;
                }
            }

            // Get filter shooting method - use target-specific override → template → config
            var filterShootMethod = GetEffectiveFilterShootingPattern(selectedState.Target, configuration);
            var effectiveBatchSize = GetEffectiveFilterBatchSize(selectedState.Target, configuration);
            
            int? batchSizeForSession = filterShootMethod == "Batch" && effectiveBatchSize > 0 
                ? effectiveBatchSize : null;

            var efficiency = configuration.ImagingEfficiencyPercent;

            // For Batch pattern, calculate segments with mid-slot filter switching
            if (filterShootMethod == "Batch")
            {
                var segments = CalculateBatchSegments(selectedState, slot.Start, slot.End, configuration, panelBatchCounts);
                
                if (!segments.Any())
                {
                    _logger.LogDebug("Slot {Start:HH:mm}-{End:HH:mm}: No batch segments generated for '{Target}' P{Panel} - trying next target",
                        slot.Start, slot.End, selectedState.Target.Name, selectedState.PanelNumber);
                    
                    // Mark this target as failed for this slot and retry with next target
                    var failedKey = selectedState.PanelId ?? selectedState.Target.Id;
                    failedTargetsThisSlot.Add(failedKey);
                    continue; // Continue while loop to try next target
                }

                foreach (var segment in segments)
                {
                    var segmentDuration = (segment.End - segment.Start).TotalMinutes;
                    var plannedDuration = segmentDuration * (efficiency / 100.0);

                    // Try to extend existing session with same filter
                    var existingSession = sessions.LastOrDefault(s =>
                        s.ScheduledTargetId == selectedState.Target.Id &&
                        s.PanelId == selectedState.PanelId &&
                        s.Filter == segment.Filter &&
                        s.EndTimeUtc == segment.Start);

                    if (existingSession != null)
                    {
                        existingSession.EndTimeUtc = segment.End;
                        existingSession.PlannedDurationMinutes += plannedDuration;
                    }
                    else
                    {
                        // Get required moon distance for this filter
                        double? requiredMoonDistance = null;
                        if (moonAvoidanceProfiles?.Any() == true)
                        {
                            var filterProfile = moonAvoidanceProfiles.FirstOrDefault(p => p.Filter == segment.Filter);
                            if (filterProfile?.MoonAvoidanceProfile != null)
                            {
                                requiredMoonDistance = filterProfile.MoonAvoidanceProfile.CalculateAvoidanceDistance(period.MoonIllumination);
                            }
                        }

                        sessions.Add(new ScheduledSessionDto
                        {
                            Id = Guid.NewGuid(),
                            ScheduledTargetId = selectedState.Target.Id,
                            PanelId = selectedState.PanelId,
                            PanelNumber = selectedState.PanelNumber,
                            PanelCenterRA = panelCenterRA,
                            PanelCenterDec = panelCenterDec,
                            SessionDate = date,
                            StartTimeUtc = segment.Start,
                            EndTimeUtc = segment.End,
                            Filter = segment.Filter,
                            PlannedDurationMinutes = plannedDuration,
                            PlannedExposures = segment.Exposures,
                            FilterSegments = $"{segment.Filter}:{segment.Exposures}",
                            Status = "Planned",
                            IsManualOverride = false,
                            FilterShootMethod = filterShootMethod,
                            BatchSize = batchSizeForSession,
                            MoonDistance = period.MoonDistance,
                            MoonIllumination = period.MoonIllumination,
                            RequiredMoonDistance = requiredMoonDistance
                        });
                    }

                    // Update filter progress
                    if (selectedState.FilterProgress.ContainsKey(segment.Filter))
                    {
                        selectedState.FilterProgress[segment.Filter].ScheduledMinutes += plannedDuration;
                        selectedState.FilterProgress[segment.Filter].RemainingMinutes -= plannedDuration;
                    }
                    
                    // Note: Batch count is updated directly in CalculateBatchSegments with fractional tracking
                }

                // Update overall state
                var slotDuration = (slot.End - slot.Start).TotalMinutes;
                selectedState.ScheduledMinutesTonight += slotDuration;
                selectedState.TotalScheduledMinutes += slotDuration;
                break; // Successfully scheduled - exit while loop
            }
            else
            {
                // Non-Batch patterns: Loop, Complete, etc. - use original logic
                var filter = DetermineNextFilter(selectedState, configuration, panelBatchCounts);

                if (filter == null)
                {
                    _logger.LogDebug("Slot {Start:HH:mm}-{End:HH:mm}: Selected '{Target}' P{Panel} but no filter available - trying next target",
                        slot.Start, slot.End, selectedState.Target.Name, selectedState.PanelNumber);
                    
                    // Mark this target as failed for this slot and retry with next target
                    var failedKey = selectedState.PanelId ?? selectedState.Target.Id;
                    failedTargetsThisSlot.Add(failedKey);
                    continue; // Continue while loop to try next target
                }

                var slotDuration = (slot.End - slot.Start).TotalMinutes;
                var plannedDuration = slotDuration * (efficiency / 100.0);

                // Try to extend existing session
                var existingSession = sessions.LastOrDefault(s =>
                    s.ScheduledTargetId == selectedState.Target.Id &&
                    s.PanelId == selectedState.PanelId &&
                    s.Filter == filter.Value &&
                    s.EndTimeUtc == slot.Start);

                if (existingSession != null)
                {
                    existingSession.EndTimeUtc = slot.End;
                    existingSession.PlannedDurationMinutes += plannedDuration;
                }
                else
                {
                    // Get required moon distance
                    double? requiredMoonDistance = null;
                    if (moonAvoidanceProfiles?.Any() == true)
                    {
                        var filterProfile = moonAvoidanceProfiles.FirstOrDefault(p => p.Filter == filter.Value);
                        if (filterProfile?.MoonAvoidanceProfile != null)
                        {
                            requiredMoonDistance = filterProfile.MoonAvoidanceProfile.CalculateAvoidanceDistance(period.MoonIllumination);
                        }
                    }

                    // For non-batch patterns, calculate exposures
                    int exposureTimeSec = 300;
                    var imagingGoal = selectedState.Target.ImagingGoals?.FirstOrDefault(g => g.Filter == filter.Value && g.IsEnabled);
                    if (imagingGoal != null)
                    {
                        exposureTimeSec = imagingGoal.ExposureTimeSeconds > 0 ? imagingGoal.ExposureTimeSeconds : 300;
                    }
                    int exposures = Math.Max(1, (int)(plannedDuration * 60 / exposureTimeSec));
                    
                    sessions.Add(new ScheduledSessionDto
                    {
                        Id = Guid.NewGuid(),
                        ScheduledTargetId = selectedState.Target.Id,
                        PanelId = selectedState.PanelId,
                        PanelNumber = selectedState.PanelNumber,
                        PanelCenterRA = panelCenterRA,
                        PanelCenterDec = panelCenterDec,
                        SessionDate = date,
                        StartTimeUtc = slot.Start,
                        EndTimeUtc = slot.End,
                        Filter = filter.Value,
                        PlannedDurationMinutes = plannedDuration,
                        PlannedExposures = exposures,
                        FilterSegments = $"{filter.Value}:{exposures}",
                        Status = "Planned",
                        IsManualOverride = false,
                        FilterShootMethod = filterShootMethod,
                        BatchSize = batchSizeForSession,
                        MoonDistance = period.MoonDistance,
                        MoonIllumination = period.MoonIllumination,
                        RequiredMoonDistance = requiredMoonDistance
                    });
                    
                    UpdateFilterPatternState(selectedState, configuration, filter.Value, exposures, panelBatchCounts);
                }

                // Update state
                selectedState.ScheduledMinutesTonight += slotDuration;
                selectedState.TotalScheduledMinutes += slotDuration;

                if (selectedState.FilterProgress.ContainsKey(filter.Value))
                {
                    selectedState.FilterProgress[filter.Value].ScheduledMinutes += plannedDuration;
                    selectedState.FilterProgress[filter.Value].RemainingMinutes -= plannedDuration;
                }
                break; // Successfully scheduled - exit while loop
            }
            } // End while loop
            
            // If we exhausted all targets without scheduling, mark as unscheduled
            if (selectedState == null)
            {
                // Build reason for why no target could be scheduled
                var reasons = new List<string>();
                foreach (var targetId in observableTargets)
                {
                    var state = targetStates[targetId];
                    var targetKey = state.PanelId ?? state.Target.Id;
                    var filtersWithTime = state.FilterProgress.Where(fp => fp.Value.RemainingMinutes > 0).Select(fp => fp.Key).ToList();
                    
                    if (failedTargetsThisSlot.Contains(targetKey))
                        reasons.Add($"{state.Target.Name} P{state.PanelNumber}: no usable filters");
                    else if (!filtersWithTime.Any())
                        reasons.Add($"{state.Target.Name}: goals complete");
                    else if (state.BlockedFilters.Intersect(filtersWithTime).Count() == filtersWithTime.Count)
                        reasons.Add($"{state.Target.Name}: all filters blocked");
                }
                var reason = reasons.Any() ? string.Join("; ", reasons.Take(3)) : "All targets skipped";
                
                _logger.LogInformation("Slot {Start:HH:mm}-{End:HH:mm}: NO TARGET SCHEDULED after trying {Failed} targets",
                    slot.Start, slot.End, failedTargetsThisSlot.Count);
                unscheduledSlots.Add(new UnscheduledSlotDto
                {
                    StartTimeUtc = slot.Start,
                    EndTimeUtc = slot.End,
                    Reason = reason
                });
                continue;
            }
            
            // Update previous target for next slot's switching penalty calculation
            previousTargetId = selectedState.PanelId ?? selectedState.Target.Id;
        }

        // Merge consecutive sessions on the same target/panel into a single session
        // A session represents continuous imaging time on a target, regardless of filter switches
        // Exposure counts are calculated from TOTAL merged duration with efficiency applied
        var batchSize = configuration.FilterBatchSize > 0 ? configuration.FilterBatchSize : 10;
        sessions = MergeConsecutiveSessions(sessions, targetStates, configuration.ImagingEfficiencyPercent, batchSize);
        
        // Filter out TARGET BLOCKS shorter than minimum duration
        // A target block is all consecutive sessions on the same target (regardless of filter)
        // This ensures we don't filter out individual filter segments within a valid target session
        if (configuration.MinSessionDurationMinutes > 0)
        {
            var beforeCount = sessions.Count;
            var orderedSessions = sessions.OrderBy(s => s.StartTimeUtc).ToList();
            var filteredSessions = new List<ScheduledSessionDto>();
            
            // Group consecutive sessions by target (allowing 1 min gaps for batch boundary rounding)
            var currentBlock = new List<ScheduledSessionDto>();
            
            foreach (var session in orderedSessions)
            {
                if (currentBlock.Count == 0)
                {
                    currentBlock.Add(session);
                }
                else
                {
                    var lastInBlock = currentBlock.Last();
                    // Same target and adjacent in time?
                    if (lastInBlock.ScheduledTargetId == session.ScheduledTargetId &&
                        lastInBlock.PanelId == session.PanelId &&
                        (session.StartTimeUtc - lastInBlock.EndTimeUtc).TotalMinutes <= 1)
                    {
                        currentBlock.Add(session);
                    }
                    else
                    {
                        // Different target or gap - process current block
                        ProcessTargetBlock(currentBlock, filteredSessions, configuration.MinSessionDurationMinutes, targetStates);
                        currentBlock = new List<ScheduledSessionDto> { session };
                    }
                }
            }
            
            // Process final block
            if (currentBlock.Count > 0)
            {
                ProcessTargetBlock(currentBlock, filteredSessions, configuration.MinSessionDurationMinutes, targetStates);
            }
            
            sessions = filteredSessions;
            
            if (sessions.Count < beforeCount)
            {
                _logger.LogWarning("MinSessionDuration filter removed {Removed} sessions (< {Min}min continuous on target).",
                    beforeCount - sessions.Count, configuration.MinSessionDurationMinutes);
            }
        }

        // MaxSequenceTimeMinutes removed - not useful for scheduling

        return (sessions, unscheduledSlots);
    }

    private List<TimeSlot> GenerateTimeSlots(DateTime start, DateTime end, TimeSpan interval)
    {
        var slots = new List<TimeSlot>();
        var current = start;

        while (current < end)
        {
            var slotEnd = current.Add(interval);
            if (slotEnd > end)
                slotEnd = end;

            slots.Add(new TimeSlot { Start = current, End = slotEnd });
            current = slotEnd;
        }

        return slots;
    }

    /// <summary>
    /// Merge consecutive unscheduled slots with the same reason into larger time blocks
    /// </summary>
    private List<UnscheduledSlotDto> MergeUnscheduledSlots(List<UnscheduledSlotDto> slots)
    {
        if (slots.Count <= 1)
            return slots;
            
        var merged = new List<UnscheduledSlotDto>();
        var orderedSlots = slots.OrderBy(s => s.StartTimeUtc).ToList();
        
        UnscheduledSlotDto? current = null;
        foreach (var slot in orderedSlots)
        {
            if (current == null)
            {
                current = new UnscheduledSlotDto
                {
                    StartTimeUtc = slot.StartTimeUtc,
                    EndTimeUtc = slot.EndTimeUtc,
                    Reason = slot.Reason
                };
            }
            else if (slot.StartTimeUtc <= current.EndTimeUtc.AddMinutes(1) && slot.Reason == current.Reason)
            {
                // Extend current slot
                current.EndTimeUtc = slot.EndTimeUtc;
            }
            else
            {
                // Save current and start new
                merged.Add(current);
                current = new UnscheduledSlotDto
                {
                    StartTimeUtc = slot.StartTimeUtc,
                    EndTimeUtc = slot.EndTimeUtc,
                    Reason = slot.Reason
                };
            }
        }
        
        if (current != null)
            merged.Add(current);
            
        return merged;
    }

    /// <summary>
    /// Merge consecutive sessions on the same target/panel into a single session.
    /// A session represents continuous imaging time on a target, regardless of filter switches.
    /// Filter changes happen WITHIN a session based on batch logic, not as separate sessions.
    /// </summary>
    private List<ScheduledSessionDto> MergeConsecutiveSessions(
        List<ScheduledSessionDto> sessions,
        Dictionary<Guid, TargetSchedulingState> targetStates,
        double efficiencyPercent,
        int batchSize)
    {
        if (sessions.Count <= 1)
            return sessions;
            
        var orderedSessions = sessions.OrderBy(s => s.StartTimeUtc).ToList();
        var mergedSessions = new List<ScheduledSessionDto>();
        
        // Track cumulative exposures PER PANEL across all merged sessions in this night
        // This ensures when a panel is scheduled again later, we continue from where we left off
        var panelCumulativeExposures = new Dictionary<Guid, int>();
        
        ScheduledSessionDto? currentMerged = null;
        // Track filter durations (in minutes) for each merged session
        var currentFilterDurations = new Dictionary<ECameraFilter, double>();
        // Track filter sequence in chronological order: (filter, duration)
        var currentFilterSequence = new List<(ECameraFilter Filter, double DurationMinutes)>();
        
        foreach (var session in orderedSessions)
        {
            if (currentMerged == null)
            {
                // Start a new merged session
                currentMerged = CloneSession(session);
                currentMerged.FilterSegments = null; // Will be recalculated
                currentMerged.PlannedExposures = 0; // Will be recalculated
                currentFilterDurations.Clear();
                currentFilterDurations[session.Filter] = session.PlannedDurationMinutes;
                currentFilterSequence.Clear();
                currentFilterSequence.Add((session.Filter, session.PlannedDurationMinutes));
            }
            else if (currentMerged.ScheduledTargetId == session.ScheduledTargetId &&
                     currentMerged.PanelId == session.PanelId &&
                     (session.StartTimeUtc - currentMerged.EndTimeUtc).TotalMinutes <= 1)
            {
                // Extend the current merged session (same target/panel, adjacent or near-adjacent time)
                currentMerged.EndTimeUtc = session.EndTimeUtc;
                currentMerged.PlannedDurationMinutes += session.PlannedDurationMinutes;
                
                // Track filter duration (not exposure count)
                if (currentFilterDurations.ContainsKey(session.Filter))
                    currentFilterDurations[session.Filter] += session.PlannedDurationMinutes;
                else
                    currentFilterDurations[session.Filter] = session.PlannedDurationMinutes;
                
                // Track filter sequence - extend last segment if same filter, otherwise add new
                if (currentFilterSequence.Count > 0 && currentFilterSequence[^1].Filter == session.Filter)
                {
                    var last = currentFilterSequence[^1];
                    currentFilterSequence[^1] = (last.Filter, last.DurationMinutes + session.PlannedDurationMinutes);
                }
                else
                {
                    currentFilterSequence.Add((session.Filter, session.PlannedDurationMinutes));
                }
            }
            else
            {
                // Finalize current merged session - calculate exposures from total duration
                // Pass the starting batch offset for this panel
                var panelKey = currentMerged.PanelId ?? currentMerged.ScheduledTargetId;
                var startingBatchOffset = panelCumulativeExposures.GetValueOrDefault(panelKey, 0);
                var exposuresAdded = FinalizeSessionExposures(currentMerged, currentFilterSequence, targetStates, efficiencyPercent, batchSize, startingBatchOffset);
                panelCumulativeExposures[panelKey] = startingBatchOffset + exposuresAdded;
                mergedSessions.Add(currentMerged);
                
                // Start new merged session
                currentMerged = CloneSession(session);
                currentMerged.FilterSegments = null;
                currentMerged.PlannedExposures = 0;
                currentFilterDurations.Clear();
                currentFilterDurations[session.Filter] = session.PlannedDurationMinutes;
                currentFilterSequence.Clear();
                currentFilterSequence.Add((session.Filter, session.PlannedDurationMinutes));
            }
        }
        
        // Don't forget the last merged session
        if (currentMerged != null)
        {
            var panelKey = currentMerged.PanelId ?? currentMerged.ScheduledTargetId;
            var startingBatchOffset = panelCumulativeExposures.GetValueOrDefault(panelKey, 0);
            var exposuresAdded = FinalizeSessionExposures(currentMerged, currentFilterSequence, targetStates, efficiencyPercent, batchSize, startingBatchOffset);
            panelCumulativeExposures[panelKey] = startingBatchOffset + exposuresAdded;
            mergedSessions.Add(currentMerged);
        }
        
        var beforeCount = sessions.Count;
        _logger.LogInformation("MergedSessions: {Before} sessions → {After} sessions",
            beforeCount, mergedSessions.Count);
        
        // Log final merged session details for debugging
        foreach (var merged in mergedSessions)
        {
            // For mosaics, targetStates uses PanelId as key; for non-mosaics, it uses TargetId
            var lookupKey = merged.PanelId ?? merged.ScheduledTargetId;
            var targetName = targetStates.TryGetValue(lookupKey, out var st) ? st.Target.Name : "?";
            _logger.LogInformation("MergedSession: '{Target}' P{Panel} {Start:HH:mm}-{End:HH:mm} ({Duration:F0}min) FilterSegments='{Segments}' PlannedExp={Exp}",
                targetName, merged.PanelNumber, merged.StartTimeUtc, merged.EndTimeUtc, 
                merged.PlannedDurationMinutes, merged.FilterSegments, merged.PlannedExposures);
        }
        
        return mergedSessions;
    }
    
    /// <summary>
    /// Calculate exposures for a merged session based on ACTUAL per-slot filter durations.
    /// Uses the filter sequence in CHRONOLOGICAL ORDER from individual slots (which respect moon avoidance).
    /// Returns the number of exposures added to this session.
    /// </summary>
    private int FinalizeSessionExposures(
        ScheduledSessionDto session,
        List<(ECameraFilter Filter, double DurationMinutes)> filterSequence,
        Dictionary<Guid, TargetSchedulingState> targetStates,
        double efficiencyPercent,
        int batchSize = 10,
        int startingBatchOffset = 0)
    {
        var efficiency = efficiencyPercent / 100.0;
        
        // Get target state for exposure times
        targetStates.TryGetValue(session.ScheduledTargetId, out var targetState);
        
        if (filterSequence.Count == 0)
        {
            session.FilterSegments = "";
            session.PlannedExposures = 0;
            return 0;
        }
        
        // Calculate exposures PER SEGMENT in CHRONOLOGICAL ORDER
        // This preserves the actual order filters were used (respects moon avoidance timing)
        // NOTE: durationMinutes from filterSequence is already efficiency-adjusted (PlannedDurationMinutes)
        // so we do NOT multiply by efficiency again here
        var segmentStrings = new List<string>();
        int totalExposures = 0;
        
        foreach (var (filter, durationMinutes) in filterSequence)
        {
            // durationMinutes is already the effective imaging time (efficiency already applied)
            // Get exposure time for this filter
            var goal = targetState?.Target.ImagingGoals?.FirstOrDefault(g => g.Filter == filter && g.IsEnabled);
            var exposureTimeMin = (goal?.ExposureTimeSeconds ?? 300) / 60.0;
            
            var exposures = (int)Math.Floor(durationMinutes / exposureTimeMin);
            if (exposures > 0)
            {
                segmentStrings.Add($"{filter}:{exposures}");
                totalExposures += exposures;
            }
        }
        
        // FilterSegments is now in CHRONOLOGICAL order (e.g., "L:5,Ha:8" means L was used first, then Ha)
        session.FilterSegments = string.Join(",", segmentStrings);
        session.PlannedExposures = totalExposures;
        
        _logger.LogDebug("FinalizeSession: Sequence={Sequence} -> {Exposures} exp, FilterSegments='{Segments}'",
            string.Join(",", filterSequence.Select(s => $"{s.Filter}:{s.DurationMinutes:F0}min")),
            totalExposures, session.FilterSegments);
        
        return totalExposures;
    }
    
    
    private Dictionary<string, int> ParseFilterSegments(string? filterSegments)
    {
        var result = new Dictionary<string, int>();
        if (string.IsNullOrEmpty(filterSegments)) return result;
        
        foreach (var segment in filterSegments.Split(','))
        {
            var parts = segment.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var count))
            {
                result[parts[0]] = count;
            }
        }
        return result;
    }
    
    /// <summary>
    /// Process a block of consecutive sessions on the same target.
    /// If total block duration meets minimum, keep all sessions. Otherwise filter out entire block.
    /// </summary>
    private void ProcessTargetBlock(
        List<ScheduledSessionDto> block,
        List<ScheduledSessionDto> output,
        double minDurationMinutes,
        Dictionary<Guid, TargetSchedulingState> targetStates)
    {
        if (block.Count == 0) return;
        
        var blockStart = block.First().StartTimeUtc;
        var blockEnd = block.Last().EndTimeUtc;
        var totalDuration = (blockEnd - blockStart).TotalMinutes;
        
        if (totalDuration >= minDurationMinutes)
        {
            // Block meets minimum duration - keep all sessions in it
            output.AddRange(block);
        }
        else
        {
            // Block too short - but check if it's finishing remaining goals that need less time than minimum
            var targetId = block.First().ScheduledTargetId;
            var targetState = targetStates.Values.FirstOrDefault(ts => ts.Target.Id == targetId);
            var targetName = targetState?.Target.Name ?? "Unknown";
            
            // Check if ALL filters in this block have remaining goals that are less than minimum duration
            // If so, keep the block (allows finishing nearly-complete goals)
            bool isFinishingSmallGoals = false;
            if (targetState != null)
            {
                var filtersInBlock = block.Select(s => s.Filter).Distinct().ToList();
                var allFiltersHaveSmallRemainingGoals = filtersInBlock.All(filter =>
                {
                    if (targetState.FilterProgress.TryGetValue(filter, out var progress))
                    {
                        // Goal needs less time than minimum AND has remaining time
                        return progress.RemainingMinutes > 0 && progress.RemainingMinutes < minDurationMinutes;
                    }
                    return false;
                });
                
                if (allFiltersHaveSmallRemainingGoals && filtersInBlock.Any())
                {
                    isFinishingSmallGoals = true;
                    _logger.LogInformation("KEEPING short target block (finishing small goals): '{Target}' {Start:HH:mm}-{End:HH:mm} ({Duration:F1}min, goals need < {Min}min)",
                        targetName, blockStart, blockEnd, totalDuration, minDurationMinutes);
                    output.AddRange(block);
                }
            }
            
            if (!isFinishingSmallGoals)
            {
                _logger.LogInformation("FILTERED OUT short target block: '{Target}' {Start:HH:mm}-{End:HH:mm} ({Duration:F1}min < {Min}min, {Count} filter segments)",
                    targetName, blockStart, blockEnd, totalDuration, minDurationMinutes, block.Count);
            }
        }
    }
    
    private ScheduledSessionDto CloneSession(ScheduledSessionDto session)
    {
        var clone = new ScheduledSessionDto
        {
            Id = session.Id,
            ScheduledTargetId = session.ScheduledTargetId,
            PanelId = session.PanelId,
            PanelNumber = session.PanelNumber,
            PanelCenterRA = session.PanelCenterRA,
            PanelCenterDec = session.PanelCenterDec,
            SessionDate = session.SessionDate,
            StartTimeUtc = session.StartTimeUtc,
            EndTimeUtc = session.EndTimeUtc,
            Filter = session.Filter,
            PlannedDurationMinutes = session.PlannedDurationMinutes,
            PlannedExposures = session.PlannedExposures,
            FilterSegments = session.FilterSegments,
            Status = session.Status,
            IsManualOverride = session.IsManualOverride,
            FilterShootMethod = session.FilterShootMethod,
            BatchSize = session.BatchSize,
            MoonDistance = session.MoonDistance,
            MoonIllumination = session.MoonIllumination,
            RequiredMoonDistance = session.RequiredMoonDistance
        };
        
        // Initialize FilterSegments if not set (for the first session in a merge)
        if (string.IsNullOrEmpty(clone.FilterSegments) && clone.PlannedExposures > 0)
        {
            clone.FilterSegments = $"{clone.Filter}:{clone.PlannedExposures}";
        }
        
        return clone;
    }

    private double CalculatePriorityScore(
        TargetSchedulingState state,
        SchedulerConfigurationDto configuration,
        DateTime currentTime)
    {
        double score = 0;

        // Primary strategy
        score += CalculateStrategyScore(state, configuration.PrimaryStrategy, currentTime) * 1000;

        // Secondary strategy (tiebreaker)
        if (configuration.SecondaryStrategy.HasValue)
        {
            score += CalculateStrategyScore(state, configuration.SecondaryStrategy.Value, currentTime) * 100;
        }

        // Tertiary strategy (third tiebreaker)
        if (configuration.TertiaryStrategy.HasValue)
        {
            score += CalculateStrategyScore(state, configuration.TertiaryStrategy.Value, currentTime);
        }

        // Apply current priority
        score += state.CurrentPriority * 10;
        
        // Sequential mosaic strategy: slight preference for panels that already have time scheduled tonight
        // This is a TIEBREAKER within the same target, not a dominant factor across different targets
        // 200 points = ~0.2° altitude equivalent - only matters when comparing panels at similar altitudes
        if (state.IsMosaicPanel && 
            state.Target.MosaicShootingStrategy == MosaicShootingStrategy.Sequential &&
            state.ScheduledMinutesTonight > 0)
        {
            // Small bonus for panels with existing sessions - tiebreaker within same target only
            score += 200;
            _logger.LogDebug("Sequential mosaic bonus: '{Target}' P{Panel} gets +200 (has {Minutes:F0}min scheduled)",
                state.Target.Name, state.PanelNumber, state.ScheduledMinutesTonight);
        }
        
        // Panel ordering: apply priority based on MosaicPanelOrderingMethod
        if (state.IsMosaicPanel)
        {
            score += CalculatePanelOrderingBonus(state);
        }

        return score;
    }

    private double CalculateStrategyScore(
        TargetSchedulingState state,
        TargetSelectionStrategy strategy,
        DateTime currentTime)
    {
        return strategy switch
        {
            // Lower priority number = higher priority, so invert: Priority 1 → score 99, Priority 99 → score 1
            TargetSelectionStrategy.PriorityFirst => 100 - state.CurrentPriority,
            TargetSelectionStrategy.AltitudeFirst => CalculateAltitudeScore(state, currentTime),
            TargetSelectionStrategy.TimeFirst => CalculateTimeWindowScore(state), // Shortest time = highest score
            TargetSelectionStrategy.HighestTimeFirst => GetRemainingTimeMinutes(state), // Longest time = highest score
            TargetSelectionStrategy.MoonAvoidanceFirst => CalculateMoonAvoidanceScore(state),
            _ => 0
        };
    }

    private double GetRemainingTimeMinutes(TargetSchedulingState state)
    {
        // Calculate total remaining time across all filters
        return state.Target.ImagingGoals.Sum(g => g.RemainingTimeMinutes);
    }

    private double CalculateAltitudeScore(TargetSchedulingState state, DateTime currentTime)
    {
        // Use current altitude from observable period if available
        if (state.CurrentPeriod != null)
        {
            // Higher altitude = higher score (0-90 degrees normalized to 0-100)
            return state.CurrentPeriod.Altitude * (100.0 / 90.0);
        }
        return 0;
    }

    private double CalculateTimeWindowScore(TargetSchedulingState state)
    {
        // Shorter observability window = higher score (inverse relationship)
        // Targets with less observable time tonight get higher priority (shoot them while available)
        // Use ObservableMinutesTonight if available, otherwise fall back to remaining goal time
        var observableMinutes = state.ObservableMinutesTonight;
        if (observableMinutes > 0)
        {
            return 1000.0 / observableMinutes;
        }
        
        // Fallback to remaining goal time if no observable data
        var remainingTime = GetRemainingTimeMinutes(state);
        return remainingTime > 0 ? 1000.0 / remainingTime : 0;
    }

    private double CalculateMoonAvoidanceScore(TargetSchedulingState state)
    {
        // Higher moon distance = higher score (farther from moon is better)
        // Use current period's moon distance if available
        if (state.CurrentPeriod != null)
        {
            // Moon distance in degrees (0-180), normalize to 0-100 score
            return state.CurrentPeriod.MoonDistance * (100.0 / 180.0);
        }
        return 50; // Default if no period data
    }
    
    /// <summary>
    /// Calculate panel ordering bonus based on MosaicPanelOrderingMethod setting.
    /// Manual: uses ShootingOrder (lower = higher priority)
    /// AutoMinObservability: shorter observable window = higher priority (not yet implemented)
    /// AutoMaxObservability: longer observable window = higher priority (not yet implemented)
    /// </summary>
    private double CalculatePanelOrderingBonus(TargetSchedulingState state)
    {
        if (!state.IsMosaicPanel || state.PanelId == null)
            return 0;
        
        var panel = state.Target.Panels?.FirstOrDefault(p => p.Id == state.PanelId);
        if (panel == null)
            return 0;
        
        switch (state.Target.MosaicPanelOrderingMethod)
        {
            case MosaicPanelOrderingMethod.Manual:
                // ShootingOrder: lower number = higher priority
                // Add bonus inversely proportional to ShootingOrder (max 1000 bonus)
                if (panel.ShootingOrder.HasValue)
                {
                    // ShootingOrder 1 = +1000, ShootingOrder 2 = +500, ShootingOrder 3 = +333, etc.
                    return 1000.0 / panel.ShootingOrder.Value;
                }
                // No ShootingOrder set - use panel number as fallback
                return 1000.0 / (state.PanelNumber ?? 999);
                
            case MosaicPanelOrderingMethod.AutoMinObservability:
                // Panels with LESS observable time get higher priority (shoot them first while available)
                // Use ObservableMinutesTonight from state
                if (state.ObservableMinutesTonight > 0)
                {
                    // Inverse: shorter window = higher bonus (max 1000 for 30min, 100 for 300min)
                    return 1000.0 / (state.ObservableMinutesTonight / 30.0);
                }
                return 0;
                
            case MosaicPanelOrderingMethod.AutoMaxObservability:
                // Panels with MORE observable time get higher priority (save short-window panels for later)
                // Use ObservableMinutesTonight from state
                if (state.ObservableMinutesTonight > 0)
                {
                    // Direct: longer window = higher bonus (normalize to ~0-100 range assuming max 600min)
                    return state.ObservableMinutesTonight / 6.0;
                }
                return 0;
                
            default:
                return 0;
        }
    }
    
    private List<ECameraFilter> CalculateBlockedFilters(
        ScheduledTargetDto target,
        double moonDistance,
        double moonIllumination,
        double moonAltitude,
        double targetAltitude,
        double globalMinAltitude,
        DateTime slotTime,
        DateTime? nauticalDusk,
        DateTime? nauticalDawn,
        DateTime? astronomicalDusk,
        DateTime? astronomicalDawn,
        List<UserFilterMoonAvoidanceProfileDto> filterProfileMappings)
    {
        var blockedFilters = new List<ECameraFilter>();
        
        // Determine if we're in nautical twilight (between nautical and astronomical)
        // Evening nautical twilight: nauticalDusk <= time < astronomicalDusk
        // Morning nautical twilight: astronomicalDawn < time <= nauticalDawn
        bool isInNauticalTwilight = false;
        if (nauticalDusk.HasValue && astronomicalDusk.HasValue && nauticalDawn.HasValue && astronomicalDawn.HasValue)
        {
            bool isEveningNautical = slotTime >= nauticalDusk.Value && slotTime < astronomicalDusk.Value;
            bool isMorningNautical = slotTime > astronomicalDawn.Value && slotTime <= nauticalDawn.Value;
            isInNauticalTwilight = isEveningNautical || isMorningNautical;
        }
        
        // Get all imaging goals that are enabled
        var enabledGoals = target.ImagingGoals?.Where(g => g.IsEnabled).ToList() ?? new List<ImagingGoalDto>();
        
        // Get unique filters
        var targetFilters = enabledGoals.Select(g => g.Filter).Distinct().ToList();
        
        foreach (var filter in targetFilters)
        {
            var goalForFilter = enabledGoals.FirstOrDefault(g => g.Filter == filter);
            
            // Check per-filter AcceptableTwilight from ExposureTemplate
            // If we're in nautical twilight, only filters with AcceptableTwilight=Nautical can image
            if (isInNauticalTwilight)
            {
                var acceptableTwilight = goalForFilter?.ExposureTemplate?.AcceptableTwilight ?? ETwilightType.Astronomical;
                if (acceptableTwilight == ETwilightType.Astronomical)
                {
                    _logger.LogInformation("TwilightBlock [{Target}] {Filter}: BLOCKED - In nautical twilight, filter requires astronomical", 
                        target.Name, filter);
                    blockedFilters.Add(filter);
                    continue;
                }
            }
            
            // Check per-filter MinAltitude from ExposureTemplate
            var filterMinAltitude = goalForFilter?.ExposureTemplate?.MinAltitude ?? -1;
            
            // Use filter-specific MinAltitude if set (>= 0), otherwise use global config
            var effectiveMinAltitude = filterMinAltitude >= 0 ? filterMinAltitude : globalMinAltitude;
            
            if (targetAltitude < effectiveMinAltitude)
            {
                _logger.LogInformation("AltitudeBlock [{Target}] {Filter}: BLOCKED - Target alt {Alt:F1}° < required {Req:F1}° (filter-specific={IsFilter})", 
                    target.Name, filter, targetAltitude, effectiveMinAltitude, filterMinAltitude >= 0);
                blockedFilters.Add(filter);
                continue; // Skip moon avoidance check - already blocked by altitude
            }
            
            // Find the moon avoidance profile for this filter
            var filterMapping = filterProfileMappings.FirstOrDefault(p => p.Filter == filter);
            var profile = filterMapping?.MoonAvoidanceProfile;
            
            if (profile == null)
            {
                _logger.LogDebug("MoonAvoidance [{Target}] {Filter}: No profile assigned", target.Name, filter);
                continue; // No profile = no restrictions
            }
            
            // Check if moon is below the minimum altitude threshold for this profile
            if (moonAltitude < profile.MinMoonAltitudeDegrees)
            {
                _logger.LogDebug("MoonAvoidance [{Target}] {Filter}: Moon below horizon ({MoonAlt:F1}°) - NOT BLOCKED", 
                    target.Name, filter, moonAltitude);
                continue; // Moon is below threshold, filter is not blocked
            }
            
            // Calculate the required avoidance distance for current moon illumination
            var requiredDistance = profile.CalculateAvoidanceDistance(moonIllumination);
            
            // Check if actual moon distance is less than required
            if (moonDistance < requiredDistance)
            {
                _logger.LogInformation("MoonAvoidance [{Target}] {Filter}: BLOCKED - MoonDist {Dist:F1}° < required {Req:F1}° (profile: {Profile}, illum={Illum:F0}%, moonAlt={MoonAlt:F1}°)", 
                    target.Name, filter, moonDistance, requiredDistance, profile.Name, moonIllumination * 100, moonAltitude);
                blockedFilters.Add(filter);
            }
            else
            {
                _logger.LogDebug("MoonAvoidance [{Target}] {Filter}: OK - MoonDist {Dist:F1}° >= required {Req:F1}° (profile: {Profile}, illum={Illum:F0}%)", 
                    target.Name, filter, moonDistance, requiredDistance, profile.Name, moonIllumination * 100);
            }
        }
        
        return blockedFilters;
    }

    private double CalculateTransitScore(TargetSchedulingState state, DateTime currentTime, double observatoryLongitude)
    {
        // Calculate proper hour angle using Local Sidereal Time
        var targetRa = state.IsMosaicPanel && state.Target.Panels?.Any() == true
            ? state.Target.Panels.FirstOrDefault(p => p.Id == state.PanelId)?.RaHours ?? state.Target.RightAscension
            : state.Target.RightAscension;
        
        // Calculate LST (simplified - for accurate LST use astronomy service)
        var jd = GetJulianDate(currentTime);
        var gmst = GetGreenwichMeanSiderealTime(jd);
        var lst = gmst + (observatoryLongitude / 15.0);
        while (lst < 0) lst += 24;
        while (lst >= 24) lst -= 24;
        
        // Hour angle = LST - RA (in hours)
        var hourAngle = lst - targetRa;
        while (hourAngle < -12) hourAngle += 24;
        while (hourAngle > 12) hourAngle -= 24;

        // Score is highest near transit (HA = 0)
        // Convert hour angle to absolute value (0-12 range)
        var absHourAngle = Math.Abs(hourAngle);
        
        // Score: 100 at transit (HA=0), 0 at HA=12
        return (12 - absHourAngle) * (100.0 / 12.0);
    }

    private ECameraFilter? DetermineNextFilter(
        TargetSchedulingState state,
        SchedulerConfigurationDto configuration,
        Dictionary<Guid, double> sharedBatchCounts)
    {
        var availableFilters = state.FilterProgress
            .Where(fp => fp.Value.RemainingMinutes > 0)
            .Select(fp => fp.Key)
            .ToList();

        if (!availableFilters.Any())
            return null;

        // Filter out filters that are currently blocked by moon avoidance
        // This is stored in the state during observable window calculation
        if (state.BlockedFilters != null && state.BlockedFilters.Any())
        {
            availableFilters = availableFilters
                .Where(f => !state.BlockedFilters.Contains(f))
                .ToList();
            
            if (!availableFilters.Any())
                return null;
        }

        // Apply filter pattern - use target-specific override → template → config
        var effectiveFilterPattern = GetEffectiveFilterShootingPattern(state.Target, configuration);
        return effectiveFilterPattern switch
        {
            "Loop" => DetermineFilterLoop(state, availableFilters, configuration),
            "Batch" => DetermineFilterBatches(state, availableFilters, configuration, sharedBatchCounts),
            "Complete" => DetermineFilterSequential(state, availableFilters, configuration),
            _ => availableFilters.First()
        };
    }

    private ECameraFilter DetermineFilterLoop(
        TargetSchedulingState state,
        List<ECameraFilter> availableFilters,
        SchedulerConfigurationDto configuration)
    {
        // Cycle through filters in priority order
        var orderedFilters = OrderFiltersByPriority(availableFilters, state, configuration);

        var currentIndex = state.CurrentFilterIndex % orderedFilters.Count;
        return orderedFilters[currentIndex];
    }

    private ECameraFilter DetermineFilterBatches(
        TargetSchedulingState state,
        List<ECameraFilter> availableFilters,
        SchedulerConfigurationDto configuration,
        Dictionary<Guid, double> panelBatchCounts)
    {
        var orderedFilters = OrderFiltersByPriority(availableFilters, state, configuration);
        var batchSize = configuration.FilterBatchSize > 0 ? configuration.FilterBatchSize : 20;

        // Use batch count PER PANEL (each mosaic panel has its own counter)
        var panelKey = GetPanelBatchKey(state);
        var batchCount = panelBatchCounts.GetValueOrDefault(panelKey, 0.0);

        // Determine which filter based on batch count (use integer part)
        var filterIndex = ((int)batchCount / batchSize) % orderedFilters.Count;
        var selectedFilter = orderedFilters[filterIndex];
        
        _logger.LogInformation("Batch: '{Target}' P{Panel} BatchCount={BatchCount}, Size={Size}, Index={Index}, Filters=[{Filters}] → {Selected}",
            state.Target.Name, state.PanelNumber, batchCount, batchSize, filterIndex,
            string.Join(",", orderedFilters.Select(f => $"{f}(p{state.Target.ImagingGoals?.FirstOrDefault(g => g.Filter == f)?.FilterPriority ?? 999})")),
            selectedFilter);
        
        return selectedFilter;
    }

    private ECameraFilter DetermineFilterSequential(
        TargetSchedulingState state,
        List<ECameraFilter> availableFilters,
        SchedulerConfigurationDto configuration)
    {
        // Complete one filter before moving to next
        var orderedFilters = OrderFiltersByPriority(availableFilters, state, configuration);

        foreach (var filter in orderedFilters)
        {
            if (state.FilterProgress[filter].RemainingMinutes > 0)
                return filter;
        }

        return orderedFilters.First();
    }

    private List<ECameraFilter> OrderFiltersByPriority(
        List<ECameraFilter> filters,
        TargetSchedulingState state,
        SchedulerConfigurationDto configuration)
    {
        // For mosaic panels, get panel goals and apply GoalOrderingMethod
        if (state.IsMosaicPanel && state.PanelId != null)
        {
            var panel = state.Target.Panels?.FirstOrDefault(p => p.Id == state.PanelId);
            var panelGoals = panel?.ImagingGoals;
            
            if (panelGoals != null && panelGoals.Any())
            {
                return filters.OrderBy(f =>
                {
                    var goal = panelGoals.FirstOrDefault(g => g.Filter == f && g.IsEnabled);
                    if (goal == null) return 999;
                    
                    // Apply GoalOrderingMethod adjustment
                    var effectivePriority = goal.FilterPriority;
                    switch (state.Target.GoalOrderingMethod)
                    {
                        case GoalOrderingMethod.BaseGoalsFirst:
                            // Custom goals get penalty (+1000)
                            if (goal.IsCustomGoal) effectivePriority += 1000;
                            break;
                        case GoalOrderingMethod.CustomGoalsFirst:
                            // Base goals get penalty (+1000)
                            if (!goal.IsCustomGoal) effectivePriority += 1000;
                            break;
                        // ByFilterPriority: no adjustment
                    }
                    return effectivePriority;
                }).ToList();
            }
        }
        
        // For non-mosaic targets or panels without goals, use target goals
        if (state.Target.ImagingGoals != null && state.Target.ImagingGoals.Any())
        {
            return filters.OrderBy(f =>
            {
                var goal = state.Target.ImagingGoals.FirstOrDefault(g => g.Filter == f && g.IsEnabled);
                // Lower FilterPriority number = higher priority (sorted first)
                return goal?.FilterPriority ?? 999;
            }).ToList();
        }

        // Fallback to configuration list order
        if (configuration.FilterPriority != null && configuration.FilterPriority.Any())
        {
            var priorityOrder = configuration.FilterPriority;
            return filters.OrderBy(f =>
            {
                var index = priorityOrder.IndexOf(f);
                return index >= 0 ? index : int.MaxValue;
            }).ToList();
        }

        return filters.OrderBy(f => f.ToString()).ToList();
    }

    private void UpdateFilterPatternState(
        TargetSchedulingState state,
        SchedulerConfigurationDto configuration,
        ECameraFilter usedFilter,
        double exposureCount,
        Dictionary<Guid, double> panelBatchCounts)
    {
        state.CurrentFilterIndex++;

        // Use target-specific filter shooting pattern
        var effectiveFilterPattern = GetEffectiveFilterShootingPattern(state.Target, configuration);
        if (effectiveFilterPattern == "Batch")
        {
            // Update batch count PER PANEL (each mosaic panel has its own counter)
            var panelKey = GetPanelBatchKey(state);
            var currentCount = panelBatchCounts.GetValueOrDefault(panelKey, 0.0);
            var newCount = currentCount + exposureCount;
            panelBatchCounts[panelKey] = newCount;
            
            _logger.LogInformation("BatchUpdate: '{Target}' P{Panel} {Filter} +{Exposures:F2} exp, BatchCount now={Total:F1}",
                state.Target.Name, state.PanelNumber, usedFilter, exposureCount, newCount);
        }
    }
    
    /// <summary>
    /// Get a unique key for batch counting. Each mosaic panel has its own counter.
    /// For non-mosaic targets, uses the target ID. For mosaic panels, uses the panel ID.
    /// </summary>
    private Guid GetPanelBatchKey(TargetSchedulingState state)
    {
        // For mosaic panels, use the panel's unique ID
        // For non-mosaic targets, use the target ID
        return state.PanelId ?? state.Target.Id;
    }

    /// <summary>
    /// Calculate batch segments for a time period, splitting at batch boundaries.
    /// Returns list of (filter, startTime, endTime, exposureCount) for each segment.
    /// </summary>
    private List<(ECameraFilter Filter, DateTime Start, DateTime End, int Exposures)> CalculateBatchSegments(
        TargetSchedulingState state,
        DateTime slotStart,
        DateTime slotEnd,
        SchedulerConfigurationDto configuration,
        Dictionary<Guid, double> panelBatchCounts)
    {
        var segments = new List<(ECameraFilter Filter, DateTime Start, DateTime End, int Exposures)>();
        
        var availableFilters = state.FilterProgress
            .Where(fp => fp.Value.RemainingMinutes > 0)
            .Select(fp => fp.Key)
            .ToList();

        if (!availableFilters.Any())
        {
            _logger.LogInformation("BatchSegments: '{Target}' P{Panel} - No filters with remaining time",
                state.Target.Name, state.PanelNumber);
            return segments;
        }

        // Filter out blocked filters
        if (state.BlockedFilters != null && state.BlockedFilters.Any())
        {
            var beforeCount = availableFilters.Count;
            availableFilters = availableFilters.Where(f => !state.BlockedFilters.Contains(f)).ToList();
            if (!availableFilters.Any())
            {
                _logger.LogInformation("BatchSegments: '{Target}' P{Panel} - All {Count} filters blocked by moon: {Blocked}",
                    state.Target.Name, state.PanelNumber, beforeCount, string.Join(", ", state.BlockedFilters));
                return segments;
            }
        }

        var orderedFilters = OrderFiltersByPriority(availableFilters, state, configuration);
        var batchSize = configuration.FilterBatchSize > 0 ? configuration.FilterBatchSize : 20;
        var panelKey = GetPanelBatchKey(state);
        
        // Log the actual filter order for debugging
        var filterOrderWithPriority = orderedFilters.Select(f => {
            var goal = state.Target.ImagingGoals?.FirstOrDefault(g => g.Filter == f && g.IsEnabled);
            return $"{f}(pri={goal?.FilterPriority ?? 999})";
        });
        _logger.LogInformation("BatchSegments: '{Target}' P{Panel} orderedFilters=[{Filters}], batchSize={BatchSize}",
            state.Target.Name, state.PanelNumber, string.Join(", ", filterOrderWithPriority), batchSize);
        
        // Get exposure time for calculating exposures per minute
        // Use the first filter's exposure time as reference (they should be similar for batch logic)
        int exposureTimeSec = 300; // Default 5 min
        var firstGoal = state.Target.ImagingGoals?.FirstOrDefault(g => availableFilters.Contains(g.Filter) && g.IsEnabled);
        if (firstGoal != null)
        {
            exposureTimeSec = firstGoal.ExposureTimeSeconds > 0 ? firstGoal.ExposureTimeSeconds : 300;
        }
        
        var currentTime = slotStart;
        var efficiency = configuration.ImagingEfficiencyPercent / 100.0;
        
        // Use FRACTIONAL batch count for precise tracking (per panel)
        // This ensures we hit exactly the batch size, not 9 or 11
        double fractionalBatchCount = panelBatchCounts.GetValueOrDefault(panelKey, 0);
        
        while (currentTime < slotEnd)
        {
            // Determine current filter based on batch count (use integer part)
            var filterIndex = ((int)fractionalBatchCount / batchSize) % orderedFilters.Count;
            var currentFilter = orderedFilters[filterIndex];
            
            // Calculate FRACTIONAL exposures until next batch boundary
            var exposuresInCurrentBatch = fractionalBatchCount % batchSize;
            var exposuresUntilSwitch = batchSize - exposuresInCurrentBatch;
            
            // Calculate FRACTIONAL exposures that fit in remaining slot time
            var remainingSlotMinutes = (slotEnd - currentTime).TotalMinutes;
            var actualImagingMinutes = remainingSlotMinutes * efficiency;
            var exposuresInRemainingSlot = actualImagingMinutes * 60 / exposureTimeSec;
            
            // If less than 0.1 exposures fit (very small remaining time), we're done
            // Use low threshold to avoid gaps - even partial exposures fill the schedule
            if (exposuresInRemainingSlot < 0.1)
                break;
            
            // Determine segment: either fill until batch switch or until slot ends
            double exposuresForSegment;
            DateTime segmentEnd;
            
            if (exposuresUntilSwitch <= exposuresInRemainingSlot)
            {
                // We'll complete this batch within the slot - split at exact boundary
                exposuresForSegment = exposuresUntilSwitch;
                var segmentMinutes = exposuresForSegment * exposureTimeSec / 60.0 / efficiency;
                segmentEnd = currentTime.AddMinutes(segmentMinutes);
            }
            else
            {
                // Slot ends before batch completes - use all remaining time
                exposuresForSegment = exposuresInRemainingSlot;
                segmentEnd = slotEnd;
            }
            
            // Round to integer for the session (but keep fractional for tracking)
            // Use ceiling to ensure at least 1 exposure for any meaningful segment
            int exposuresInt = Math.Max(1, (int)Math.Ceiling(exposuresForSegment));
            
            _logger.LogInformation("BatchSegment: '{Target}' {Start:HH:mm}-{End:HH:mm} {Filter} ({Exposures} exp, frac={Frac:F2}), BatchCount {Before:F1}→{After:F1}",
                state.Target.Name, currentTime, segmentEnd, currentFilter, exposuresInt, exposuresForSegment,
                fractionalBatchCount, fractionalBatchCount + exposuresForSegment);
            
            segments.Add((currentFilter, currentTime, segmentEnd, exposuresInt));
            
            currentTime = segmentEnd;
            fractionalBatchCount += exposuresForSegment; // Add FRACTIONAL amount for precise tracking
        }
        
        // Update the per-panel batch count with the final fractional value
        // This ensures precise tracking across slots
        panelBatchCounts[panelKey] = fractionalBatchCount;
        
        return segments;
    }

    private void UpdateTargetStates(
        Dictionary<Guid, TargetSchedulingState> targetStates,
        List<ScheduledSessionDto> nightSessions,
        SchedulerConfigurationDto configuration)
    {
        // Check for completed goals and adjust priorities
        foreach (var state in targetStates.Values)
        {
            var allGoalsComplete = state.FilterProgress.Values.All(fp => fp.RemainingMinutes <= 0);

            if (allGoalsComplete)
            {
                // Use target-specific goal completion behavior
                var effectiveGoalCompletionBehavior = GetEffectiveGoalCompletionBehavior(state.Target, configuration);
                var effectiveLowerPriorityTo = GetEffectiveLowerPriorityTo(state.Target, configuration);
                
                switch (effectiveGoalCompletionBehavior)
                {
                    case "Stop":
                        // Target will be filtered out in next iteration
                        break;
                    case "LowerPriority":
                        state.CurrentPriority = effectiveLowerPriorityTo > 0 ? effectiveLowerPriorityTo : 99;
                        break;
                    case "ContinueAsBackup":
                        state.CurrentPriority = 99; // Lowest priority
                        break;
                }
            }
        }
    }

    private SchedulerStatistics CalculateStatistics(
        List<ScheduledSessionDto> sessions,
        List<ScheduledTargetDto> targets,
        int totalNights)
    {
        var stats = new SchedulerStatistics
        {
            TotalNights = totalNights,
            TotalSessions = sessions.Count,
            TotalPlannedHours = sessions.Sum(s => s.PlannedDurationMinutes) / 60.0,
            TargetsScheduled = sessions.Select(s => s.ScheduledTargetId).Distinct().Count(),
            SessionsByFilter = sessions.GroupBy(s => s.Filter.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TimeByTarget = sessions.GroupBy(s => s.ScheduledTargetId)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.PlannedDurationMinutes) / 60.0)
        };

        return stats;
    }
    
    /// <summary>
    /// Check if a target is in its meridian flip window at a given time
    /// </summary>
    private bool IsInMeridianFlipWindow(DateTime slotStart, DateTime slotEnd, double targetRaHours, double observatoryLongitude, MeridianFlipSettingsDto settings)
    {
        // Calculate hour angle at slot start and end
        var haStartMinutes = CalculateHourAngleMinutes(slotStart, targetRaHours, observatoryLongitude);
        var haEndMinutes = CalculateHourAngleMinutes(slotEnd, targetRaHours, observatoryLongitude);
        
        // Flip window bounds
        var windowStartMinutes = -(settings.PauseTimeBeforeFlipMinutes + settings.MaxMinutesToMeridian);
        var windowEndMinutes = settings.MinutesAfterMeridian;
        
        // Check if slot OVERLAPS with flip window (not just if start is in window)
        // Slot overlaps if:
        // 1. Slot start is in window, OR
        // 2. Slot end is in window, OR
        // 3. Slot spans the entire window (start before, end after)
        var startInWindow = haStartMinutes >= windowStartMinutes && haStartMinutes <= windowEndMinutes;
        var endInWindow = haEndMinutes >= windowStartMinutes && haEndMinutes <= windowEndMinutes;
        var spansWindow = haStartMinutes < windowStartMinutes && haEndMinutes > windowEndMinutes;
        
        var inWindow = startInWindow || endInWindow || spansWindow;
        
        // Log when near meridian (within 30 minutes at start or end) - reduced from 60 to reduce log spam
        if (Math.Abs(haStartMinutes) < 30 || Math.Abs(haEndMinutes) < 30)
        {
            _logger.LogInformation("MeridianFlip: Slot={Start:HH:mm}-{End:HH:mm}, RA={RA:F2}h, HA=[{HAStart:F1},{HAEnd:F1}]min, Window=[{WinStart:F0},{WinEnd:F0}], InWindow={InWindow}",
                slotStart, slotEnd, targetRaHours, haStartMinutes, haEndMinutes, windowStartMinutes, windowEndMinutes, inWindow);
        }
        
        return inWindow;
    }
    
    private double CalculateHourAngleMinutes(DateTime time, double targetRaHours, double observatoryLongitude)
    {
        // Calculate Local Sidereal Time (LST)
        var jd = GetJulianDate(time);
        var gmst = GetGreenwichMeanSiderealTime(jd);
        var lst = gmst + (observatoryLongitude / 15.0);
        
        // Normalize to 0-24 range
        while (lst < 0) lst += 24;
        while (lst >= 24) lst -= 24;
        
        // Hour angle = LST - RA
        var hourAngle = lst - targetRaHours;
        
        // Normalize hour angle to -12 to +12 range
        while (hourAngle < -12) hourAngle += 24;
        while (hourAngle > 12) hourAngle -= 24;
        
        return hourAngle * 60; // Convert to minutes
    }
    
    /// <summary>
    /// Calculate Julian Date from DateTime (UTC)
    /// </summary>
    private double GetJulianDate(DateTime dateTime)
    {
        var y = dateTime.Year;
        var m = dateTime.Month;
        var d = dateTime.Day + dateTime.TimeOfDay.TotalDays;
        
        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }
        
        var a = (int)(y / 100.0);
        var b = 2 - a + (int)(a / 4.0);
        
        return (int)(365.25 * (y + 4716)) + (int)(30.6001 * (m + 1)) + d + b - 1524.5;
    }
    
    /// <summary>
    /// Calculate Greenwich Mean Sidereal Time in hours from Julian Date
    /// </summary>
    private double GetGreenwichMeanSiderealTime(double jd)
    {
        var t = (jd - 2451545.0) / 36525.0;
        var gmst = 280.46061837 + 360.98564736629 * (jd - 2451545.0) + 0.000387933 * t * t - t * t * t / 38710000.0;
        
        // Convert to hours and normalize
        gmst = gmst / 15.0;
        while (gmst < 0) gmst += 24;
        while (gmst >= 24) gmst -= 24;
        
        return gmst;
    }
    
    /// <summary>
    /// Calculate the transit time (meridian crossing) for a target on a given night
    /// </summary>
    /// <param name="targetRaHours">Target RA in hours</param>
    /// <param name="observatoryLongitude">Observatory longitude in degrees (east positive)</param>
    /// <param name="nightStart">Start of the night (astronomical dusk)</param>
    /// <param name="nightEnd">End of the night (astronomical dawn)</param>
    /// <returns>Transit time in UTC, or null if transit is not during the night</returns>
    private DateTime? CalculateTransitTime(double targetRaHours, double observatoryLongitude, DateTime nightStart, DateTime nightEnd)
    {
        // Transit occurs when Local Sidereal Time equals RA
        // LST = GMST + longitude/15
        // So GMST at transit = RA - longitude/15
        
        // Search for transit time within the night window
        // Start from night midpoint and search outward
        var midNight = nightStart.AddMinutes((nightEnd - nightStart).TotalMinutes / 2);
        
        // Calculate hour angle at midnight
        var haMidnight = CalculateHourAngleMinutes(midNight, targetRaHours, observatoryLongitude);
        
        // Transit time is when HA = 0, so we need to go back by haMidnight minutes
        // (positive HA means target is past meridian, negative means before)
        var transitTime = midNight.AddMinutes(-haMidnight);
        
        // Check if transit is within the night window (with some margin)
        var extendedStart = nightStart.AddHours(-2);
        var extendedEnd = nightEnd.AddHours(2);
        
        if (transitTime >= extendedStart && transitTime <= extendedEnd)
        {
            return transitTime;
        }
        
        // Try adding/subtracting 24 hours (sidereal day is ~23h56m)
        var transitTimePlus = transitTime.AddHours(23.9344696);
        if (transitTimePlus >= extendedStart && transitTimePlus <= extendedEnd)
        {
            return transitTimePlus;
        }
        
        var transitTimeMinus = transitTime.AddHours(-23.9344696);
        if (transitTimeMinus >= extendedStart && transitTimeMinus <= extendedEnd)
        {
            return transitTimeMinus;
        }
        
        return null;
    }
    
    /// <summary>
    /// Calculate the meridian flip window for a target
    /// </summary>
    /// <param name="transitTime">Transit time in UTC</param>
    /// <param name="settings">Meridian flip settings</param>
    /// <returns>Tuple of (FlipWindowStart, FlipWindowEnd) in UTC</returns>
    private (DateTime? Start, DateTime? End) CalculateMeridianFlipWindow(DateTime? transitTime, MeridianFlipSettingsDto? settings)
    {
        if (transitTime == null || settings?.Enabled != true)
            return (null, null);
        
        // Flip window starts before transit (PauseTimeBeforeFlipMinutes + MaxMinutesToMeridian before)
        // Flip window ends after transit (MinutesAfterMeridian after)
        var windowStart = transitTime.Value.AddMinutes(-(settings.PauseTimeBeforeFlipMinutes + settings.MaxMinutesToMeridian));
        var windowEnd = transitTime.Value.AddMinutes(settings.MinutesAfterMeridian);
        
        return (windowStart, windowEnd);
    }
    
    #region Target Template Helpers
    
    /// <summary>
    /// Get effective minimum altitude for a target (template → config).
    /// Value of -1 means "no override" - use config.
    /// </summary>
    private double GetEffectiveMinAltitude(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        // Start with scheduler config as base
        var effectiveMinAlt = config.MinAltitudeDegrees;
        
        // Target Template can override (if value is >= 0, i.e. not -1)
        if (target.SchedulerTargetTemplate?.MinAltitude.HasValue == true && 
            target.SchedulerTargetTemplate.MinAltitude.Value >= 0)
        {
            effectiveMinAlt = target.SchedulerTargetTemplate.MinAltitude.Value;
        }
        
        return effectiveMinAlt;
    }
    
    /// <summary>
    /// Get effective max hours per night for a target (template → config).
    /// Value of -1 means "no override".
    /// </summary>
    private double GetEffectiveMaxHoursPerNight(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        var effective = config.MaxHoursPerTargetPerNight;
        
        if (target.SchedulerTargetTemplate?.MaxHoursPerNight.HasValue == true &&
            target.SchedulerTargetTemplate.MaxHoursPerNight.Value >= 0)
            effective = target.SchedulerTargetTemplate.MaxHoursPerNight.Value;
        
        return effective;
    }
    
    /// <summary>
    /// Get effective min session duration for a target (template → config).
    /// Value of -1 means "no override".
    /// </summary>
    private int GetEffectiveMinSessionDuration(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        var effective = config.MinSessionDurationMinutes;
        
        if (target.SchedulerTargetTemplate?.MinSessionDurationMinutes.HasValue == true &&
            target.SchedulerTargetTemplate.MinSessionDurationMinutes.Value >= 0)
            effective = target.SchedulerTargetTemplate.MinSessionDurationMinutes.Value;
        
        return effective;
    }
    
    /// <summary>
    /// Get effective goal completion behavior for a target (template → config)
    /// </summary>
    private string GetEffectiveGoalCompletionBehavior(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        if (!string.IsNullOrEmpty(target.SchedulerTargetTemplate?.GoalCompletionBehaviour))
            return target.SchedulerTargetTemplate.GoalCompletionBehaviour;
        return config.GoalCompletionBehavior;
    }
    
    /// <summary>
    /// Get effective lower priority value for a target (template → config).
    /// Value of -1 means "no override".
    /// </summary>
    private int GetEffectiveLowerPriorityTo(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        var effective = config.LowerPriorityTo;
        
        if (target.SchedulerTargetTemplate?.LowerPriorityTo.HasValue == true &&
            target.SchedulerTargetTemplate.LowerPriorityTo.Value >= 0)
            effective = target.SchedulerTargetTemplate.LowerPriorityTo.Value;
        
        return effective;
    }
    
    /// <summary>
    /// Get effective moon avoidance setting for a target (template → config)
    /// </summary>
    private bool GetEffectiveUseMoonAvoidance(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        if (target.SchedulerTargetTemplate?.UseMoonAvoidance.HasValue == true)
            return target.SchedulerTargetTemplate.UseMoonAvoidance.Value;
        return config.UseMoonAvoidance;
    }
    
    /// <summary>
    /// Get effective min moon phase percent for a target (template → null if not set)
    /// </summary>
    private double? GetEffectiveMinMoonPhasePercent(ScheduledTargetDto target)
    {
        if (target.SchedulerTargetTemplate?.MinMoonPhasePercent.HasValue == true)
            return target.SchedulerTargetTemplate.MinMoonPhasePercent.Value;
        return null;
    }
    
    /// <summary>
    /// Get effective max moon phase percent for a target (template → null if not set)
    /// </summary>
    private double? GetEffectiveMaxMoonPhasePercent(ScheduledTargetDto target)
    {
        if (target.SchedulerTargetTemplate?.MaxMoonPhasePercent.HasValue == true)
            return target.SchedulerTargetTemplate.MaxMoonPhasePercent.Value;
        return null;
    }
    
    /// <summary>
    /// Get effective filter shooting pattern for a target (template → config)
    /// </summary>
    private string GetEffectiveFilterShootingPattern(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        if (!string.IsNullOrEmpty(target.SchedulerTargetTemplate?.FilterShootingPattern))
            return target.SchedulerTargetTemplate.FilterShootingPattern;
        return config.FilterShootingPattern;
    }
    
    /// <summary>
    /// Get effective filter batch size for a target (template → config)
    /// Note: No inline override for batch size on ScheduledTarget currently
    /// </summary>
    private int GetEffectiveFilterBatchSize(ScheduledTargetDto target, SchedulerConfigurationDto config)
    {
        if (target.SchedulerTargetTemplate?.FilterBatchSize.HasValue == true)
            return target.SchedulerTargetTemplate.FilterBatchSize.Value;
        return config.FilterBatchSize;
    }
    
    /// <summary>
    /// Get timezone abbreviation for display (e.g., "PST", "UTC+8")
    /// </summary>
    private static string GetTimezoneAbbreviation(TimeZoneInfo tz, DateTime utcTime)
    {
        try
        {
            // Check if daylight saving is in effect
            var isDst = tz.IsDaylightSavingTime(utcTime);
            var offset = tz.GetUtcOffset(utcTime);
            
            // Try to get a meaningful abbreviation
            // For common timezones, use standard abbreviations
            var tzId = tz.Id.ToLowerInvariant();
            if (tzId.Contains("pacific")) return isDst ? "PDT" : "PST";
            if (tzId.Contains("mountain")) return isDst ? "MDT" : "MST";
            if (tzId.Contains("central") && tzId.Contains("america")) return isDst ? "CDT" : "CST";
            if (tzId.Contains("eastern")) return isDst ? "EDT" : "EST";
            if (tzId.Contains("utc") || tzId.Contains("coordinated")) return "UTC";
            if (tzId.Contains("greenwich")) return "GMT";
            
            // Fall back to UTC offset format
            var sign = offset >= TimeSpan.Zero ? "+" : "";
            if (offset.Minutes == 0)
                return $"UTC{sign}{offset.Hours}";
            return $"UTC{sign}{offset.Hours}:{Math.Abs(offset.Minutes):D2}";
        }
        catch
        {
            return "UTC";
        }
    }
    
    #endregion
    
    #region Real-Time Slot Selection
    
    /// <summary>
    /// Get the next slot to execute at the current time using the same algorithm as preview.
    /// This ensures in-sequence target selection matches the NightPreview.
    /// </summary>
    public async Task<RealTimeSlotResult?> GetNextSlotAsync(
        List<ScheduledTargetDto> targets,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        DateTime currentTime,
        Guid? currentTargetId,
        Guid? currentPanelId,
        string? currentFilter,
        MeridianFlipSettingsDto? meridianFlipSettings = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GetNextSlotAsync: Evaluating {TargetCount} targets at {Time:HH:mm:ss} UTC",
                targets.Count, currentTime);
            
            if (!targets.Any())
            {
                return new RealTimeSlotResult 
                { 
                    HasSlot = false, 
                    ShouldWait = true, 
                    WaitMinutes = 5, 
                    Message = "No active targets" 
                };
            }
            
            // Get twilight times for tonight
            // Use noon of the correct day to ensure we get the right night's twilight
            // If we're before noon UTC, we might still be in the previous night (after midnight but before dawn)
            // So use the previous day's noon to get that night's twilight
            var twilightDate = currentTime.Hour < 12 
                ? currentTime.Date.AddDays(-1).AddHours(12)  // Before noon: use previous day's noon
                : currentTime.Date.AddHours(12);              // After noon: use current day's noon
            
            var twilightTimes = await _astronomyService.GetTwilightTimesAsync(
                observatory.Latitude, observatory.Longitude, twilightDate);
            
            _logger.LogDebug("GetNextSlotAsync: CurrentTime={CurrentTime:yyyy-MM-dd HH:mm:ss} UTC, TwilightDate={TwilightDate:yyyy-MM-dd HH:mm} UTC",
                currentTime, twilightDate);
            
            if (!twilightTimes.Astronomical.Dawn.HasValue || !twilightTimes.Astronomical.Dusk.HasValue)
            {
                _logger.LogWarning("GetNextSlotAsync: No astronomical twilight data available");
                return new RealTimeSlotResult 
                { 
                    HasSlot = false, 
                    ShouldWait = true, 
                    WaitMinutes = 5, 
                    Message = "No twilight data available" 
                };
            }
            
            // LEGACY SWAP: AASharp naming uses Begin=Dawn (evening) and End=Dusk (morning)
            // which is opposite to conventional naming, so we swap here
            var astronomicalDusk = twilightTimes.Astronomical.Dawn.Value; // Evening start (swapped)
            var astronomicalDawn = twilightTimes.Astronomical.Dusk.Value; // Morning end (swapped)
            if (astronomicalDawn < astronomicalDusk)
                astronomicalDawn = astronomicalDawn.AddDays(1);
            
            _logger.LogInformation("GetNextSlotAsync: Astronomical.Dawn={Dawn:yyyy-MM-dd HH:mm:ss} UTC, Astronomical.Dusk={Dusk:yyyy-MM-dd HH:mm:ss} UTC",
                twilightTimes.Astronomical.Dawn, twilightTimes.Astronomical.Dusk);
            _logger.LogInformation("GetNextSlotAsync: AstroDusk(evening)={Dusk:yyyy-MM-dd HH:mm:ss} UTC, AstroDawn(morning)={Dawn:yyyy-MM-dd HH:mm:ss} UTC, Now={Now:yyyy-MM-dd HH:mm:ss} UTC",
                astronomicalDusk, astronomicalDawn, currentTime);
            _logger.LogInformation("GetNextSlotAsync: Checks - BeforeDusk={BeforeDusk}, AfterDawn={AfterDawn}",
                currentTime < astronomicalDusk, currentTime > astronomicalDawn);
            
            // Get nautical twilight times for targets that allow imaging during nautical twilight
            // LEGACY SWAP: Same swap as astronomical twilight
            DateTime? nauticalDusk = twilightTimes.Nautical.Dawn; // Evening start (swapped)
            DateTime? nauticalDawn = twilightTimes.Nautical.Dusk; // Morning end (swapped)
            if (nauticalDusk.HasValue && nauticalDawn.HasValue && nauticalDawn.Value < nauticalDusk.Value)
                nauticalDawn = nauticalDawn.Value.AddDays(1);
            
            // Use nautical twilight as the outer bounds (earliest possible start, latest possible end)
            // Individual targets may have stricter requirements based on their exposure templates
            var earliestStart = nauticalDusk ?? astronomicalDusk;
            var latestEnd = nauticalDawn ?? astronomicalDawn;
            
            // Check if we're completely outside any possible imaging time
            if (currentTime > latestEnd)
            {
                _logger.LogInformation("GetNextSlotAsync: Past nautical dawn, night is over");
                return new RealTimeSlotResult 
                { 
                    HasSlot = false, 
                    ShouldStop = true, 
                    StopReason = Shared.Model.DTO.Client.StopReason.PastAstronomicalDawn,
                    Message = "Past twilight - night is over" 
                };
            }
            
            // Calculate observable windows for all targets at current time
            // This considers each target's twilight requirements from exposure templates
            var observableWindows = await CalculateObservableWindowsForNightAsync(
                targets, currentTime.Date, twilightTimes, observatory, configuration,
                moonAvoidanceProfiles, cancellationToken);
            
            if (!observableWindows.Any())
            {
                _logger.LogWarning("GetNextSlotAsync: No observable windows calculated");
                return new RealTimeSlotResult 
                { 
                    HasSlot = false, 
                    ShouldWait = true, 
                    WaitMinutes = 5, 
                    Message = "No targets have observable windows" 
                };
            }
            
            // Initialize target states
            var targetStates = InitializeTargetStates(targets);
            
            // Find targets observable RIGHT NOW (current time within an observable period)
            var nowObservableTargets = new List<(Guid TargetId, TargetSchedulingState State, ObservablePeriod Period)>();
            
            foreach (var window in observableWindows)
            {
                var currentPeriod = window.ObservablePeriods.FirstOrDefault(p =>
                    p.StartTime <= currentTime && p.EndTime >= currentTime && p.IsObservable);
                
                if (currentPeriod != null && targetStates.ContainsKey(window.TargetId))
                {
                    var state = targetStates[window.TargetId];
                    state.CurrentPeriod = currentPeriod;
                    
                    // Calculate blocked filters for this target at current time
                    state.BlockedFilters = CalculateBlockedFilters(
                        state.Target,
                        currentPeriod.MoonDistance,
                        currentPeriod.MoonIllumination,
                        currentPeriod.MoonAltitude,
                        currentPeriod.Altitude,
                        GetEffectiveMinAltitude(state.Target, configuration),
                        currentTime,
                        nauticalDusk,
                        nauticalDawn,
                        astronomicalDusk,
                        astronomicalDawn,
                        GetEffectiveUseMoonAvoidance(state.Target, configuration) ? moonAvoidanceProfiles : new List<UserFilterMoonAvoidanceProfileDto>());
                    
                    nowObservableTargets.Add((window.TargetId, state, currentPeriod));
                }
            }
            
            if (!nowObservableTargets.Any())
            {
                // Find next observable time (must be before end of night - nautical dawn if available)
                var nextObservableTime = observableWindows
                    .SelectMany(w => w.ObservablePeriods)
                    .Where(p => p.IsObservable && p.StartTime > currentTime && p.StartTime < latestEnd)
                    .OrderBy(p => p.StartTime)
                    .FirstOrDefault()?.StartTime;
                
                // If no more observable periods tonight, stop the scheduler
                if (!nextObservableTime.HasValue)
                {
                    _logger.LogInformation("GetNextSlotAsync: No more targets observable tonight - stopping");
                    return new RealTimeSlotResult 
                    { 
                        HasSlot = false, 
                        ShouldStop = true, 
                        StopReason = Shared.Model.DTO.Client.StopReason.NoMoreTargetsTonight,
                        Message = "No more targets observable tonight" 
                    };
                }
                
                var waitMinutes = Math.Max(1, (int)Math.Ceiling((nextObservableTime.Value - currentTime).TotalMinutes));
                
                var nextObservableLocal = nextObservableTime.Value.ToLocalTime();
                _logger.LogInformation("GetNextSlotAsync: No targets observable now, next at {Time:HH:mm} local", nextObservableLocal);
                return new RealTimeSlotResult 
                { 
                    HasSlot = false, 
                    ShouldWait = true, 
                    WaitMinutes = Math.Min(waitMinutes, 30),
                    WaitUntilUtc = nextObservableTime,
                    Message = $"Next target observable at {nextObservableLocal:HH:mm}" 
                };
            }
            
            // Score and sort targets using same algorithm as preview
            var scoredTargets = nowObservableTargets
                .Select(t => new
                {
                    t.TargetId,
                    t.State,
                    t.Period,
                    Priority = CalculatePriorityScore(t.State, configuration, currentTime)
                })
                .OrderByDescending(t => t.Priority)
                .ToList();
            
            _logger.LogInformation("GetNextSlotAsync: {Count} targets observable now. Top: {Targets}",
                scoredTargets.Count,
                string.Join(", ", scoredTargets.Take(3).Select(t => 
                    $"{t.State.Target.Name}={t.Priority:F0} alt={t.Period.Altitude:F1}°")));
            
            // Try each target in priority order until we find one with an available goal
            foreach (var scored in scoredTargets)
            {
                var state = scored.State;
                var target = state.Target;
                var period = scored.Period;
                var repeatCount = Math.Max(1, target.RepeatCount);
                
                // For mosaic panels, get panel-specific goals; for regular targets, use target goals
                IEnumerable<ImagingGoalDto>? goalsSource = null;
                ScheduledTargetPanelDto? panel = null;
                
                if (state.IsMosaicPanel && state.PanelId.HasValue && target.Panels?.Any() == true)
                {
                    panel = target.Panels.FirstOrDefault(p => p.Id == state.PanelId.Value);
                    if (panel?.ImagingGoals?.Any() == true)
                    {
                        // Use panel-specific goals (converted from PanelImagingGoalDto)
                        // Filter, FilterPriority, ExposureTimeSeconds are computed from ExposureTemplate
                        goalsSource = panel.ImagingGoals.Select(pg => new ImagingGoalDto
                        {
                            Id = pg.Id,
                            ExposureTemplateId = pg.ExposureTemplateId,
                            ExposureTemplate = pg.ExposureTemplate,
                            GoalExposureCount = pg.GoalExposureCount,
                            CompletedExposures = pg.CompletedExposures,
                            IsEnabled = pg.IsEnabled
                        });
                        _logger.LogDebug("GetNextSlotAsync: Using panel {PanelNum} goals for mosaic target {Name}", 
                            panel.PanelNumber, target.Name);
                    }
                    else
                    {
                        _logger.LogDebug("GetNextSlotAsync: Panel {PanelId} has no goals, skipping", state.PanelId);
                        continue;
                    }
                }
                else
                {
                    goalsSource = target.ImagingGoals;
                }
                
                // Get available (non-blocked) imaging goals, ordered by FilterPriority
                var goals = goalsSource?
                    .Where(g => g.IsEnabled && g.CompletedExposures < g.GoalExposureCount * repeatCount)
                    .Where(g => state.BlockedFilters == null || !state.BlockedFilters.Contains(g.Filter))
                    .OrderBy(g => g.FilterPriority) // Lower number = higher priority
                    .ToList() ?? new List<ImagingGoalDto>();
                
                if (!goals.Any())
                {
                    _logger.LogDebug("GetNextSlotAsync: Target {Name}{Panel} has no available goals (blocked: {Blocked})",
                        target.Name, 
                        panel != null ? $" Panel {panel.PanelNumber}" : "",
                        string.Join(",", state.BlockedFilters ?? new List<ECameraFilter>()));
                    continue;
                }
                
                // Apply FilterShootingPattern logic to select the appropriate goal
                var filterPattern = GetEffectiveFilterShootingPattern(target, configuration);
                var batchSize = GetEffectiveFilterBatchSize(target, configuration);
                var goal = FilterPatternSelector.SelectGoal(goals, filterPattern, batchSize, currentFilter, target.Name, panel?.PanelNumber);
                
                _logger.LogDebug("GetNextSlotAsync: Target {Name}{Panel} available goals: [{Goals}] → selected {Selected} (priority {Pri}, pattern={Pattern})",
                    target.Name,
                    panel != null ? $" Panel {panel.PanelNumber}" : "",
                    string.Join(", ", goals.Select(g => $"{g.Filter}(p{g.FilterPriority})")),
                    goal.Filter, goal.FilterPriority, filterPattern);
                var total = goal.GoalExposureCount * repeatCount;
                var template = goal.ExposureTemplate;
                
                // Dither: use exposure template value, fallback to target template if -1
                var ditherEvery = template?.DitherEveryX ?? -1;
                if (ditherEvery <= 0)
                {
                    ditherEvery = target.SchedulerTargetTemplate?.DitherEveryX ?? -1;
                }
                
                // For mosaic panels, use panel coordinates; for regular targets, use target coordinates
                var slotRa = panel?.RaHours ?? target.RightAscension;
                var slotDec = panel?.DecDegrees ?? target.Declination;
                
                // Check if slew is required: different target OR different panel for mosaics
                var requiresSlew = currentTargetId != target.Id || 
                    (state.IsMosaicPanel && currentPanelId != state.PanelId);
                
                var result = new RealTimeSlotResult
                {
                    HasSlot = true,
                    TargetId = target.Id,
                    TargetName = target.Name,
                    PanelId = state.PanelId,
                    PanelNumber = state.PanelNumber,
                    RightAscensionHours = slotRa,
                    DeclinationDegrees = slotDec,
                    PositionAngle = target.PA,
                    ImagingGoalId = goal.Id,
                    Filter = goal.Filter.ToString(),
                    ExposureTimeSeconds = goal.ExposureTimeSeconds,
                    Gain = template?.Gain ?? -1,
                    Offset = template?.Offset ?? -1,
                    Binning = $"{template?.Binning ?? 1}x{template?.Binning ?? 1}",
                    RequiresSlew = requiresSlew,
                    RequiresFilterChange = currentFilter != goal.Filter.ToString(),
                    CompletedExposures = goal.CompletedExposures,
                    TotalGoalExposures = total,
                    DitherEveryX = ditherEvery > 0 ? ditherEvery : 0,
                    DitherAfterExposure = ditherEvery > 0 && (goal.CompletedExposures + 1) % ditherEvery == 0,
                    CurrentAltitude = period.Altitude,
                    MoonDistance = period.MoonDistance,
                    MoonIllumination = period.MoonIllumination * 100,
                    SelectionReason = $"Priority={scored.Priority:F0}, Alt={period.Altitude:F1}°",
                    Message = panel != null 
                        ? $"{target.Name} P{panel.PanelNumber} - {goal.Filter} ({goal.CompletedExposures + 1}/{total})"
                        : $"{target.Name} - {goal.Filter} ({goal.CompletedExposures + 1}/{total})"
                };
                
                _logger.LogInformation("GetNextSlotAsync: Selected {Target}{Panel} {Filter} (Alt={Alt:F1}°, MoonDist={Moon:F1}°, Priority={Pri:F0})",
                    target.Name, panel != null ? $" P{panel.PanelNumber}" : "", goal.Filter, period.Altitude, period.MoonDistance, scored.Priority);
                
                return result;
            }
            
            // No target had available goals (all blocked by moon/altitude)
            _logger.LogWarning("GetNextSlotAsync: All observable targets have blocked filters");
            return new RealTimeSlotResult 
            { 
                HasSlot = false, 
                ShouldWait = true, 
                WaitMinutes = 5, 
                Message = "All filters blocked by moon avoidance or altitude" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNextSlotAsync: Error evaluating targets");
            return new RealTimeSlotResult 
            { 
                HasSlot = false, 
                ShouldWait = true, 
                WaitMinutes = 5, 
                Message = $"Error: {ex.Message}" 
            };
        }
    }
    
    #endregion
}

// Helper classes
internal class TargetSchedulingState
{
    public ScheduledTargetDto Target { get; set; } = null!;
    public Guid? PanelId { get; set; } // For mosaic panels
    public int? PanelNumber { get; set; } // For mosaic panels
    public bool IsMosaicPanel { get; set; }
    public int CurrentPriority { get; set; }
    public double TotalScheduledMinutes { get; set; }
    public double ScheduledMinutesTonight { get; set; }
    public double ObservableMinutesTonight { get; set; } // Total observable time for TimeFirst strategy
    public int CurrentFilterIndex { get; set; }
    public int CurrentBatchCount { get; set; }
    public Dictionary<ECameraFilter, FilterProgress> FilterProgress { get; set; } = new();
    public List<ECameraFilter>? BlockedFilters { get; set; } // Filters currently blocked by moon avoidance
    public ObservablePeriod? CurrentPeriod { get; set; } // Current observable period for scoring
}

internal class FilterProgress
{
    public double GoalMinutes { get; set; }
    public double CompletedMinutes { get; set; }
    public double ScheduledMinutes { get; set; }
    public double RemainingMinutes { get; set; }
}

internal class TargetObservableWindow
{
    public Guid TargetId { get; set; }
    public ScheduledTargetDto Target { get; set; } = null!;
    public Guid? PanelId { get; set; } // For mosaic panels
    public int? PanelNumber { get; set; } // For mosaic panels
    public DateTime Date { get; set; }
    public DateTime NightStart { get; set; }
    public DateTime NightEnd { get; set; }
    public List<ObservablePeriod> ObservablePeriods { get; set; } = new();
    public double TotalObservableMinutes { get; set; }
}

/// <summary>
/// Select the appropriate goal based on FilterShootingPattern.
/// This enables Batch, Loop, and Complete/Sequential modes for live scheduling.
/// </summary>
internal static class FilterPatternSelector
{
    /// <summary>
    /// Select goal based on filter shooting pattern
    /// </summary>
    /// <param name="goals">Available goals ordered by FilterPriority</param>
    /// <param name="filterPattern">Batch, Loop, or Complete</param>
    /// <param name="batchSize">Batch size for Batch mode</param>
    /// <param name="currentFilter">Currently loaded filter (from NINA)</param>
    /// <param name="targetName">Target name for logging</param>
    /// <param name="panelNumber">Panel number for logging (null for non-mosaic)</param>
    public static ImagingGoalDto SelectGoal(
        List<ImagingGoalDto> goals,
        string filterPattern,
        int batchSize,
        string? currentFilter,
        string targetName,
        int? panelNumber)
    {
        if (goals.Count == 1)
            return goals.First();
        
        // Parse current filter if provided
        ECameraFilter? currentFilterEnum = null;
        if (!string.IsNullOrEmpty(currentFilter) && Enum.TryParse<ECameraFilter>(currentFilter, true, out var parsed))
        {
            currentFilterEnum = parsed;
        }
        
        return filterPattern switch
        {
            "Batch" => SelectBatchGoal(goals, batchSize, currentFilterEnum),
            "Loop" => SelectLoopGoal(goals, currentFilterEnum),
            "Complete" or "Sequential" => goals.First(), // Complete mode: stick with highest priority
            _ => goals.First()
        };
    }
    
    /// <summary>
    /// Batch mode: Continue with current filter until batch is complete, then move to next
    /// </summary>
    private static ImagingGoalDto SelectBatchGoal(List<ImagingGoalDto> goals, int batchSize, ECameraFilter? currentFilter)
    {
        if (batchSize <= 0) batchSize = 10;
        
        // If we have a current filter, check if we should continue with it
        if (currentFilter.HasValue)
        {
            var currentGoal = goals.FirstOrDefault(g => g.Filter == currentFilter.Value);
            if (currentGoal != null)
            {
                // Calculate total exposures across ALL goals to determine batch position
                var totalCompleted = goals.Sum(g => g.CompletedExposures);
                var batchPosition = totalCompleted % batchSize;
                
                // If we're mid-batch (not at batch boundary), continue with current filter
                if (batchPosition != 0)
                {
                    return currentGoal;
                }
                
                // At batch boundary - check if current filter's batch is complete
                // Also check if current filter still has remaining work
                var currentFilterCompleted = currentGoal.CompletedExposures;
                var currentBatchRemaining = batchSize - (currentFilterCompleted % batchSize);
                
                // If current filter has more work and we haven't done a full batch yet, continue
                if (currentBatchRemaining < batchSize && currentBatchRemaining > 0)
                {
                    return currentGoal;
                }
            }
        }
        
        // No current filter or batch complete - select next filter based on batch count
        var totalExposures = goals.Sum(g => g.CompletedExposures);
        var filterIndex = (totalExposures / batchSize) % goals.Count;
        return goals[filterIndex];
    }
    
    /// <summary>
    /// Loop mode: Cycle through filters round-robin style
    /// </summary>
    private static ImagingGoalDto SelectLoopGoal(List<ImagingGoalDto> goals, ECameraFilter? currentFilter)
    {
        // Find the current filter's index and select the next one
        if (currentFilter.HasValue)
        {
            var currentIndex = goals.FindIndex(g => g.Filter == currentFilter.Value);
            if (currentIndex >= 0)
            {
                // Move to next filter (round-robin)
                var nextIndex = (currentIndex + 1) % goals.Count;
                return goals[nextIndex];
            }
        }
        
        // No current filter or not found - start with first (highest priority)
        return goals.First();
    }
}

internal class ObservablePeriod
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Altitude { get; set; }
    public double Azimuth { get; set; }
    public double MoonDistance { get; set; }
    public double MoonIllumination { get; set; }
    public double MoonAltitude { get; set; } // Moon's altitude at this time
    public bool IsObservable { get; set; }
}

internal class TimeSlot
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
