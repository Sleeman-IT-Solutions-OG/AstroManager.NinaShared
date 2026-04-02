namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for updating meridian flip settings from NINA client
/// </summary>
public class MeridianFlipSettingsUpdateDto
{
    public bool Enabled { get; set; }
    public double MinutesAfterMeridian { get; set; }
    public double PauseTimeBeforeFlipMinutes { get; set; }
    public double MaxMinutesToMeridian { get; set; }
}
