using Shared.Model.Enums;

namespace Shared.Model.DTO.Client;

/// <summary>
/// Request DTO for reporting an error during imaging
/// </summary>
public class ErrorReportDto
{
    /// <summary>
    /// Session ID for tracking
    /// </summary>
    public Guid? SessionId { get; set; }
    
    /// <summary>
    /// Target ID where error occurred
    /// </summary>
    public Guid? TargetId { get; set; }
    
    /// <summary>
    /// Panel ID if mosaic (null for non-mosaic)
    /// </summary>
    public Guid? PanelId { get; set; }
    
    /// <summary>
    /// Type of error
    /// </summary>
    public ErrorType ErrorType { get; set; } = ErrorType.Unknown;
    
    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional error details (stack trace, etc.)
    /// </summary>
    public string? ErrorDetails { get; set; }
    
    /// <summary>
    /// How many times this operation has been retried
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// UTC timestamp when error occurred
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response DTO for error report - tells client what to do next
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Instruction for client: Retry, Wait, SkipTarget, or Stop
    /// </summary>
    public ErrorInstruction Instruction { get; set; }
    
    /// <summary>
    /// Minutes to wait (when Instruction is Wait)
    /// </summary>
    public int WaitMinutes { get; set; }
    
    /// <summary>
    /// Human-readable message
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Whether the target was marked inactive
    /// </summary>
    public bool TargetMarkedInactive { get; set; }
    
    /// <summary>
    /// If target was temporarily skipped, when it will be reactivated (UTC)
    /// </summary>
    public DateTime? ReactivateAtUtc { get; set; }
}
