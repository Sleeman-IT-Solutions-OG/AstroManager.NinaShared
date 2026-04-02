namespace Shared.Model.Enums;

/// <summary>
/// Defines acceptable twilight types for imaging
/// </summary>
public enum ETwilightType
{
    /// <summary>
    /// Astronomical twilight - Sun is 12-18° below horizon (darker, default for most imaging)
    /// </summary>
    Astronomical = 0,
    
    /// <summary>
    /// Nautical twilight - Sun is 6-12° below horizon (allows some twilight imaging)
    /// </summary>
    Nautical = 1,
    
    /// <summary>
    /// Civil twilight - Sun is 0-6° below horizon (brightest, rarely used for DSO)
    /// </summary>
    Civil = 2
}
