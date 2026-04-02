namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for client license information
/// </summary>
public class ClientLicenseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? MachineId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActivated { get; set; }
    public bool IsValid { get; set; }
    
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
    /// Imaging software configured for this client
    /// </summary>
    public string? ImagingSoftware { get; set; }

    /// <summary>
    /// Enables automatic image grading for captures from this client
    /// </summary>
    public bool AutoImageGradingEnabled { get; set; }

    /// <summary>
    /// Assigned image grading criteria set ID
    /// </summary>
    public Guid? ImageGradingCriteriaSetId { get; set; }
}

/// <summary>
/// DTO for creating a new client license
/// </summary>
public class CreateClientLicenseDto
{
    public string ClientName { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public Guid? ObservatoryId { get; set; }
    public Guid? EquipmentId { get; set; }
    public string? ImagingSoftware { get; set; }
    public bool AutoImageGradingEnabled { get; set; } = false;
    public Guid? ImageGradingCriteriaSetId { get; set; }
}

/// <summary>
/// DTO for activating a client license
/// </summary>
public class ActivateClientLicenseDto
{
    public string MachineId { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a client license (Admin only)
/// </summary>
public class UpdateClientLicenseDto
{
    public string? ClientName { get; set; }
    public string? MachineId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? AutoImageGradingEnabled { get; set; }
    public Guid? ImageGradingCriteriaSetId { get; set; }
}
