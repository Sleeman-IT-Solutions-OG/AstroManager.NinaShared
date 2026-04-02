namespace Shared.Model.Configuration;

/// <summary>
/// Configuration for telescope client connection mode
/// </summary>
public class TelescopeClientConnectionConfig
{
    /// <summary>
    /// Connection mode for this client
    /// </summary>
    public ConnectionMode Mode { get; set; } = ConnectionMode.DirectApi;
    
    /// <summary>
    /// API base URL (for DirectApi mode)
    /// </summary>
    public string ApiBaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Sync directory path (for OfflineFileSync mode)
    /// Can be local path or network share (e.g., \\server\share)
    /// </summary>
    public string? SyncDirectoryPath { get; set; }
    
    /// <summary>
    /// How often to write heartbeat file (seconds)
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;
    
    /// <summary>
    /// How often to check for updated export files (seconds)
    /// </summary>
    public int CheckForUpdatesIntervalSeconds { get; set; } = 60;
    
    /// <summary>
    /// Enable detailed file logging
    /// </summary>
    public bool EnableDetailedFileLogging { get; set; } = true;
    
    /// <summary>
    /// Auto-reload configuration when export file changes
    /// </summary>
    public bool AutoReloadOnFileChange { get; set; } = true;
}

public enum ConnectionMode
{
    /// <summary>
    /// Direct HTTPS connection to API (requires internet)
    /// </summary>
    DirectApi = 1,
    
    /// <summary>
    /// File-based sync via local or network directory (works offline)
    /// </summary>
    OfflineFileSync = 2
}
