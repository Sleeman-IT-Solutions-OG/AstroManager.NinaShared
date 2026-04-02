namespace Shared.Model.ViewModel;

/// <summary>
/// Distance unit options for UI components
/// </summary>
public enum DistanceUnit
{
    LightYears,
    KiloLightYears,
    MegaLightYears
}

/// <summary>
/// Time unit options for quick time buttons
/// </summary>
public enum TimeUnit
{
    Minute,
    Hour,
    Day,
    Month
}

/// <summary>
/// Size unit options for UI components
/// </summary>
public enum SizeUnit
{
    ArcSeconds,
    ArcMinutes,
    Degrees
}

/// <summary>
/// Distance unit option for UI dropdowns
/// </summary>
public class DistanceUnitOption
{
    public DistanceUnit Value { get; set; }
    public string Text { get; set; } = "";
}

/// <summary>
/// Size unit option for UI dropdowns
/// </summary>
public class SizeUnitOption
{
    public SizeUnit Value { get; set; }
    public string Text { get; set; } = "";
}
