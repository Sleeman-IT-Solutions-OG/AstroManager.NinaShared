namespace Shared.Model.Enums;

/// <summary>
/// Type of error that occurred during imaging
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Plate solve operation failed
    /// </summary>
    PlateSolveFailed = 0,
    
    /// <summary>
    /// Failed to start guiding
    /// </summary>
    GuidingFailed = 1,
    
    /// <summary>
    /// Guiding was lost during imaging
    /// </summary>
    GuidingLost = 2,
    
    /// <summary>
    /// Filter wheel error (stuck, wrong position, etc.)
    /// </summary>
    FilterWheelError = 3,
    
    /// <summary>
    /// Focuser error
    /// </summary>
    FocuserError = 4,
    
    /// <summary>
    /// Rotator error
    /// </summary>
    RotatorError = 5,
    
    /// <summary>
    /// Camera error (timeout, disconnection, etc.)
    /// </summary>
    CameraError = 6,
    
    /// <summary>
    /// Telescope/mount error
    /// </summary>
    TelescopeError = 7,
    
    /// <summary>
    /// Dome error
    /// </summary>
    DomeError = 8,
    
    /// <summary>
    /// Weather alert triggered
    /// </summary>
    WeatherAlert = 9,
    
    /// <summary>
    /// Safety event (cloud sensor, rain, etc.)
    /// </summary>
    SafetyEvent = 10,
    
    /// <summary>
    /// Unknown or unclassified error
    /// </summary>
    Unknown = 99
}
