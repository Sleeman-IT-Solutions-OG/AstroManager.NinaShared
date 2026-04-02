using Shared.Model.Configuration;
using Shared.Model.DTO.Settings;

namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for client configuration
/// </summary>
public class ClientConfigurationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ClientLicenseId { get; set; }
    public Guid ObservatoryId { get; set; }
    public string? ObservatoryName { get; set; }
    public Guid EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public Guid? DefaultSchedulerConfigurationId { get; set; }
    public string? DefaultSchedulerConfigurationName { get; set; }
    public Guid? RuntimeStopSafetyPolicyId { get; set; }
    public string? RuntimeStopSafetyPolicyName { get; set; }
    public RuntimeStopSafetyPolicyDto? RuntimeStopSafetyPolicy { get; set; }
    public string ImagingSoftware { get; set; } = "NINA";
    public string SequenceFilePath { get; set; } = @"C:\AstroSequences\current-sequence.json";
    public string SequenceTemplatePath { get; set; } = @"C:\AstroSequences\template-sequence.json";
    public string VoyagerFilterMapping { get; set; } = "{\"L\":0, \"R\":1, \"G\":2, \"B\":3, \"Ha\":4, \"Oiii\":5, \"Sii\":6}";
    public string LocalStoragePath { get; set; } = @"C:\AstroImages";
    public double? TargetTemperature { get; set; }
    public int? Gain { get; set; }
    public int? Offset { get; set; }
    public string Binning { get; set; } = "1x1";
    public int AutoFocusIntervalMinutes { get; set; }
    public bool UseDithering { get; set; }
    public int DitheringPixels { get; set; }
    public int DitheringInterval { get; set; }
    public int HeartbeatIntervalSeconds { get; set; } = 300;
    public bool AutoStartSessions { get; set; }
    public bool ParkOnComplete { get; set; }
    public bool WarmCameraOnComplete { get; set; }
    public string CallbackExecutablePath { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = "http://localhost:5000";
    public MainAppSyncConfig? SyncConfig { get; set; }
    
    // === Meridian Flip Settings (from NINA profile) ===
    public bool MeridianFlipEnabled { get; set; } = false;
    public double MeridianFlipMinutesAfterMeridian { get; set; } = 5;
    public double MeridianFlipPauseMinutes { get; set; } = 5;
    public double MeridianFlipMaxMinutesToMeridian { get; set; } = 5;
    
    public int ConfigVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating/updating client configuration
/// </summary>
public class SaveClientConfigurationDto
{
    public Guid ClientLicenseId { get; set; }
    public Guid ObservatoryId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid? DefaultSchedulerConfigurationId { get; set; }
    public Guid? RuntimeStopSafetyPolicyId { get; set; }
    public string ImagingSoftware { get; set; } = "NINA";
    public string SequenceFilePath { get; set; } = @"C:\AstroSequences\current-sequence.json";
    public string SequenceTemplatePath { get; set; } = @"C:\AstroSequences\template-sequence.json";
    public string VoyagerFilterMapping { get; set; } = "{\"L\":0, \"R\":1, \"G\":2, \"B\":3, \"Ha\":4, \"Oiii\":5, \"Sii\":6}";
    public string LocalStoragePath { get; set; } = @"C:\AstroImages";
    public double? TargetTemperature { get; set; } = -10.0;
    public int? Gain { get; set; } = 100;
    public int? Offset { get; set; } = 10;
    public string Binning { get; set; } = "1x1";
    public int AutoFocusIntervalMinutes { get; set; } = 30;
    public bool UseDithering { get; set; } = true;
    public int DitheringPixels { get; set; } = 5;
    public int DitheringInterval { get; set; } = 1;
    public int HeartbeatIntervalSeconds { get; set; } = 300;
    public bool AutoStartSessions { get; set; } = true;
    public bool ParkOnComplete { get; set; } = true;
    public bool WarmCameraOnComplete { get; set; } = true;
    public string CallbackExecutablePath { get; set; } = @"C:\Program Files\AstroManager\callback.exe";
    public string CallbackUrl { get; set; } = "http://localhost:5000";
    public MainAppSyncConfig? SyncConfig { get; set; }
    
    // === Meridian Flip Settings (from NINA profile) ===
    public bool MeridianFlipEnabled { get; set; } = false;
    public double MeridianFlipMinutesAfterMeridian { get; set; } = 5;
    public double MeridianFlipPauseMinutes { get; set; } = 5;
    public double MeridianFlipMaxMinutesToMeridian { get; set; } = 5;
}
