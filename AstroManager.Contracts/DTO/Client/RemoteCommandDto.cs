namespace Shared.Model.DTO.Client;

/// <summary>
/// Types of remote commands that can be sent to a telescope client
/// </summary>
public enum RemoteCommandType
{
    /// <summary>Stop the current sequence</summary>
    StopSequence,
    
    /// <summary>Pause the current sequence</summary>
    PauseSequence,
    
    /// <summary>Resume a paused sequence</summary>
    ResumeSequence,
    
    /// <summary>Skip the current target and move to next</summary>
    SkipTarget,
    
    /// <summary>Run autofocus</summary>
    RunAutofocus,
    
    /// <summary>Park the telescope</summary>
    ParkTelescope,
    
    /// <summary>Unpark the telescope</summary>
    UnparkTelescope,
    
    /// <summary>Warm up the camera</summary>
    WarmCamera,
    
    /// <summary>Cool down the camera</summary>
    CoolCamera,
    
    /// <summary>Turn off camera cooler without warmup</summary>
    TurnOffCooler,
    
    /// <summary>Reconnect all equipment</summary>
    ReconnectEquipment,
    
    /// <summary>Refresh configuration from server</summary>
    RefreshConfig,
    
    /// <summary>Start a new session</summary>
    StartSession,
    
    /// <summary>Emergency stop - immediate halt of all operations</summary>
    EmergencyStop,
    
    /// <summary>Center on current target</summary>
    CenterTarget,
    
    /// <summary>Run meridian flip</summary>
    MeridianFlip,
    
    /// <summary>Stop guiding</summary>
    StopGuiding,
    
    /// <summary>Start guiding</summary>
    StartGuiding,
    
    /// <summary>Calibrate guider</summary>
    CalibrateGuider,
    
    /// <summary>Slew telescope to coordinates</summary>
    SlewToCoordinates,
    
    /// <summary>Slew and center on coordinates (with plate solve)</summary>
    SlewAndCenter,
    
    /// <summary>Home telescope</summary>
    HomeTelescope,
    
    /// <summary>Set telescope tracking state (on/off)</summary>
    SetTracking,
    
    /// <summary>Set telescope tracking rate (Sidereal, Lunar, Solar, King)</summary>
    SetTrackingRate,
    
    /// <summary>Take single exposure</summary>
    TakeExposure,
    
    /// <summary>Move focuser to position</summary>
    MoveFocuser,
    
    /// <summary>Change filter</summary>
    ChangeFilter,
    
    /// <summary>Connect equipment</summary>
    ConnectEquipment,
    
    /// <summary>Disconnect equipment</summary>
    DisconnectEquipment,
    
    /// <summary>Move rotator to angle</summary>
    MoveRotator,
    
    /// <summary>Reverse rotator direction</summary>
    ReverseRotator,
    
    /// <summary>Set flat panel brightness</summary>
    SetFlatPanelBrightness,
    
    /// <summary>Toggle flat panel on/off</summary>
    ToggleFlatPanel,
    
    /// <summary>Open or close flat panel cover</summary>
    OpenCloseFlatPanelCover,
    
    /// <summary>Plate solve current position</summary>
    PlateSolve,
    
    /// <summary>Plate solve and sync mount</summary>
    PlateSolveAndSync,
    
    /// <summary>Stop all equipment operations</summary>
    StopAll,
    
    /// <summary>Stop/abort mount slew</summary>
    StopMount,
    
    /// <summary>Stop/abort camera exposure</summary>
    StopExposure,
    
    /// <summary>Stop/abort autofocus</summary>
    StopAutofocus,
    
    /// <summary>Stop/halt focuser movement</summary>
    StopFocuser,
    
    /// <summary>Stop/halt rotator movement</summary>
    StopRotator,
    
    /// <summary>Connect all equipment</summary>
    ConnectAllEquipment,
    
    /// <summary>Disconnect all equipment</summary>
    DisconnectAllEquipment,
    
    /// <summary>Custom command with parameters</summary>
    Custom,
    
    /// <summary>Start the loaded sequence</summary>
    StartSequence,
    
