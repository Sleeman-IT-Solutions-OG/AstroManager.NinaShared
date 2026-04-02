namespace Shared.Model.DTO.Client;

/// <summary>
/// Request DTO for reporting a completed exposure
/// </summary>
public class ExposureCompleteDto
{
    /// <summary>
    /// Session ID for tracking
    /// </summary>
    public Guid? SessionId { get; set; }
    
    /// <summary>
    /// Target ID
    /// </summary>
    public Guid TargetId { get; set; }
    
    /// <summary>
    /// Panel ID if mosaic (null for non-mosaic)
    /// </summary>
    public Guid? PanelId { get; set; }
    
    /// <summary>
    /// Imaging goal ID
    /// </summary>
    public Guid ImagingGoalId { get; set; }
    
    /// <summary>
    /// Filter used
    /// </summary>
    public string Filter { get; set; } = string.Empty;
    
    /// <summary>
    /// Exposure time in seconds
    /// </summary>
    public int ExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Local file path where image was saved
    /// </summary>
    public string? ImagePath { get; set; }
    
    /// <summary>
    /// Image quality and metadata from NINA
    /// </summary>
    public ImageMetadataDto? ImageMetadata { get; set; }
    
    /// <summary>
    /// Whether exposure was successful
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Error message if exposure failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Image metadata from NINA after exposure
/// </summary>
public class ImageMetadataDto
{
    /// <summary>
    /// Half Flux Radius (star quality metric)
    /// </summary>
    public double? HFR { get; set; }
    
    /// <summary>
    /// Full Width Half Maximum (seeing metric)
    /// </summary>
    public double? FWHM { get; set; }
    
    /// <summary>
    /// Number of detected stars
    /// </summary>
    public int? StarCount { get; set; }
    
    /// <summary>
    /// Mean ADU value
    /// </summary>
    public double? MeanADU { get; set; }
    
    /// <summary>
    /// Median ADU value
    /// </summary>
    public double? MedianADU { get; set; }
    
    /// <summary>
    /// Noise level in ADU
    /// </summary>
    public double? NoiseADU { get; set; }
    
    /// <summary>
    /// Signal to Noise Ratio
    /// </summary>
    public double? SNR { get; set; }
    
    /// <summary>
    /// Altitude at capture (degrees)
    /// </summary>
    public double? Altitude { get; set; }
    
    /// <summary>
    /// Azimuth at capture (degrees)
    /// </summary>
    public double? Azimuth { get; set; }
    
    /// <summary>
    /// Airmass at capture
    /// </summary>
    public double? Airmass { get; set; }
    
    /// <summary>
    /// Guiding RMS error in RA (arcsec)
    /// </summary>
    public double? GuidingRmsRA { get; set; }
    
    /// <summary>
    /// Guiding RMS error in Dec (arcsec)
    /// </summary>
    public double? GuidingRmsDec { get; set; }
    
    /// <summary>
    /// Ambient temperature (Celsius)
    /// </summary>
    public double? Temperature { get; set; }
    
    /// <summary>
    /// Camera sensor temperature (Celsius)
    /// </summary>
    public double? CameraTemp { get; set; }
    
    /// <summary>
    /// Focuser position
    /// </summary>
    public int? FocuserPosition { get; set; }
    
    /// <summary>
    /// Rotator angle (degrees)
    /// </summary>
    public double? RotatorAngle { get; set; }
    
    /// <summary>
    /// Plate solved RA (hours)
    /// </summary>
    public double? PlateSolveRA { get; set; }
    
    /// <summary>
    /// Plate solved Dec (degrees)
    /// </summary>
    public double? PlateSolveDec { get; set; }
    
    /// <summary>
    /// Plate solve rotation angle (degrees)
    /// </summary>
    public double? PlateSolveRotation { get; set; }
    
    /// <summary>
    /// Pixel scale (arcsec/pixel)
    /// </summary>
    public double? PixelScale { get; set; }
}

/// <summary>
/// Response DTO for exposure complete
/// </summary>
public class ExposureCompleteResponseDto
{
    /// <summary>
    /// Whether the report was acknowledged
    /// </summary>
    public bool Acknowledged { get; set; }
    
    /// <summary>
    /// New completed count for this goal
    /// </summary>
    public int NewCompletedCount { get; set; }
    
    /// <summary>
    /// Total goal count (including RepeatCount)
    /// </summary>
    public int TotalGoalCount { get; set; }
    
    /// <summary>
    /// Completion percentage for this goal
    /// </summary>
    public double CompletionPercentage { get; set; }
    
    /// <summary>
    /// Optional message
    /// </summary>
    public string? Message { get; set; }
}
