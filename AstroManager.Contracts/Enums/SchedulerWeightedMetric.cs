namespace Shared.Model.Enums;

/// <summary>
/// Neutral metrics used by weighted scheduler prioritization.
/// </summary>
public enum SchedulerWeightedMetric
{
    Priority,
    Altitude,
    ObservableMinutes,
    RemainingTimeMinutes,
    MoonDistance,
    CompletionPercentage
}