    /// <summary>Reset and start the loaded sequence (resets progress to beginning)</summary>
    ResetAndStartSequence,
    
    /// <summary>Load a sequence file by path</summary>
    LoadSequence,
    
    /// <summary>List available sequence files</summary>
    ListSequenceFiles,
    
    /// <summary>Get the current sequence tree structure with status</summary>
    GetSequenceTree,
    
    /// <summary>Restart scheduler session with a new configuration</summary>
    RestartSessionWithConfig,

    /// <summary>Stop only the AstroManager scheduler instruction and continue the remaining NINA sequence</summary>
    StopAmScheduler
}

/// <summary>
/// Status of a remote command
/// </summary>
public enum RemoteCommandStatus
{
    /// <summary>Command is pending execution</summary>
    Pending,
    
    /// <summary>Command has been received by client</summary>
    Received,
    
    /// <summary>Command is being executed</summary>
    Executing,
    
    /// <summary>Command completed successfully</summary>
    Completed,
    
    /// <summary>Command failed</summary>
    Failed,
    
    /// <summary>Command was cancelled</summary>
    Cancelled,
    
    /// <summary>Command expired before execution</summary>
    Expired
}

/// <summary>
/// DTO for remote commands sent to telescope clients
/// </summary>
public class RemoteCommandDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The client license this command is for
    /// </summary>
    public Guid ClientLicenseId { get; set; }
    
    /// <summary>
    /// Type of command
    /// </summary>
    public RemoteCommandType CommandType { get; set; }
    
    /// <summary>
    /// Current status of the command
    /// </summary>
    public RemoteCommandStatus Status { get; set; } = RemoteCommandStatus.Pending;
    
    /// <summary>
    /// Optional parameters for the command (JSON)
    /// </summary>
    public string? Parameters { get; set; }
    
    /// <summary>
    /// Priority (higher = more urgent, 0 = normal)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// When the command was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the command expires (null = never)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// When the command was received by the client
    /// </summary>
    public DateTime? ReceivedAt { get; set; }
    
    /// <summary>
    /// When the command finished executing
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Result message from execution
    /// </summary>
    public string? ResultMessage { get; set; }
    
    /// <summary>
    /// User who created the command
    /// </summary>
    public Guid CreatedByUserId { get; set; }
    
    /// <summary>
    /// Optional note from user
    /// </summary>
    public string? Note { get; set; }
}

