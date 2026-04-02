namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for a night imaging report
/// </summary>
public class NightReportDto
{
    public Guid Id { get; set; }
    public Guid ClientLicenseId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    
    // Night timing
    public DateTime NightDate { get; set; }
    public DateTime SessionStart { get; set; }
    public DateTime SessionEnd { get; set; }
    public double TotalDurationHours { get; set; }
    public double EffectiveImagingHours { get; set; }
    
    // Observatory info
    public string? ObservatoryName { get; set; }
    public string? EquipmentName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    
    // Summary statistics
    public int TotalExposures { get; set; }
    public int SuccessfulExposures { get; set; }
    public int FailedExposures { get; set; }
    public double SuccessRate { get; set; }
    public double TotalIntegrationMinutes { get; set; }
    
    // Weather/conditions
    public double? AverageTemperature { get; set; }
    public double? MinTemperature { get; set; }
    public double? MaxTemperature { get; set; }
    public double? AverageHumidity { get; set; }
    public double? AverageDewPoint { get; set; }
    public double? AveragePressure { get; set; }
    public double? AverageCloudCover { get; set; }
    public double? AverageWindSpeed { get; set; }
    public double? AverageSkyQuality { get; set; }
    public double? AverageSkyTemperature { get; set; }
    public double? AverageSeeingArcsec { get; set; }
    
    // Weather history for graphs
    public List<WeatherDataPointDto> WeatherHistory { get; set; } = new();
    
    // Guiding stats
    public double? AverageGuideRmsArcsec { get; set; }
    public double? MinGuideRmsArcsec { get; set; }
    public double? MaxGuideRmsArcsec { get; set; }
    
    // HFR stats
    public double? AverageHfr { get; set; }
    public double? MinHfr { get; set; }
    public double? MaxHfr { get; set; }
    
    // Targets imaged
    public List<NightReportTargetDto> Targets { get; set; } = new();
    
    // Filter breakdown
    public List<NightReportFilterDto> FilterBreakdown { get; set; } = new();
    
    // Events/issues
    public List<NightReportEventDto> Events { get; set; } = new();
    
    // Autofocus runs
    public int AutofocusRuns { get; set; }
    public int SuccessfulAutofocusRuns { get; set; }
    
    // Meridian flips
    public int MeridianFlips { get; set; }
    
    // Time series data for graphs
    public List<NightReportDataPointDto> HfrHistory { get; set; } = new();
    public List<NightReportDataPointDto> StarCountHistory { get; set; } = new();
    public List<NightReportDataPointDto> GuidingRmsHistory { get; set; } = new();
    
    // Detailed history
    public List<NightReportAutofocusDto> AutofocusHistory { get; set; } = new();
    public List<NightReportPlatesolvDto> PlatesolveHistory { get; set; } = new();
    public List<NightReportGoalProgressDto> GoalProgress { get; set; } = new();
    
    // Wait time statistics
    public double TotalWaitTimeMinutes { get; set; }
    public double AverageWaitBetweenExposuresSeconds { get; set; }
    
    // Full log for the night
    public List<SequencerLogEntryDto> Logs { get; set; } = new();
    
    // Detailed captured images list
    public List<NightReportCapturedImageDto> CapturedImages { get; set; } = new();
    
    // Report generation
    public DateTime GeneratedAt { get; set; }
    public string? PdfUrl { get; set; }
    public bool EmailSent { get; set; }
    public DateTime? EmailSentAt { get; set; }
}

/// <summary>
/// Target summary for night report
/// </summary>
public class NightReportTargetDto
{
    public string TargetName { get; set; } = string.Empty;
    public double RaHours { get; set; }
    public double DecDegrees { get; set; }
    public int ExposureCount { get; set; }
    public double IntegrationMinutes { get; set; }
    public List<NightReportFilterDto> FilterBreakdown { get; set; } = new();
    public double? AverageAltitude { get; set; }
    public double? AverageHfr { get; set; }
    public string? ThumbnailBase64 { get; set; }
}

