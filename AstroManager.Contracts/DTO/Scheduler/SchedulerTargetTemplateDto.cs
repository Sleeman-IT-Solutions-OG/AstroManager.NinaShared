using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// DTO for SchedulerTargetTemplate - reusable scheduler settings
/// </summary>
public class SchedulerTargetTemplateDto
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? EquipmentId { get; set; }
    
    public string? FilterShootingPattern { get; set; }
    public int? FilterBatchSize { get; set; }
    public int? DitherEveryX { get; set; }
    public int? MinSessionDurationMinutes { get; set; }
    public double? MinAltitude { get; set; }
    public double? MaxHoursPerNight { get; set; }
    public int? MaxSequenceTimeMinutes { get; set; }
    public string? GoalCompletionBehaviour { get; set; }
    public int? LowerPriorityTo { get; set; }
    public bool? UseMoonAvoidance { get; set; }
    public string? MoonAvoidanceProfilesJson { get; set; }
    public TimeSpan? MinStartTime { get; set; }
    public TimeSpan? MaxStartTime { get; set; }
    public double? MinMoonPhasePercent { get; set; }
    public double? MaxMoonPhasePercent { get; set; }
    
    public bool IsSystemDefault { get; set; }
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new SchedulerTargetTemplate
/// </summary>
public class CreateSchedulerTargetTemplateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? EquipmentId { get; set; }
    
    public string? FilterShootingPattern { get; set; }
    public int? FilterBatchSize { get; set; }
    public int? DitherEveryX { get; set; }
    public int? MinSessionDurationMinutes { get; set; }
    public double? MinAltitude { get; set; }
    public double? MaxHoursPerNight { get; set; }
    public int? MaxSequenceTimeMinutes { get; set; }
    public string? GoalCompletionBehaviour { get; set; }
    public int? LowerPriorityTo { get; set; }
    public bool? UseMoonAvoidance { get; set; }
    public string? MoonAvoidanceProfilesJson { get; set; }
    public TimeSpan? MinStartTime { get; set; }
    public TimeSpan? MaxStartTime { get; set; }
    public double? MinMoonPhasePercent { get; set; }
    public double? MaxMoonPhasePercent { get; set; }
    
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for updating a SchedulerTargetTemplate
/// </summary>
public class UpdateSchedulerTargetTemplateDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? EquipmentId { get; set; }
    
    public string? FilterShootingPattern { get; set; }
    public int? FilterBatchSize { get; set; }
    public int? DitherEveryX { get; set; }
    public int? MinSessionDurationMinutes { get; set; }
    public double? MinAltitude { get; set; }
    public double? MaxHoursPerNight { get; set; }
    public int? MaxSequenceTimeMinutes { get; set; }
    public string? GoalCompletionBehaviour { get; set; }
    public int? LowerPriorityTo { get; set; }
    public bool? UseMoonAvoidance { get; set; }
    public string? MoonAvoidanceProfilesJson { get; set; }
    public TimeSpan? MinStartTime { get; set; }
    public TimeSpan? MaxStartTime { get; set; }
    public double? MinMoonPhasePercent { get; set; }
    public double? MaxMoonPhasePercent { get; set; }
    
    public int DisplayOrder { get; set; }
}