/// <summary>
/// DTO for creating a new remote command
/// </summary>
public class CreateRemoteCommandDto
{
    public Guid ClientLicenseId { get; set; }
    public RemoteCommandType CommandType { get; set; }
    public string? Parameters { get; set; }
    public int Priority { get; set; } = 0;
    public int? ExpiresInMinutes { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// DTO for updating command status (from client)
/// </summary>
public class UpdateRemoteCommandStatusDto
{
    public Guid CommandId { get; set; }
    public RemoteCommandStatus Status { get; set; }
    public string? ResultMessage { get; set; }
}

/// <summary>
/// DTO for client status information
/// </summary>
public class ClientStatusDto
{
    public Guid ClientLicenseId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    
    /// <summary>
    /// Is the client currently connected/online
    /// </summary>
    public bool IsOnline { get; set; }
    
    /// <summary>
    /// Last heartbeat received
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }
    
    /// <summary>
    /// Current client state
    /// </summary>
    public ClientState State { get; set; } = ClientState.Idle;
    
    /// <summary>
    /// Current target being imaged (if any)
    /// </summary>
    public string? CurrentTarget { get; set; }
    
    /// <summary>
    /// Current scheduled target ID (for compact target view)
    /// </summary>
    public Guid? CurrentTargetId { get; set; }
    
    /// <summary>
    /// Current imaging goal ID (for tracking active goal)
    /// </summary>
    public Guid? CurrentImagingGoalId { get; set; }
    
    /// <summary>
    /// Current panel ID (for mosaics)
    /// </summary>
    public Guid? CurrentPanelId { get; set; }
    
    /// <summary>
    /// Current panel name
    /// </summary>
    public string? CurrentPanelName { get; set; }
    
    /// <summary>
    /// Current exposure time in seconds
    /// </summary>
    public int? CurrentExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Scheduler configuration name currently in use
    /// </summary>
    public string? SchedulerConfigurationName { get; set; }
    
    /// <summary>
    /// Whether the scheduler is using the default config (vs explicitly set in sequence)
    /// </summary>
    public bool IsUsingDefaultConfig { get; set; }
    
    /// <summary>
    /// Current filter in use
    /// </summary>
    public string? CurrentFilter { get; set; }
    
    /// <summary>
    /// Current exposure progress (0-100)
    /// </summary>
    public double? ExposureProgress { get; set; }
    
    /// <summary>
    /// Number of exposures completed in current session
    /// </summary>
    public int? ExposuresCompleted { get; set; }
    
    /// <summary>
    /// Total exposures planned for current session
    /// </summary>
    public int? ExposuresPlanned { get; set; }
    
    /// <summary>
    /// Camera temperature
    /// </summary>
    public double? CameraTemperature { get; set; }
    
    /// <summary>
    /// Camera cooler power percentage
    /// </summary>
    public double? CoolerPower { get; set; }
    
    /// <summary>
    /// Is the telescope tracking
    /// </summary>
    public bool? IsTracking { get; set; }
    
    /// <summary>
    /// Is the telescope parked
    /// </summary>
    public bool? IsParked { get; set; }
    
    /// <summary>
    /// Current guiding RMS in arcseconds
    /// </summary>
    public double? GuidingRmsArcsec { get; set; }
    
    /// <summary>
    /// Any error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Current operation being performed (e.g., "AF: Measuring HFR (Pos: 23000)")
    /// </summary>
    public string? CurrentOperation { get; set; }
    
    /// <summary>
    /// Client software version
    /// </summary>
    public string? ClientVersion { get; set; }
    
    /// <summary>
    /// Pending commands count
    /// </summary>
    public int PendingCommands { get; set; }
    
    /// <summary>
    /// IP address of the client
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Observatory name
    /// </summary>
    public string? ObservatoryName { get; set; }
    
    /// <summary>
    /// Equipment name
    /// </summary>
    public string? EquipmentName { get; set; }
    
    /// <summary>
    /// Observatory ID for filtering targets
    /// </summary>
    public Guid? ObservatoryId { get; set; }
    
    /// <summary>
    /// Equipment ID for loading FOV data
    /// </summary>
    public Guid? EquipmentId { get; set; }
    
    /// <summary>
    /// Last captured image thumbnail (base64 encoded, low-res JPEG)
    /// </summary>
    public string? LastImageThumbnail { get; set; }
    
    /// <summary>
    /// Recent image history (last N images)
    /// </summary>
    public List<ImageHistoryItemDto>? ImageHistory { get; set; }
    
    /// <summary>
    /// Last image capture time
    /// </summary>
    public DateTime? LastImageTime { get; set; }
    
    /// <summary>
    /// Last image file name
    /// </summary>
    public string? LastImageFileName { get; set; }
    
    /// <summary>
    /// Last image stats summary (e.g. "HFR: 2.5, Stars: 150")
    /// </summary>
    public string? LastImageStats { get; set; }
    
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
    
    /// <summary>
    /// Is the mount currently performing a meridian flip
    /// </summary>
    public bool? IsMeridianFlipping { get; set; }
    
    /// <summary>
    /// Time when the last meridian flip started (UTC)
    /// </summary>
    public DateTime? MeridianFlipStartedUtc { get; set; }
    
    // Focuser status
    public int? FocuserPosition { get; set; }
    public double? FocuserTemperature { get; set; }
    
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
    public string? FlatPanelCoverState { get; set; }
    public bool? FlatPanelSupportsOpenClose { get; set; }
    
    // Guider status
    public double? GuidingRaRms { get; set; }
    public double? GuidingDecRms { get; set; }
    public bool? IsGuiding { get; set; }
    public bool? IsCalibrating { get; set; }
    
    /// <summary>
    /// Calculated Total RMS from RA and Dec RMS: sqrt(RA^2 + DEC^2)
    /// </summary>
    public double? GuidingRmsTotal => (GuidingRaRms.HasValue && GuidingDecRms.HasValue)
        ? Math.Sqrt(GuidingRaRms.Value * GuidingRaRms.Value + GuidingDecRms.Value * GuidingDecRms.Value)
        : null;
    
    // Camera status
    public double? CameraTargetTemperature { get; set; }
    public bool? IsCoolerOn { get; set; }
    public int? CameraBinning { get; set; }
    public double? CurrentExposureTime { get; set; }
    public bool? IsExposing { get; set; }
    public double? ExposureDurationSeconds { get; set; }
    public double? ExposureElapsedSeconds { get; set; }
    
    // Mount movement status
    public bool? IsSlewing { get; set; }
    
    // Focuser movement status
    public bool? IsFocuserMoving { get; set; }
    
    // Last autofocus report and history
    public AutofocusReportDto? CurrentAutofocusReport { get; set; }
    public AutofocusReportDto? LastAutofocusReport { get; set; }
    public List<AutofocusReportDto>? AutofocusHistory { get; set; }
    
    // Last plate solve report and history
    public PlateSolveReportDto? LastPlateSolveReport { get; set; }
    public List<PlateSolveReportDto>? PlateSolveHistory { get; set; }
    
    // Sequence status
    public bool? IsSequenceRunning { get; set; }
    public string? SequenceName { get; set; }
    public string? SequenceFilesFolder { get; set; }
    public List<SequenceFileEntryDto>? AvailableSequenceFiles { get; set; }
    public SequenceTreeDto? SequenceTree { get; set; }
    
    /// <summary>
    /// Scheduler mode (Auto or Manual)
    /// </summary>
    public SchedulerMode SchedulerMode { get; set; } = SchedulerMode.Auto;
    
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
}

/// <summary>
/// DTO for autofocus report with graph data
/// </summary>
public class AutofocusReportDto
{
    public DateTime CompletedAt { get; set; }
    public bool Success { get; set; }
    public int FinalPosition { get; set; }
    public double FinalHfr { get; set; }
    public double Temperature { get; set; }
    public string? Filter { get; set; }
    public List<AutofocusDataPointDto> DataPoints { get; set; } = new();
    public string? FittingMethod { get; set; }
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// R² value for hyperbolic fitting (0-1, higher is better)
    /// </summary>
    public double? RSquaredHyperbolic { get; set; }
    
