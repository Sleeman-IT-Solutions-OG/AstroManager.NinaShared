using Shared.Model.DTO.Client;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Extension methods for CapturedImageDto
/// </summary>
public static class CapturedImageDtoExtensions
{
    /// <summary>
    /// Convert CapturedImageDto to ImageHistoryItemDto for use with shared UI components
    /// </summary>
    public static ImageHistoryItemDto ToImageHistoryItem(this CapturedImageDto dto)
    {
        return new ImageHistoryItemDto
        {
            Id = dto.Id,
            ThumbnailBase64 = dto.ThumbnailBase64,
            MicroThumbnailBase64 = dto.MicroThumbnailBase64,
            FileName = dto.FileName,
            CapturedAt = DateTime.SpecifyKind(dto.CapturedAt, DateTimeKind.Utc),
            Filter = dto.Filter.ToString(),
            ExposureTime = dto.ExposureTimeSeconds,
            HFR = dto.HfdPixels,
            DetectedStars = dto.StarCount,
            Gain = dto.Gain >= 0 ? dto.Gain : null,
            Offset = dto.Offset >= 0 ? dto.Offset : null,
            CameraTemp = dto.CameraTemperature,
            Binning = dto.Binning,
            Mean = dto.BackgroundMean,
            Median = dto.MedianAdu,
            StdDev = dto.BackgroundNoise,
            GradeScore = dto.GradeScore,
            GradeBand = dto.GradeBand,
            IsAccepted = dto.IsAccepted,
            TargetName = dto.TargetName,
            PanelNumber = dto.PanelNumber,
            PanelImagingGoalId = dto.PanelImagingGoalId,
            RightAscension = dto.RightAscension,
            Declination = dto.Declination,
            Altitude = dto.AltitudeDegrees,
            Azimuth = dto.AzimuthDegrees,
            FocuserPosition = dto.FocuserPosition,
            CameraName = dto.CameraName,
            TelescopeName = dto.TelescopeName,
            FocalLength = dto.FocalLength,
            PixelScale = dto.PixelScale,
            ImageWidth = dto.ImageWidth,
            ImageHeight = dto.ImageHeight,
            BitDepth = dto.BitDepth,
            GuidingRmsRA = dto.GuidingRmsRa,
            GuidingRmsDec = dto.GuidingRmsDec,
            GuidingRmsTotal = dto.GuidingRmsArcsec,
            WeatherTemperature = dto.WeatherTemperature,
            WeatherHumidity = dto.WeatherHumidity,
            WeatherDewPoint = dto.WeatherDewPoint,
            WeatherPressure = dto.WeatherPressure,
            WeatherCloudCover = dto.WeatherCloudCover,
            WeatherWindSpeed = dto.WeatherWindSpeed,
            WeatherSkyQuality = dto.WeatherSkyQuality
        };
    }
    
    /// <summary>
    /// Convert a list of CapturedImageDto to ImageHistoryItemDto list
    /// </summary>
    public static List<ImageHistoryItemDto> ToImageHistoryItems(this IEnumerable<CapturedImageDto> dtos)
    {
        return dtos.Select(d => d.ToImageHistoryItem()).ToList();
    }
}
