using Shared.Model.Enums;

namespace Shared.Model.Common;

/// <summary>
/// Option for displaying target selection strategies in dropdowns
/// </summary>
public class StrategyOption
{
    public TargetSelectionStrategy? Value { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Option for displaying goal completion behaviors in dropdowns
/// </summary>
public class BehaviorOption
{
    public GoalCompletionBehavior Value { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Option for displaying filter shooting patterns in dropdowns
/// </summary>
public class FilterPatternOption
{
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
