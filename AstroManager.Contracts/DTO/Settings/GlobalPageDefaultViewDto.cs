namespace Shared.Model.DTO.Settings;

/// <summary>
/// Global default card layout snapshot for a page/device combination.
/// Used as fallback when a user has no personal saved layout/default view.
/// </summary>
public class GlobalPageDefaultViewDto
{
    public string PageId { get; set; } = string.Empty;
    public string DeviceType { get; set; } = "Desktop";
    public string? ViewId { get; set; }
    public string LayoutJson { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
