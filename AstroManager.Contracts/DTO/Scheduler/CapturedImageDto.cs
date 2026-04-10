using Shared.Model.DTO.Common;
using Shared.Model.DTO.Settings;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Data Transfer Object for captured image with quality statistics
/// Stores image metadata and quality metrics for each captured frame
/// </summary>
public class CapturedImageDto : IImageDisplayData
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Reference to the client license (for images captured via status updates)
    /// </summary>
    public Guid? ClientLicenseId { get; set; }
    
    /// <summary>
    /// Reference to the scheduled target this image belongs to
    /// </summary>
    public Guid? ScheduledTargetId { get; set; }
    
    /// <summary>
    /// Reference to the imaging goal this image contributes to (optional, for non-panel targets)
    /// </summary>
    public Guid? ImagingGoalId { get; set; }
    
    /// <summary>
    /// Reference to the panel imaging goal this image contributes to (for mosaic panel captures)
    /// </summary>
    public Guid? PanelImagingGoalId { get; set; }
    
    /// <summary>
    /// Reference to the session this image was captured in (optional)
    /// </summary>
    public Guid? SessionId { get; set; }
    
    /// <summary>
    /// Panel number (1-based) for mosaic targets
    /// </summary>
    public int? PanelNumber { get; set; }
    
    /// <summary>
    /// Original filename of the captured image (optional - can be assigned later via ImageDashboard)
    /// </summary>
    [StringLength(255)]
    public string? FileName { get; set; }
    
    /// <summary>
    /// Reference to the AstroImage entity if file has been imported/assigned via ImageDashboard
    /// </summary>
    public Guid? AstroImageId { get; set; }
    
    /// <summary>
    /// Indicates if this captured image record has a file assigned yet
    /// </summary>
    public bool HasFileAssigned => !string.IsNullOrEmpty(FileName) || AstroImageId.HasValue;
    
    /// <summary>
    /// Full file path where the image is stored
    /// </summary>
    [StringLength(1000)]
    public string? FilePath { get; set; }
    
    /// <summary>
    /// Filter used for this capture
    /// </summary>
    public ECameraFilter Filter { get; set; }

    /// <summary>
    /// Canonical filter name for custom-filter-aware captures.
    /// Falls back to legacy Filter when not set.
    /// </summary>
    public string? FilterName { get; set; }
    
    /// <summary>
    /// Exposure time in seconds
    /// </summary>
    [Range(0.001, 3600)]
    public double ExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Camera gain setting
    /// </summary>
    public int Gain { get; set; } = -1;
    
    /// <summary>
    /// Camera offset setting
    /// </summary>
    public int Offset { get; set; } = -1;
    
    /// <summary>
    /// Camera binning (1x1, 2x2, etc.)
    /// </summary>
    [Range(1, 4)]
    public int Binning { get; set; } = 1;
    
    /// <summary>
    /// Camera temperature in Celsius at capture time
    /// </summary>
    public double? CameraTemperature { get; set; }
    
    /// <summary>
    /// Date and time when the image was captured (UTC)
    /// </summary>
    public DateTime CapturedAt { get; set; }
    
    // Quality Statistics
    
    /// <summary>
    /// Full Width at Half Maximum (FWHM) in arcseconds
    /// Lower is better - measures star sharpness
    /// </summary>
    public double? FwhmArcsec { get; set; }
    
    /// <summary>
    /// Half Flux Diameter (HFD) in pixels
    /// Similar to FWHM but measured differently
    /// </summary>
    public double? HfdPixels { get; set; }
    
    /// <summary>
    /// Number of stars detected in the image
    /// </summary>
    public int? StarCount { get; set; }
    
    /// <summary>
    /// Mean background level (ADU)
    /// </summary>
    public double? BackgroundMean { get; set; }
    
    /// <summary>
    /// Background noise (standard deviation in ADU)
    /// </summary>
    public double? BackgroundNoise { get; set; }
    
    /// <summary>
    /// Signal-to-Noise Ratio
    /// </summary>
    public double? Snr { get; set; }
    
    /// <summary>
    /// Eccentricity of stars (0 = perfect circle, 1 = line)
    /// Lower is better - measures tracking/guiding quality
    /// </summary>
    public double? Eccentricity { get; set; }
    
    /// <summary>
    /// RMS guiding error during exposure (arcseconds)
    /// </summary>
    public double? GuidingRmsArcsec { get; set; }
    
    /// <summary>
    /// Altitude of target at capture time (degrees)
    /// </summary>
    public double? AltitudeDegrees { get; set; }
    
    /// <summary>
    /// Azimuth of target at capture time (degrees)
    /// </summary>
    public double? AzimuthDegrees { get; set; }
    
    /// <summary>
    /// Airmass at capture time
    /// </summary>
    public double? Airmass { get; set; }
    
    /// <summary>
    /// Moon distance from target in degrees
    /// </summary>
    public double? MoonDistanceDegrees { get; set; }
    
    /// <summary>
    /// Moon illumination percentage at capture time
    /// </summary>
    public double? MoonIlluminationPercent { get; set; }
    
    /// <summary>
    /// Whether this image was accepted (passed quality checks)
    /// </summary>
    public bool IsAccepted { get; set; } = true;

    /// <summary>
    /// Image quality score (1-100)
    /// </summary>
    public int? GradeScore { get; set; }

    /// <summary>
    /// Grade band derived from score (A-E)
    /// </summary>
    public string? GradeBand { get; set; }

    /// <summary>
    /// True when user manually overrode auto grading
    /// </summary>
    public bool IsGradeManuallyOverridden { get; set; } = false;

    /// <summary>
    /// Source of grade assignment (Auto|Manual)
    /// </summary>
    public string? GradeSource { get; set; }

    /// <summary>
    /// Criteria set used for grading
    /// </summary>
    public Guid? GradingCriteriaSetId { get; set; }

    /// <summary>
    /// Timestamp of latest grading update
    /// </summary>
    public DateTime? GradedAt { get; set; }
    
    /// <summary>
    /// Rejection reason if not accepted
    /// </summary>
    [StringLength(500)]
    public string? RejectionReason { get; set; }
    
    /// <summary>
    /// User notes about this image
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Thumbnail image as base64 (small preview)
    /// </summary>
    public string? ThumbnailBase64 { get; set; }
    
    /// <summary>
    /// Micro thumbnail as base64 (~50px, for list views - much smaller file size)
    /// </summary>
    public string? MicroThumbnailBase64 { get; set; }
    
    /// <summary>
    /// Target name at capture time
    /// </summary>
    public string? TargetName { get; set; }
    
    /// <summary>
    /// Right Ascension at capture time (hours)
    /// </summary>
    public double? RightAscension { get; set; }
    
    /// <summary>
    /// Declination at capture time (degrees)
    /// </summary>
    public double? Declination { get; set; }
    
    /// <summary>
    /// Focuser position at capture time
    /// </summary>
    public int? FocuserPosition { get; set; }
    
    /// <summary>
    /// Camera name
    /// </summary>
    public string? CameraName { get; set; }
    
    /// <summary>
    /// Telescope name
    /// </summary>
    public string? TelescopeName { get; set; }
    
    /// <summary>
    /// Focal length in mm
    /// </summary>
    public double? FocalLength { get; set; }
    
    /// <summary>
    /// Pixel scale in arcsec/pixel
    /// </summary>
    public double? PixelScale { get; set; }
    
    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int? ImageWidth { get; set; }
    
    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int? ImageHeight { get; set; }
    
    /// <summary>
    /// Bit depth
    /// </summary>
    public int? BitDepth { get; set; }
    
    /// <summary>
    /// Median ADU value
    /// </summary>
    public double? MedianAdu { get; set; }
    
    /// <summary>
    /// Guiding RMS RA (arcsec)
    /// </summary>
    public double? GuidingRmsRa { get; set; }
    
    /// <summary>
    /// Guiding RMS Dec (arcsec)
    /// </summary>
    public double? GuidingRmsDec { get; set; }
    
    // Weather at capture time
    public double? WeatherTemperature { get; set; }
    public double? WeatherHumidity { get; set; }
    public double? WeatherDewPoint { get; set; }
    public double? WeatherPressure { get; set; }
    public double? WeatherCloudCover { get; set; }
    public double? WeatherWindSpeed { get; set; }
    public double? WeatherSkyQuality { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // IImageDisplayData interface implementations (explicit for property name mappings)
    double IImageDisplayData.ExposureTimeSeconds => ExposureTimeSeconds;
    double? IImageDisplayData.HFR => HfdPixels;
    double? IImageDisplayData.MeanADU => BackgroundMean;
    double? IImageDisplayData.NoiseADU => BackgroundNoise;
    double? IImageDisplayData.MedianADU => MedianAdu;
    double? IImageDisplayData.SNR => Snr;
    double? IImageDisplayData.Altitude => AltitudeDegrees;
    double? IImageDisplayData.Azimuth => AzimuthDegrees;
    double? IImageDisplayData.GuidingRmsRA => GuidingRmsRa;
    double? IImageDisplayData.GuidingRmsDec => GuidingRmsDec;
    double? IImageDisplayData.GuidingRmsTotal => GuidingRmsArcsec;
    string? IImageDisplayData.Filter => !string.IsNullOrWhiteSpace(FilterName) ? FilterName : Filter.ToString();
    int? IImageDisplayData.GradeScore => GradeScore;
    string? IImageDisplayData.GradeBand => GradeBand;
}

