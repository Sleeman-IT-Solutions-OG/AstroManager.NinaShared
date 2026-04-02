namespace Shared.Model.DTO.Client;

/// <summary>
/// Current status of an imaging session for live monitoring
/// </summary>
public class SessionStatusDto
{
    /// <summary>
    /// Session state ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Current session status
    /// </summary>
    public string Status { get; set; } = "Idle";
    
    /// <summary>
    /// Whether session is actively running
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// When the session started (UTC)
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }
    
    /// <summary>
    /// Last activity time (UTC)
    /// </summary>
    public DateTime LastActivityUtc { get; set; }
    
    /// <summary>
    /// Current target being imaged
    /// </summary>
    public Guid? CurrentTargetId { get; set; }
    
    /// <summary>
    /// Current target name
    /// </summary>
    public string? CurrentTargetName { get; set; }
    
    /// <summary>
    /// Current panel ID (for mosaics)
    /// </summary>
    public Guid? CurrentPanelId { get; set; }
    
    /// <summary>
    /// Current panel name
    /// </summary>
    public string? CurrentPanelName { get; set; }
    
    /// <summary>
    /// Current filter being used
    /// </summary>
    public string? CurrentFilter { get; set; }
    
    /// <summary>
    /// Current exposure time in seconds
    /// </summary>
    public int? CurrentExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Status message for display
    /// </summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>
    /// Current operation being performed
    /// </summary>
    public string? CurrentOperation { get; set; }
    
    /// <summary>
    /// Total exposures taken this session
    /// </summary>
    public int TotalExposuresTaken { get; set; }
    
    /// <summary>
    /// Total imaging time in seconds this session
    /// </summary>
    public int TotalImagingTimeSeconds { get; set; }
    
    /// <summary>
    /// Total imaging time formatted (e.g., "2h 30m")
    /// </summary>
    public string TotalImagingTimeFormatted => FormatTime(TotalImagingTimeSeconds);
    
    /// <summary>
    /// Current goal: completed exposures
    /// </summary>
    public int CurrentGoalCompleted { get; set; }
    
    /// <summary>
    /// Current goal: total exposures needed
    /// </summary>
    public int CurrentGoalTotal { get; set; }
    
    /// <summary>
    /// Current goal progress percentage
    /// </summary>
    public double CurrentGoalProgressPercent => CurrentGoalTotal > 0 
        ? Math.Round((double)CurrentGoalCompleted / CurrentGoalTotal * 100, 1) 
        : 0;
    
    /// <summary>
    /// Last error message if any
    /// </summary>
    public string? LastErrorMessage { get; set; }
    
    /// <summary>
    /// Last error time
    /// </summary>
    public DateTime? LastErrorAtUtc { get; set; }
    
    /// <summary>
    /// Observatory name
    /// </summary>
    public string? ObservatoryName { get; set; }
    
    /// <summary>
    /// Equipment name
    /// </summary>
    public string? EquipmentName { get; set; }
    
    /// <summary>
    /// Configuration name
    /// </summary>
    public string? ConfigurationName { get; set; }
    
    /// <summary>
    /// Recently captured images (last 10)
    /// </summary>
    public List<RecentCapturedImageDto> RecentImages { get; set; } = new();
    
    private static string FormatTime(int totalSeconds)
    {
        var hours = totalSeconds / 3600;
        var minutes = (totalSeconds % 3600) / 60;
        if (hours > 0)
            return $"{hours}h {minutes}m";
        return $"{minutes}m";
    }
}

/// <summary>
/// Summary of a recently captured image
/// </summary>
public class RecentCapturedImageDto
{
    public Guid Id { get; set; }
    public DateTime CapturedAtUtc { get; set; }
    public string Filter { get; set; } = string.Empty;
    public int ExposureTimeSeconds { get; set; }
    public string? TargetName { get; set; }
    public string? PanelName { get; set; }
    public double? HFR { get; set; }
    public double? FWHM { get; set; }
    public int? StarCount { get; set; }
    public bool IsAccepted { get; set; }
    public string? ThumbnailBase64 { get; set; }
}

