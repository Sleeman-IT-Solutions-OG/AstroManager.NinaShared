using Shared.Model.DTO.Scheduler;
using Shared.Model.DTO.Settings;

namespace Shared.Services.Scheduler;

/// <summary>
/// Shared service for running the scheduling algorithm locally (both Blazor and NINA)
/// </summary>
public interface ISchedulingAlgorithmService
{
    /// <summary>
    /// Run the scheduling algorithm to generate imaging sessions
    /// </summary>
    Task<SchedulerRunResult> RunSchedulerAsync(
        List<ScheduledTargetDto> targets,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        EquipmentDto equipment,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        IProgress<SchedulerProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate a preview for a single night
    /// </summary>
    /// <param name="startTimeOverride">Optional start time override - if set, scheduling starts from this time instead of astronomical dusk. Use for "Now" functionality.</param>
    Task<SchedulerPreviewDto> GeneratePreviewAsync(
        List<ScheduledTargetDto> targets,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        DateTime previewDate,
        MeridianFlipSettingsDto? meridianFlipSettings = null,
        DateTime? startTimeOverride = null,
        IProgress<SchedulerProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate a preview for a single night with options for detailed explanations
    /// </summary>
    /// <param name="startTimeOverride">Optional start time override - if set, scheduling starts from this time instead of astronomical dusk. Use for "Now" functionality.</param>
    Task<SchedulerPreviewDto> GeneratePreviewAsync(
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
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the next slot to execute at the current time using the same algorithm as preview.
    /// This ensures in-sequence target selection matches the NightPreview.
    /// </summary>
    /// <param name="targets">Active targets with imaging goals</param>
    /// <param name="configuration">Scheduler configuration</param>
    /// <param name="observatory">Observatory location</param>
    /// <param name="moonAvoidanceProfiles">Moon avoidance profiles for filters</param>
    /// <param name="currentTime">Current UTC time</param>
    /// <param name="currentTargetId">Currently loaded target (to minimize slews)</param>
    /// <param name="currentPanelId">Currently loaded panel</param>
    /// <param name="currentFilter">Currently loaded filter</param>
    /// <param name="meridianFlipSettings">Meridian flip settings from NINA</param>
    /// <returns>Next slot to execute, or null if no valid slot</returns>
    Task<RealTimeSlotResult?> GetNextSlotAsync(
        List<ScheduledTargetDto> targets,
        SchedulerConfigurationDto configuration,
        ObservatoryDto observatory,
        List<UserFilterMoonAvoidanceProfileDto> moonAvoidanceProfiles,
        DateTime currentTime,
        Guid? currentTargetId,
        Guid? currentPanelId,
        string? currentFilter,
        MeridianFlipSettingsDto? meridianFlipSettings = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from scheduler run
/// </summary>
public class SchedulerRunResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ScheduledSessionDto> Sessions { get; set; } = new();
    public SchedulerStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Statistics from scheduler run
/// </summary>
public class SchedulerStatistics
{
    public int TotalNights { get; set; }
    public int TotalSessions { get; set; }
    public double TotalPlannedHours { get; set; }
    public int TargetsScheduled { get; set; }
    public Dictionary<string, int> SessionsByFilter { get; set; } = new();
    public Dictionary<Guid, double> TimeByTarget { get; set; } = new();
}

/// <summary>
/// Progress update during scheduling
/// </summary>
public class SchedulerProgress
{
    public int CurrentNight { get; set; }
    public int TotalNights { get; set; }
    public DateTime CurrentDate { get; set; }
    public int SessionsGenerated { get; set; }
    public string? CurrentActivity { get; set; }
}

/// <summary>
/// Result from real-time GetNextSlot call
/// </summary>
public class RealTimeSlotResult
{
    public bool HasSlot { get; set; }
    public string? Message { get; set; }
    
    // Target/Panel info
    public Guid? TargetId { get; set; }
    public string? TargetName { get; set; }
    public Guid? PanelId { get; set; }
    public int? PanelNumber { get; set; }
    
    // Coordinates
    public double RightAscensionHours { get; set; }
    public double DeclinationDegrees { get; set; }
    public double? PositionAngle { get; set; }
    
    // Imaging goal
    public Guid? ImagingGoalId { get; set; }
    public string? Filter { get; set; }
    public double ExposureTimeSeconds { get; set; }
    public int Gain { get; set; } = -1;
    public int Offset { get; set; } = -1;
    public string Binning { get; set; } = "1x1";
    
    // Progress
    public int CompletedExposures { get; set; }
    public int TotalGoalExposures { get; set; }
    
    // Flags
    public bool RequiresSlew { get; set; }
    public bool RequiresFilterChange { get; set; }
    public bool DitherAfterExposure { get; set; }
    public int DitherEveryX { get; set; }
    
    // Wait/Stop info
    public bool ShouldWait { get; set; }
    public int WaitMinutes { get; set; }
    public DateTime? WaitUntilUtc { get; set; }
    public bool ShouldStop { get; set; }
    public Shared.Model.DTO.Client.StopReason? StopReason { get; set; }
    
    // Debug info
    public double? CurrentAltitude { get; set; }
    public double? MoonDistance { get; set; }
    public double? MoonIllumination { get; set; }
    public string? SelectionReason { get; set; }
}
