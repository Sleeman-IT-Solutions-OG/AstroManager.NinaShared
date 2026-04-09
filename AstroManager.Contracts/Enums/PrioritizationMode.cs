namespace Shared.Model.Enums;

/// <summary>
/// Controls how the scheduler ranks eligible targets.
/// </summary>
public enum PrioritizationMode
{
    /// <summary>
    /// Legacy primary/secondary/tertiary strategy ordering.
    /// </summary>
    Simple,

    /// <summary>
    /// Weighted multi-criterion scoring across enabled metrics.
    /// </summary>
    Weighted
}
