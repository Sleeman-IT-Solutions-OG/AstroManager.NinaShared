namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for a queued target in manual scheduler mode
/// </summary>
public class ClientTargetQueueDto
{
    public Guid Id { get; set; }
    public Guid ClientLicenseId { get; set; }
    public Guid ScheduledTargetId { get; set; }
    public int QueueOrder { get; set; }
    public QueueItemStatus Status { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    
    // Denormalized target info for display
    public string? TargetName { get; set; }
    public double? RightAscension { get; set; }
    public double? Declination { get; set; }
    
    // Additional info for better display
    public List<string> Filters { get; set; } = new();
    public double ProgressPercent { get; set; }
    public int CompletedExposures { get; set; }
    public int TotalExposures { get; set; }
}

/// <summary>
/// DTO for adding a target to the queue
/// </summary>
public class AddToQueueDto
{
    public Guid ClientLicenseId { get; set; }
    public Guid ScheduledTargetId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for reordering queue items
/// </summary>
public class ReorderQueueDto
{
    public Guid ClientLicenseId { get; set; }
    public List<Guid> QueueItemIds { get; set; } = new();
}

/// <summary>
/// DTO for updating scheduler mode
/// </summary>
public class UpdateSchedulerModeDto
{
    public Guid ClientLicenseId { get; set; }
    public SchedulerMode Mode { get; set; }
}

/// <summary>
/// Status of a queued target
/// </summary>
public enum QueueItemStatus
{
    Pending,
    Active,
    Completed,
    Skipped,
    Failed
}
