namespace Shared.Model.Enums;

/// <summary>
/// Action to take when a configured safety/quality limit is violated during scheduling.
/// </summary>
public enum SchedulerViolationAction
{
    /// <summary>
    /// Stop the full NINA sequence immediately.
    /// </summary>
    StopScheduler = 0,

    /// <summary>
    /// Park the mount (if possible) and retry after a configured delay.
    /// </summary>
    ParkAndRetry = 1,

    /// <summary>
    /// Stop mount tracking and retry after a configured delay.
    /// </summary>
    StopTrackingAndRetry = 2,

    /// <summary>
    /// Stop only the AstroManager scheduler instruction and allow the remaining NINA sequence to continue.
    /// </summary>
    StopAmScheduler = 3,

    /// <summary>
    /// Trigger a guider calibration and continue.
    /// </summary>
    CalibrateGuider = 4,

    /// <summary>
    /// Trigger an autofocus run and continue.
    /// </summary>
    RunAutofocus = 5,

    /// <summary>
    /// Reconnect critical imaging equipment (camera, mount, guider) and continue.
    /// </summary>
    ReconnectEquipment = 6,

    /// <summary>
    /// Reconnect all available equipment and continue.
    /// </summary>
    ReconnectAllEquipment = 7,

    /// <summary>
    /// Send a safety alert email and continue.
    /// </summary>
    SendEmail = 8,

    /// <summary>
    /// Create an AstroManager notification and continue.
    /// </summary>
    CreateNotification = 9,

    /// <summary>
    /// Adjust the camera cooler target temperature by a signed delta in °C and continue.
    /// </summary>
    AdjustCoolerTemperatureDelta = 10
}
