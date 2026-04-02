using Shared.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Settings;

/// <summary>
/// Runtime stop/safety policy assignable to telescope clients.
/// Evaluated between slots by the NINA scheduler.
/// </summary>
public class RuntimeStopSafetyPolicyDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public bool IsDefault { get; set; }

    public bool AlwaysStopWhenNoTargetsForNight { get; set; } = true;

    /// <summary>
    /// Dynamic runtime safety rules (authoritative source of checks and actions).
    /// </summary>
    public List<RuntimeSafetyRuleDto> Rules { get; set; } = new();

    /// <summary>
    /// Runtime-only action configuration when a single rule action is being executed.
    /// Not persisted directly.
    /// </summary>
    public RuntimeSafetyActionConfigDto? ActiveActionConfig { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