/// <summary>
/// Summary statistics for a target's captured images
/// </summary>
public class CapturedImageSummaryDto
{
    public Guid TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of captured images
    /// </summary>
    public int TotalImages { get; set; }
    
    /// <summary>
    /// Number of accepted images
    /// </summary>
    public int AcceptedImages { get; set; }
    
    /// <summary>
    /// Number of rejected images
    /// </summary>
    public int RejectedImages { get; set; }
    
    /// <summary>
    /// Total integration time in minutes
    /// </summary>
    public double TotalIntegrationMinutes { get; set; }
    
    /// <summary>
    /// Average FWHM across all accepted images
    /// </summary>
    public double? AverageFwhm { get; set; }
    
    /// <summary>
    /// Best (lowest) FWHM
    /// </summary>
    public double? BestFwhm { get; set; }
    
    /// <summary>
    /// Worst (highest) FWHM
    /// </summary>
    public double? WorstFwhm { get; set; }
    
    /// <summary>
    /// Average HFD across all accepted images
    /// </summary>
    public double? AverageHfd { get; set; }
    
    /// <summary>
    /// Average star count
    /// </summary>
    public double? AverageStarCount { get; set; }
    
    /// <summary>
    /// Average SNR
    /// </summary>
    public double? AverageSnr { get; set; }
    
