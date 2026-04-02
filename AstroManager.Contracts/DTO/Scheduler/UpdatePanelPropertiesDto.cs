namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// DTO for updating panel properties (ShootingOrder, IsEnabled)
/// </summary>
public class UpdatePanelPropertiesDto
{
    public Guid PanelId { get; set; }
    public int? ShootingOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}