    /// <summary>
    /// R² value for parabolic fitting (0-1, higher is better)
    /// </summary>
    public double? RSquaredParabolic { get; set; }
    
    /// <summary>
    /// R² value for the selected fitting method
    /// </summary>
    public double? RSquared { get; set; }
}

/// <summary>
/// DTO for plate solve result
/// </summary>
public class PlateSolveReportDto
{
    public DateTime CompletedAt { get; set; }
    public bool Success { get; set; }
    public double? SolvedRa { get; set; }
    public double? SolvedDec { get; set; }
    public double? Rotation { get; set; }
    public double? PixelScale { get; set; }
    public string? RaFormatted { get; set; }
    public string? DecFormatted { get; set; }
    public bool WasSynced { get; set; }
    public double? SolveDurationSeconds { get; set; }
    public double? SeparationArcsec { get; set; }
    /// <summary>
    /// RA separation in arcseconds (positive = East)
    /// </summary>
    public double? RaSeparationArcsec { get; set; }
    /// <summary>
    /// DEC separation in arcseconds (positive = North)
    /// </summary>
    public double? DecSeparationArcsec { get; set; }
    public string? FailureReason { get; set; }
}

/// <summary>
/// Single data point for AF graph
/// </summary>
public class AutofocusDataPointDto
{
    public int Position { get; set; }
    public double Hfr { get; set; }
    public int StarCount { get; set; }
}

/// <summary>
/// DTO for image history item
/// </summary>
public class ImageHistoryItemDto : Common.IImageDisplayData
{
    public Guid Id { get; set; }
    public string? ThumbnailBase64 { get; set; }
    public string? RawThumbnailBase64 { get; set; } // Non-stretched version for auto-stretch
    public string? MicroThumbnailBase64 { get; set; } // ~50px thumbnail for list views
    public string? FileName { get; set; }
    public string? Stats { get; set; }
    public DateTime CapturedAt { get; set; }
    