/// <summary>
/// Filter breakdown for night report
/// </summary>
public class NightReportFilterDto
{
    public string FilterName { get; set; } = string.Empty;
    public int ExposureCount { get; set; }
    public double ExposureTimeSeconds { get; set; }
    public double TotalIntegrationMinutes { get; set; }
    public double? AverageHfr { get; set; }
}

/// <summary>
/// Significant event during the night
/// </summary>
public class NightReportEventDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SequencerLogLevel Severity { get; set; }
}

/// <summary>
/// Settings for automatic night report generation
/// </summary>
public class NightReportSettingsDto
{
    public Guid ClientLicenseId { get; set; }
    public bool AutoGenerateReport { get; set; } = true;
    public bool AutoEmailReport { get; set; } = false;
    public string? EmailRecipients { get; set; }
    public bool IncludeThumbnails { get; set; } = true;
    public bool IncludeDetailedLogs { get; set; } = false;
}

/// <summary>
/// Request to generate a night report
/// </summary>
public class GenerateNightReportDto
{
    public Guid ClientLicenseId { get; set; }
    public DateTime NightDate { get; set; }
    public bool SendEmail { get; set; } = false;
    public string? EmailOverride { get; set; }
}

/// <summary>
/// Request to send a night report via email
/// </summary>
public class SendEmailDto
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Time series data point for night report graphs
/// </summary>
public class NightReportDataPointDto
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? TargetName { get; set; }
    public string? Filter { get; set; }
}

/// <summary>
/// Autofocus run details for night report
/// </summary>
public class NightReportAutofocusDto
{
    public DateTime Timestamp { get; set; }
    public string? TargetName { get; set; }
    public string? Filter { get; set; }
    public bool Success { get; set; }
    public double? InitialHfr { get; set; }
    public double? FinalHfr { get; set; }
    public int? InitialPosition { get; set; }
    public int? FinalPosition { get; set; }
    public double? Temperature { get; set; }
    public double DurationSeconds { get; set; }
}

/// <summary>
/// Platesolve details for night report
/// </summary>
public class NightReportPlatesolvDto
{
    public DateTime Timestamp { get; set; }
    public string? TargetName { get; set; }
    public bool Success { get; set; }
    public double? RaHours { get; set; }
    public double? DecDegrees { get; set; }
    public double? RaErrorArcsec { get; set; }
    public double? DecErrorArcsec { get; set; }
    public double? RotationDegrees { get; set; }
    public double? PixelScale { get; set; }
    public double DurationSeconds { get; set; }
}

/// <summary>
/// Imaging goal progress for night report
/// </summary>
public class NightReportGoalProgressDto
{
    public string TargetName { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
    public double ExposureTimeSeconds { get; set; }
    public int GoalCount { get; set; }
    public int CompletedCount { get; set; }
    public double CompletionPercent { get; set; }
    public double TotalIntegrationMinutes { get; set; }
}

/// <summary>
/// Detailed captured image for night report
/// </summary>
public class NightReportCapturedImageDto
{
    public DateTime CapturedAt { get; set; }
    public string? TargetName { get; set; }
    public string? FileName { get; set; }
    public string Filter { get; set; } = string.Empty;
    public double ExposureTimeSeconds { get; set; }
    public int? Gain { get; set; }
    public int? Offset { get; set; }
    public int Binning { get; set; } = 1;
    public int? FocuserPosition { get; set; }
    public double? MedianAdu { get; set; }
    public double? CameraTemperature { get; set; }
    public double? Hfr { get; set; }
    public int? StarCount { get; set; }
    public double? GuidingRmsArcsec { get; set; }
    public double? Altitude { get; set; }
    public double? BackgroundMean { get; set; }
    public double? BackgroundNoise { get; set; }
    public bool IsAccepted { get; set; }
    public string? ThumbnailBase64 { get; set; }
    
    // Weather conditions at capture time
    public double? WeatherTemperature { get; set; }
    public double? WeatherHumidity { get; set; }
    public double? WeatherDewPoint { get; set; }
    public double? WeatherCloudCover { get; set; }
    public double? WeatherPressure { get; set; }
    public double? WeatherSkyQuality { get; set; }
}
