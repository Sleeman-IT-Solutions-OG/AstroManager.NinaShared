namespace Shared.Model.Enums;

/// <summary>
/// Types of reminders for target list items (used for UI calculation only)
/// </summary>
public enum ReminderType
{
    /// <summary>
    /// Remind when target is observable for minimum duration at observatory
    /// </summary>
    ObservableDuration = 0,
    
    /// <summary>
    /// Remind at or before opposition date
    /// </summary>
    OppositionDate = 1,
    
    /// <summary>
    /// Remind on a custom date
    /// </summary>
    CustomDate = 2
}
