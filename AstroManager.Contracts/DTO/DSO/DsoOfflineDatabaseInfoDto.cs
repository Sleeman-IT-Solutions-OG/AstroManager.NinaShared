namespace Shared.Model.DTO.DSO;

public class DsoOfflineDatabaseInfoDto
{
    public int TotalObjects { get; set; }
    public int ObjectsWithImages { get; set; }
    public long EstimatedSizeWithoutImages { get; set; }
    public long EstimatedSizeWithImages { get; set; }
    public DateTime LastUpdated { get; set; }
    public string DatabaseVersion { get; set; } = string.Empty;
    public List<string> AvailableCatalogs { get; set; } = new();
}
