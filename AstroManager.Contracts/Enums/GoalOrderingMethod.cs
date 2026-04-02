namespace Shared.Model.Enums;

/// <summary>
/// Defines the order in which base goals and custom panel-specific goals are shot
/// </summary>
public enum GoalOrderingMethod
{
    /// <summary>
    /// Shoot base goals (synced from parent target) before custom panel-specific goals
    /// Example: Base L/R/G/B goals first, then custom Ha/OIII goals
    /// Use case: Get broadband data first, then add narrowband details
    /// </summary>
    BaseGoalsFirst = 0,
    
    /// <summary>
    /// Shoot custom panel-specific goals before base goals
    /// Example: Custom Ha/OIII goals first, then base L/R/G/B goals
    /// Use case: Prioritize narrowband or special filters when they're available
    /// </summary>
    CustomGoalsFirst = 1,
    
    /// <summary>
    /// Interleave base and custom goals based on filter priority
    /// Goals are ordered by FilterPriority value regardless of being base or custom
    /// Use case: Most flexible, respects priority settings
    /// </summary>
    ByFilterPriority = 2
}
