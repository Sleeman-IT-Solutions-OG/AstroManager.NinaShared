namespace Shared.Model.DTO.Settings;

public class CameraCatalogEntryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public bool IsMono { get; set; }
    public bool IsCooled { get; set; }
    public int PixelX { get; set; }
    public int PixelY { get; set; }
    public double PixelSize { get; set; }
    public double? QuantumEfficiencyPercent { get; set; }
}
