namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// DTO for bulk updating ALL panel data in a single API call
/// Combines properties, custom goals, and base goal completion updates
/// </summary>
public class BulkPanelUpdateDto
{
    /// <summary>
    /// Panel properties (ShootingOrder, IsEnabled) keyed by panel ID
    /// </summary>
    public Dictionary<Guid, UpdatePanelPropertiesDto> PanelProperties { get; set; } = new();
    
    /// <summary>
    /// Custom panel-specific goals keyed by panel ID
    /// </summary>
    public Dictionary<Guid, List<PanelImagingGoalDto>> CustomGoals { get; set; } = new();
    
    /// <summary>
    /// Base goal completion times keyed by panel ID
    /// </summary>
    public Dictionary<Guid, List<PanelImagingGoalDto>> BaseGoalCompletion { get; set; } = new();
}