/// <summary>
/// Request to update session status from client
/// </summary>
public class UpdateSessionStatusDto
{
    /// <summary>
    /// Machine name of the client (for SignalR broadcast without DB query)
    /// </summary>
    public string? MachineName { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    public string Status { get; set; } = "Imaging";
    
    /// <summary>
    /// Current target ID
    /// </summary>
    public Guid? CurrentTargetId { get; set; }
    
    /// <summary>
    /// Current target name
    /// </summary>
    public string? CurrentTargetName { get; set; }
    
    /// <summary>
    /// Current panel ID (for mosaics)
    /// </summary>
    public Guid? CurrentPanelId { get; set; }
    
    /// <summary>
    /// Current panel name
    /// </summary>
    public string? CurrentPanelName { get; set; }
    
    /// <summary>
    /// Current imaging goal ID (for tracking active goal)
    /// </summary>
    public Guid? CurrentImagingGoalId { get; set; }
    
    /// <summary>
    /// Current filter
    /// </summary>
    public string? CurrentFilter { get; set; }
    
    /// <summary>
    /// Current exposure time
    /// </summary>
    public int? CurrentExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Status message
    /// </summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>
    /// Scheduler configuration name currently in use
    /// </summary>
    public string? SchedulerConfigurationName { get; set; }
    
    /// <summary>
    /// Whether the scheduler is using the default config (vs explicitly set in sequence)
    /// </summary>
    public bool IsUsingDefaultConfig { get; set; }
    
    /// <summary>
    /// Current operation
    /// </summary>
    public string? CurrentOperation { get; set; }
    
    /// <summary>
    /// Current goal completed count
    /// </summary>
    public int CurrentGoalCompleted { get; set; }
    
    /// <summary>
    /// Current goal total count
    /// </summary>
    public int CurrentGoalTotal { get; set; }
    
    // Equipment connection status
    public bool? IsCameraConnected { get; set; }
    public bool? IsTelescopeConnected { get; set; }
    public bool? IsFocuserConnected { get; set; }
    public bool? IsFilterWheelConnected { get; set; }
    public bool? IsGuiderConnected { get; set; }
    public bool? IsRotatorConnected { get; set; }
    public bool? IsDomeConnected { get; set; }
    public bool? IsWeatherConnected { get; set; }
    public bool? IsFlatPanelConnected { get; set; }
    public bool? IsSafetyMonitorConnected { get; set; }
    
    // Safety Monitor status
    public bool? IsSafe { get; set; }
    
    // Mount status
    public double? MountRightAscension { get; set; }
    public double? MountDeclination { get; set; }
    public double? MountAltitude { get; set; }
    public double? MountAzimuth { get; set; }
    public string? MountSideOfPier { get; set; }
    public string? MountTrackingRate { get; set; }
    public bool? IsMeridianFlipping { get; set; }
    public DateTime? MeridianFlipStartedUtc { get; set; }
    public bool? IsTracking { get; set; }
    public bool? IsParked { get; set; }
    public bool? IsSlewing { get; set; }
    
    // Focuser status
    public int? FocuserPosition { get; set; }
    public double? FocuserTemperature { get; set; }
    public bool? IsFocuserMoving { get; set; }
    
    // Filter Wheel status
    public string? SelectedFilter { get; set; }
    public List<string>? FilterWheelFilters { get; set; }
    
    // Rotator status
    public double? RotatorAngle { get; set; }
    public bool? RotatorReverse { get; set; }
    public bool? RotatorCanReverse { get; set; }
    
    // Flat Panel status
    public bool? FlatPanelLightOn { get; set; }
    public int? FlatPanelBrightness { get; set; }
    public string? FlatPanelCoverState { get; set; } // Unknown, NeitherOpenNorClosed, Open, Closed, Moving, Error
    public bool? FlatPanelSupportsOpenClose { get; set; }
    
    // Guider status
    public double? GuidingRaRms { get; set; }
    public double? GuidingDecRms { get; set; }
    public bool? IsGuiding { get; set; }
    public bool? IsCalibrating { get; set; }
    
    // Camera status
    public double? CameraTemperature { get; set; }
    public double? CameraTargetTemperature { get; set; }
    public double? CoolerPower { get; set; }
    public bool? IsCoolerOn { get; set; }
    public int? CameraBinning { get; set; }
    public bool? IsExposing { get; set; }
    public double? ExposureDurationSeconds { get; set; }
    public double? ExposureElapsedSeconds { get; set; }
    
    // Last autofocus report and history
    public AutofocusReportDto? CurrentAutofocusReport { get; set; }
    public AutofocusReportDto? LastAutofocusReport { get; set; }
    public List<AutofocusReportDto>? AutofocusHistory { get; set; }
    
    // Last plate solve report and history
    public PlateSolveReportDto? LastPlateSolveReport { get; set; }
    public List<PlateSolveReportDto>? PlateSolveHistory { get; set; }
    
    // Image history (recent captures)
    public List<ImageHistoryItemDto>? ImageHistory { get; set; }
    
    // Sequence status
    public bool? IsSequenceRunning { get; set; }
    /// <summary>Name of the currently running sequence</summary>
    public string? SequenceName { get; set; }
    /// <summary>Name of the loaded sequence (even when not running)</summary>
    public string? LoadedSequenceName { get; set; }
    /// <summary>Detected sequence folder in NINA.</summary>
    public string? SequenceFilesFolder { get; set; }
    /// <summary>Available sequence files snapshot.</summary>
    public List<SequenceFileEntryDto>? AvailableSequenceFiles { get; set; }
    /// <summary>Current sequence tree snapshot captured by plugin.</summary>
    public SequenceTreeDto? SequenceTree { get; set; }
    
    // Weather data (current values)
    public double? WeatherTemperature { get; set; }
    public double? WeatherHumidity { get; set; }
    public double? WeatherDewPoint { get; set; }
    public double? WeatherPressure { get; set; }
    public double? WeatherCloudCover { get; set; }
    public double? WeatherRainRate { get; set; }
    public double? WeatherWindSpeed { get; set; }
    public double? WeatherWindDirection { get; set; }
    public double? WeatherSkyQuality { get; set; }
    public double? WeatherSkyTemperature { get; set; }
    public double? WeatherStarFWHM { get; set; }
    
    /// <summary>
    /// Weather data point for historical tracking
    /// </summary>
    public WeatherDataPointDto? WeatherDataPoint { get; set; }
}

/// <summary>
/// Weather data point for historical tracking and graphs
/// </summary>
public class WeatherDataPointDto
{
    public Guid Id { get; set; }
    public Guid ClientLicenseId { get; set; }
    public Guid? SessionExecutionId { get; set; }
    public DateTime TimestampUtc { get; set; }
    
    // Temperature, Humidity, DewPoint (Graph 1)
    public double? Temperature { get; set; }
    public double? Humidity { get; set; }
    public double? DewPoint { get; set; }
    
    // Pressure (status only, no graph)
    public double? Pressure { get; set; }
    
    // Cloud Cover, Rain Rate (Graph 2)
    public double? CloudCover { get; set; }
    public double? RainRate { get; set; }
    
    // Wind Direction, Wind Speed (Graph 3)
    public double? WindSpeed { get; set; }
    public double? WindDirection { get; set; }
    public double? WindGust { get; set; }
    
    // Sky Quality, Sky Temperature (Graph 4)
    public double? SkyQuality { get; set; }
    public double? SkyTemperature { get; set; }
    
    // Additional data
    public double? StarFWHM { get; set; }
}