    // Enhanced image metadata
    public string? Filter { get; set; }
    public double? ExposureTime { get; set; }
    public double? HFR { get; set; }
    public int? DetectedStars { get; set; }
    public string? TargetName { get; set; }
    public int? PanelNumber { get; set; }
    public Guid? PanelImagingGoalId { get; set; }
    public int? Gain { get; set; }
    public int? Offset { get; set; }
    public double? CameraTemp { get; set; }
    public int? Binning { get; set; }
    
    // Image statistics
    public double? Mean { get; set; }
    public double? Median { get; set; }
    public double? StdDev { get; set; }
    public int? GradeScore { get; set; }
    public string? GradeBand { get; set; }
    public bool? IsAccepted { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public long? ADU { get; set; }
    
    // Coordinates and position at capture time
    public double? RightAscension { get; set; } // hours
    public double? Declination { get; set; } // degrees
    public double? Altitude { get; set; } // degrees - altitude of target at capture time
    public double? Azimuth { get; set; } // degrees - azimuth of target at capture time
    
    // Equipment
    public string? CameraName { get; set; }
    public string? TelescopeName { get; set; }
    public double? FocalLength { get; set; }
    public double? PixelScale { get; set; }
    public int? FocuserPosition { get; set; }
    
    // Image dimensions
    public int? ImageWidth { get; set; }
    public int? ImageHeight { get; set; }
    public int? BitDepth { get; set; }
    
    // Weather data at capture time
    public double? WeatherTemperature { get; set; }
    public double? WeatherHumidity { get; set; }
    public double? WeatherDewPoint { get; set; }
    public double? WeatherPressure { get; set; }
    public double? WeatherCloudCover { get; set; }
    public double? WeatherWindSpeed { get; set; }
    public double? WeatherSkyQuality { get; set; }
    
    // Guiding RMS (arcseconds)
    public double? GuidingRmsRA { get; set; }
    public double? GuidingRmsDec { get; set; }
    public double? GuidingRmsTotal { get; set; }
    
    // IImageDisplayData interface implementations (explicit for property name mappings)
    double Common.IImageDisplayData.ExposureTimeSeconds => ExposureTime ?? 0;
    int Common.IImageDisplayData.Gain => Gain ?? -1;
    int Common.IImageDisplayData.Offset => Offset ?? -1;
    int Common.IImageDisplayData.Binning => Binning ?? 1;
    double? Common.IImageDisplayData.CameraTemperature => CameraTemp;
    double? Common.IImageDisplayData.HFR => HFR;
    int? Common.IImageDisplayData.StarCount => DetectedStars;
    double? Common.IImageDisplayData.MeanADU => Mean;
    double? Common.IImageDisplayData.NoiseADU => StdDev;
    double? Common.IImageDisplayData.MedianADU => Median;
    double? Common.IImageDisplayData.SNR => null; // Not available in ImageHistoryItemDto
    int? Common.IImageDisplayData.GradeScore => GradeScore;
    string? Common.IImageDisplayData.GradeBand => GradeBand;
    double? Common.IImageDisplayData.Altitude => Altitude;
    double? Common.IImageDisplayData.Azimuth => Azimuth;
}

/// <summary>
/// DTO for uploading image thumbnail from NINA plugin
/// </summary>
public class UploadImageThumbnailDto
{
    /// <summary>
    /// Base64 encoded JPEG thumbnail (stretched for display)
    /// </summary>
    public string ThumbnailBase64 { get; set; } = string.Empty;
    
    /// <summary>
    /// Base64 encoded raw (non-stretched) thumbnail for auto-stretch
    /// </summary>
    public string? RawThumbnailBase64 { get; set; }
    
    /// <summary>
    /// Micro thumbnail (~50px) for list views - much smaller file size
    /// </summary>
    public string? MicroThumbnailBase64 { get; set; }
    
