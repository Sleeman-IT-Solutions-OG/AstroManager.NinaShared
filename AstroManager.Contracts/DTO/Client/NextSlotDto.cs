using Shared.Model.Enums;

namespace Shared.Model.DTO.Client;

/// <summary>
/// Response DTO for the next exposure slot request
/// Contains all information needed by NINA to execute the next exposure
/// </summary>
public class NextSlotDto
{
    /// <summary>
    /// Type of slot: Exposure, Wait, Stop, or Park
    /// </summary>
    public SlotType SlotType { get; set; } = SlotType.Exposure;
    
    /// <summary>
    /// Session ID for tracking (used for progress reporting)
    /// </summary>
    public Guid? SessionId { get; set; }
    
    /// <summary>
    /// Target ID
    /// </summary>
    public Guid? TargetId { get; set; }
    
    /// <summary>
    /// Queue item ID (for manual mode - to update status on skip/fail)
    /// </summary>
    public Guid? QueueItemId { get; set; }
    
    /// <summary>
    /// Target name for display
    /// </summary>
    public string? TargetName { get; set; }
    
    /// <summary>
    /// Panel ID if this is a mosaic target (null for non-mosaic)
    /// </summary>
    public Guid? PanelId { get; set; }
    
    /// <summary>
    /// Panel name for display (e.g., "Panel_1")
    /// </summary>
    public string? PanelName { get; set; }
    
    /// <summary>
    /// Panel number (1-based) for mosaic targets
    /// </summary>
    public int? PanelNumber { get; set; }
    
    /// <summary>
    /// Imaging goal ID for progress tracking
    /// </summary>
    public Guid? ImagingGoalId { get; set; }
    
    /// <summary>
    /// Right Ascension in hours (0-24)
    /// </summary>
    public double RightAscensionHours { get; set; }
    
    /// <summary>
    /// Declination in degrees (-90 to +90)
    /// </summary>
    public double DeclinationDegrees { get; set; }
    
    /// <summary>
    /// Position angle in degrees (0-360), null if not set
    /// </summary>
    public double? PositionAngle { get; set; }
    
    /// <summary>
    /// Filter name to use (AM standard name: L, R, G, B, Ha, Oiii, Sii)
    /// </summary>
    public string? Filter { get; set; }
    
    /// <summary>
    /// NINA filter name to switch to (translated from AM filter using equipment's filter name mappings).
    /// If null, use Filter directly (no mapping configured or names match).
    /// NINA should use this value when changing filters on the filter wheel.
    /// </summary>
    public string? NinaFilterName { get; set; }
    
    /// <summary>
    /// Whether the camera is monochrome (true) or OSC/color (false).
    /// When false (OSC), filter wheel operations should be skipped.
    /// </summary>
    public bool IsMono { get; set; } = true;
    
    /// <summary>
    /// Exposure time in seconds
    /// </summary>
    public int ExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Camera gain (-1 for default/profile setting)
    /// </summary>
    public int Gain { get; set; } = -1;
    
    /// <summary>
    /// Camera offset (-1 for default/profile setting)
    /// </summary>
    public int Offset { get; set; } = -1;
    
    /// <summary>
    /// Binning setting (e.g., "1x1", "2x2")
    /// </summary>
    public string Binning { get; set; } = "1x1";
    
    /// <summary>
    /// Whether to dither after this exposure (server-calculated, based on total goal progress)
    /// </summary>
    public bool DitherAfterExposure { get; set; }
    
    /// <summary>
    /// Dither every X exposures (0 = disabled, used for client-side calculation)
    /// </summary>
    public int DitherEveryX { get; set; }
    
    /// <summary>
    /// Whether client needs to slew (target changed from current)
    /// </summary>
    public bool RequiresSlew { get; set; }
    
    /// <summary>
    /// Whether client needs to change filter
    /// </summary>
    public bool RequiresFilterChange { get; set; }

    /// <summary>
    /// Whether the client should actively automate filter changes for this slot.
    /// False for manual filter workflows where AstroManager or NINA only tracks the active filter.
    /// </summary>
    public bool ShouldAutomateFilterChanges { get; set; } = true;

    /// <summary>
    /// Whether capture attribution should prefer the AstroManager/runtime slot filter over
    /// the image metadata filter reported by N.I.N.A. This is used for direct manual-filter input in AstroManager.
    /// </summary>
    public bool PreferSchedulerFilterForCaptureAttribution { get; set; }
    
    /// <summary>
    /// Minutes to wait (when SlotType is Wait)
    /// </summary>
    public int WaitMinutes { get; set; }
    
    /// <summary>
    /// UTC time until which to wait (for display conversion to local time)
    /// </summary>
    public DateTime? WaitUntilUtc { get; set; }
    
    /// <summary>
    /// Human-readable status message
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Current progress: completed exposures for this goal
    /// </summary>
    public int CompletedExposures { get; set; }
    
    /// <summary>
    /// Current progress: total goal exposures (including RepeatCount)
    /// </summary>
    public int TotalGoalExposures { get; set; }
    
    /// <summary>
    /// Exposure template ID if using templates
    /// </summary>
    public Guid? ExposureTemplateId { get; set; }
    
    /// <summary>
    /// Reason for stopping (when SlotType is Stop)
    /// </summary>
    public StopReason? StopReason { get; set; }
    
    /// <summary>
    /// Run autofocus before the next exposure (user-triggered mid-session AF)
    /// </summary>
    public bool RunAutofocusFirst { get; set; }
    
    /// <summary>
    /// Calibrate guider before the next exposure (user-triggered mid-session calibration)
    /// </summary>
    public bool CalibrateGuiderFirst { get; set; }
}

/// <summary>
/// Reason why the scheduler is stopping
/// </summary>
public enum StopReason
{
    /// <summary>
    /// Unspecified or general stop
    /// </summary>
    Unspecified = 0,
    
    /// <summary>
    /// No more targets available to image tonight
    /// </summary>
    NoMoreTargetsTonight = 1,
    
    /// <summary>
    /// All targets have been completed
    /// </summary>
    AllTargetsComplete = 2,
    
    /// <summary>
    /// Past astronomical dawn - night is over
    /// </summary>
    PastAstronomicalDawn = 3,
    
    /// <summary>
    /// User requested stop
    /// </summary>
    UserRequested = 4
}
