namespace Shared.Model.DTO.Settings;

/// <summary>
/// DTO for image grading criteria set with configurable score-to-band thresholds.
/// Initial implementation focuses on threshold mapping; full rule definitions follow.
/// </summary>
public class ImageGradingCriteriaSetDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public int MinScoreForA { get; set; } = 90;
    public int MinScoreForB { get; set; } = 80;
    public int MinScoreForC { get; set; } = 70;
    public int MinScoreForD { get; set; } = 60;

    /// <summary>
    /// JSON-serialized ImageAutoScoreProfile configuration
    /// </summary>
    public string? AutoScoreProfileJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