    /// <summary>
    /// Original file name
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// Image stats summary (e.g. "HFR: 2.5, Stars: 150")
    /// </summary>
    public string? Stats { get; set; }
    
    // Enhanced image metadata from FITS headers
    public string? Filter { get; set; }
    public double? ExposureTime { get; set; }
    public double? HFR { get; set; }
    public int? DetectedStars { get; set; }
    public string? TargetName { get; set; }
    public int? PanelNumber { get; set; }
    public int? Gain { get; set; }
    public int? Offset { get; set; }
    public double? CameraTemp { get; set; }
    public int? Binning { get; set; }
    
    // Image statistics
    public double? Mean { get; set; }
    public double? Median { get; set; }
    public double? StdDev { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public long? ADU { get; set; }
    
    // Coordinates and position at capture time
    public double? RightAscension { get; set; } // hours
    public double? Declination { get; set; } // degrees
    public double? Altitude { get; set; } // degrees - altitude of target at capture time
    public double? Azimuth { get; set; } // degrees - azimuth of target at capture time
    
    // Equipment
    public string? CameraName { get; set; }
    public string? TelescopeName { get; set; }
    public string? MountName { get; set; }
    public double? FocalLength { get; set; }
    public double? Aperture { get; set; }
    public double? PixelScale { get; set; }
    public double? PixelSizeX { get; set; }
    public double? PixelSizeY { get; set; }
    public double? RotatorAngle { get; set; }
    public int? FocuserPosition { get; set; }
    
    // Location
    public double? SiteLatitude { get; set; }
    public double? SiteLongitude { get; set; }
    public double? SiteElevation { get; set; }
    
    // Guiding RMS (arcseconds)
    public double? GuidingRmsRA { get; set; }
    public double? GuidingRmsDec { get; set; }
    public double? GuidingRmsTotal { get; set; }
    
    // Image dimensions
    public int? ImageWidth { get; set; }
    public int? ImageHeight { get; set; }
    public int? BitDepth { get; set; }
    
    // Software
    public string? Software { get; set; }
    
    // Timestamp
    /// <summary>
    /// Actual capture time from FITS header (DATE-OBS). If null, server uses current UTC time.
    /// </summary>
    public DateTime? CapturedAt { get; set; }
    
    // Scheduler context (from AstroManager scheduler)
    /// <summary>
    /// Scheduled target ID if captured via AstroManager scheduler
    /// </summary>
    public Guid? ScheduledTargetId { get; set; }
    
    /// <summary>
    /// Imaging goal ID if captured via AstroManager scheduler
    /// </summary>
    public Guid? ImagingGoalId { get; set; }
    
    /// <summary>
    /// Panel ID for mosaic targets if captured via AstroManager scheduler
    /// </summary>
    public Guid? PanelId { get; set; }
    
    // Weather data at capture time
    public double? WeatherTemperature { get; set; }
    public double? WeatherHumidity { get; set; }
    public double? WeatherDewPoint { get; set; }
    public double? WeatherPressure { get; set; }
    public double? WeatherCloudCover { get; set; }
    public double? WeatherWindSpeed { get; set; }
    public double? WeatherSkyQuality { get; set; }
}

/// <summary>
/// Request DTO for auto-stretch image processing
/// </summary>
public class AutoStretchRequestDto
{
    public string ThumbnailBase64 { get; set; } = string.Empty;
}

/// <summary>
/// Client operational state
/// </summary>
public enum ClientState
{
    Idle,
    Initializing,
    Cooling,
    Slewing,
    Centering,
    Autofocusing,
    Exposing,
    Dithering,
    Downloading,
    Processing,
    Waiting,
    Paused,
    Parking,
    Parked,
    Error,
    Disconnected
}

/// <summary>
/// Scheduler control mode for a client
/// </summary>
public enum SchedulerMode
{
    /// <summary>
    /// Automatic mode - scheduler picks targets based on algorithm
    /// </summary>
    Auto,
    
    /// <summary>
    /// Manual mode - user queues targets manually
    /// </summary>
    Manual
}
