namespace Shared.Model.Enums;

/// <summary>
/// Behavior when a target's imaging goal is completed
/// </summary>
public enum GoalCompletionBehavior
{
    /// <summary>
    /// Stop scheduling this target when imaging goal is reached
    /// </summary>
    Stop,
    
    /// <summary>
    /// Lower priority of this target and continue scheduling
    /// </summary>
    LowerPriority,
    
    /// <summary>
    /// Continue scheduling at same priority
    /// </summary>
    Continue
}
