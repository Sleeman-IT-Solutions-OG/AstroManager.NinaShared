namespace Shared.Model.Configuration;

/// <summary>
/// Configuration for main app file sync monitoring
/// </summary>
public class MainAppSyncConfig
{
    /// <summary>
    /// Default sync directory for exports/imports
    /// </summary>
    public string DefaultSyncDirectory { get; set; } = @"C:\ProgramData\AstroManager\Sync";
    
    /// <summary>
    /// Automatically start monitoring on app startup
    /// </summary>
    public bool EnableAutoMonitoring { get; set; } = true;
    
    /// <summary>
    /// How often to scan for status files (seconds)
    /// </summary>
    public int MonitorIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// Move processed files to archive folder
    /// </summary>
    public bool ArchiveProcessedFiles { get; set; } = true;
    
    /// <summary>
    /// Delete archived files older than this many days (0 = never delete)
    /// </summary>
    public int ArchiveRetentionDays { get; set; } = 90;
    
    /// <summary>
    /// Sync status updates to API immediately when detected
    /// </summary>
    public bool ImmediateSyncToApi { get; set; } = true;
    
    /// <summary>
    /// Show desktop notifications for status updates
    /// </summary>
    public bool ShowDesktopNotifications { get; set; } = true;
}
