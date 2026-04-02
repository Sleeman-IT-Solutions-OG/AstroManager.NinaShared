namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for validating a client license
/// </summary>
public class ValidateClientLicenseDto
{
    public string LicenseKey { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public string ClientVersion { get; set; } = string.Empty;
}

/// <summary>
/// DTO for client authentication response
/// </summary>
public class ClientAuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public Guid UserId { get; set; }
    public Guid ClientLicenseId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>
    /// Observatory this license is linked to (for scoping exposure templates)
    /// </summary>
    public Guid? ObservatoryId { get; set; }
    public string? ObservatoryName { get; set; }
    
    /// <summary>
    /// Equipment profile this license is linked to (for scoping exposure templates)
    /// </summary>
    public Guid? EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    
    /// <summary>
    /// Offline token for operating without server connection (valid for ~27 hours)
    /// </summary>
    public OfflineTokenDto? OfflineToken { get; set; }
}

/// <summary>
/// DTO for refreshing client token
/// </summary>
public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