    /// <summary>
    /// Average eccentricity
    /// </summary>
    public double? AverageEccentricity { get; set; }

    /// <summary>
    /// Average guiding RMS total (arcseconds)
    /// </summary>
    public double? AverageGuidingRmsArcsec { get; set; }
    
    /// <summary>
    /// Images grouped by filter
    /// </summary>
    public List<FilterImageStats> FilterStats { get; set; } = new();
    
    /// <summary>
    /// Date of first capture
    /// </summary>
    public DateTime? FirstCaptureDate { get; set; }
    
    /// <summary>
    /// Date of last capture
    /// </summary>
    public DateTime? LastCaptureDate { get; set; }
}

/// <summary>
/// Statistics per filter
/// </summary>
public class FilterImageStats
{
    public ECameraFilter Filter { get; set; }
    public string? FilterName { get; set; }
    public int ImageCount { get; set; }
    public int AcceptedCount { get; set; }
    public double IntegrationMinutes { get; set; }
    public double? AverageFwhm { get; set; }
    public double? AverageHfd { get; set; }
    public double? AverageSnr { get; set; }
}

public class RegradeCapturedImagesRequestDto
{
    public Guid? TargetId { get; set; }
    public Guid? ClientLicenseId { get; set; }
    public bool OnlyActiveTargets { get; set; } = false;
    public bool ExcludeArchivedTargets { get; set; } = false;
    public Guid? CriteriaSetId { get; set; }
    public bool IncludeManualOverrides { get; set; } = false;
    public bool RecalculateGoalProgress { get; set; } = true;
    public int? MaxImages { get; set; } = 5000;
}

public class RegradeCapturedImagesResultDto
{
    public int Processed { get; set; }
    public int Updated { get; set; }
    public int SkippedManual { get; set; }
    public bool GoalProgressRecalculated { get; set; }
    public int AutoCompletedTargets { get; set; }
    public int ReactivatedTargets { get; set; }
}
