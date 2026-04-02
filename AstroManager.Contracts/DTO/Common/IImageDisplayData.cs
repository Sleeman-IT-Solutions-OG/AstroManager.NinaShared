namespace Shared.Model.DTO.Common;

/// <summary>
/// Common interface for image display data - used by both ImageHistoryItemDto and CapturedImageDto
/// Enables shared UI components to display image details without duplication
/// </summary>
public interface IImageDisplayData
{
    // Thumbnails
    string? ThumbnailBase64 { get; }
    string? MicroThumbnailBase64 { get; }
    
    // Basic info
    string? FileName { get; }
    DateTime CapturedAt { get; }
    string? Filter { get; }
    double ExposureTimeSeconds { get; }
    
    // Camera settings
    int Gain { get; }
    int Offset { get; }
    int Binning { get; }
    double? CameraTemperature { get; }
    
    // Quality metrics
    double? HFR { get; }
    int? StarCount { get; }
    double? MedianADU { get; }
    double? MeanADU { get; }
    double? NoiseADU { get; }
    double? SNR { get; }
    int? GradeScore { get; }
    string? GradeBand { get; }
    
    // Position
    double? RightAscension { get; }
    double? Declination { get; }
    double? Altitude { get; }
    double? Azimuth { get; }
    
    // Equipment
    string? CameraName { get; }
    string? TelescopeName { get; }
    double? FocalLength { get; }
    double? PixelScale { get; }
    int? FocuserPosition { get; }
    
    // Image dimensions
    int? ImageWidth { get; }
    int? ImageHeight { get; }
    int? BitDepth { get; }
    
    // Guiding
    double? GuidingRmsRA { get; }
    double? GuidingRmsDec { get; }
    double? GuidingRmsTotal { get; }
    
    // Weather
    double? WeatherTemperature { get; }
    double? WeatherHumidity { get; }
    double? WeatherDewPoint { get; }
    double? WeatherCloudCover { get; }
    double? WeatherWindSpeed { get; }
    double? WeatherSkyQuality { get; }
    
    // Target
    string? TargetName { get; }
}
