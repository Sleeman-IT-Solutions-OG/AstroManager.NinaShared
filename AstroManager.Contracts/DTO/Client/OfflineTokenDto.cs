using System.Text.Json.Serialization;

namespace Shared.Model.DTO.Client;

/// <summary>
/// Offline token for license validation when server is unreachable.
/// Contains signed data that allows the plugin to operate offline for a limited time.
/// </summary>
public class OfflineTokenDto
{
    /// <summary>
    /// Client license ID
    /// </summary>
    public Guid LicenseId { get; set; }
    
    /// <summary>
    /// User ID associated with this license
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// When this token was issued (UTC)
    /// </summary>
    public DateTime IssuedAt { get; set; }
    
    /// <summary>
    /// When this token expires (UTC) - typically 27 hours from issue
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Machine fingerprint (hash of machine identifiers) to prevent token sharing
    /// </summary>
    public string MachineFingerprint { get; set; } = string.Empty;
    
    /// <summary>
    /// HMAC-SHA256 signature of the token data, signed by server secret
    /// </summary>
    public string Signature { get; set; } = string.Empty;
    
    /// <summary>
    /// Check if token is still valid (not expired)
    /// Note: This only checks expiry, not signature validity
    /// </summary>
    [JsonIgnore]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    
    /// <summary>
    /// Time remaining until token expires
    /// </summary>
    [JsonIgnore]
    public TimeSpan TimeRemaining => IsExpired ? TimeSpan.Zero : ExpiresAt - DateTime.UtcNow;
}

/// <summary>
/// Queued capture data for offline sync
/// </summary>
public class OfflineCaptureDto
{
    /// <summary>
    /// Unique ID for this queued item
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// When this capture occurred (UTC)
    /// </summary>
    public DateTime CapturedAt { get; set; }
    
    /// <summary>
    /// Target ID being imaged
    /// </summary>
    public Guid? TargetId { get; set; }
    
    /// <summary>
    /// Imaging goal ID
    /// </summary>
    public Guid? ImagingGoalId { get; set; }
    
    /// <summary>
    /// Panel ID (for mosaics)
    /// </summary>
    public Guid? PanelId { get; set; }
    
    /// <summary>
    /// Filter used
    /// </summary>
    public string? Filter { get; set; }
    
    /// <summary>
    /// Exposure time in seconds
    /// </summary>
    public double? ExposureTimeSeconds { get; set; }
    
    /// <summary>
    /// Whether this exposure was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// File name of the captured image
    /// </summary>
    public string? FileName { get; set; }
    
    /// <summary>
    /// HFR value if available
    /// </summary>
    public double? HFR { get; set; }
    
    /// <summary>
    /// Number of detected stars
    /// </summary>
    public int? DetectedStars { get; set; }
    
    /// <summary>
    /// Camera temperature at capture
    /// </summary>
    public double? CameraTemp { get; set; }
    
    /// <summary>
    /// Gain setting
    /// </summary>
    public int? Gain { get; set; }
    
    /// <summary>
    /// Whether this item has been synced to server
    /// </summary>
    public bool IsSynced { get; set; }
    
    /// <summary>
    /// Number of sync attempts
    /// </summary>
    public int SyncAttempts { get; set; }
    
    /// <summary>
    /// Last sync error message if any
    /// </summary>
    public string? LastSyncError { get; set; }
}

/// <summary>
/// Request to sync multiple offline captures
/// </summary>
public class OfflineSyncRequestDto
{
    /// <summary>
    /// List of captures to sync
    /// </summary>
    public List<OfflineCaptureDto> Captures { get; set; } = new();
}

/// <summary>
/// Response from offline sync
/// </summary>
public class OfflineSyncResponseDto
{
    /// <summary>
    /// Whether sync was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Number of captures successfully synced
    /// </summary>
    public int SyncedCount { get; set; }
    
    /// <summary>
    /// IDs of successfully synced captures
    /// </summary>
    public List<Guid> SyncedIds { get; set; } = new();
    
    /// <summary>
    /// Error message if sync failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
