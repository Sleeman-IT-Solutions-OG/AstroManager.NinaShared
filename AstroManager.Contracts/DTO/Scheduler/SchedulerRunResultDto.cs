namespace Shared.Model.DTO.Scheduler;

/// <summary>
/// Result DTO for scheduler run
/// </summary>
public class SchedulerRunResultDto
{
    /// <summary>
    /// Whether the scheduler run was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Configuration used for this run
    /// </summary>
    public SchedulerConfigurationDto? Configuration { get; set; }
    
    /// <summary>
    /// Observatory used for this run
    /// </summary>
    public Guid ObservatoryId { get; set; }
    
    /// <summary>
    /// Equipment used for this run
    /// </summary>
    public Guid EquipmentId { get; set; }
    
    /// <summary>
    /// Generated scheduled sessions grouped by date
    /// </summary>
    public Dictionary<DateTime, List<ScheduledSessionDto>> SessionsByDate { get; set; } = new Dictionary<DateTime, List<ScheduledSessionDto>>();
    
    /// <summary>
    /// All generated sessions (flat list)
    /// </summary>
    public List<ScheduledSessionDto> AllSessions { get; set; } = new List<ScheduledSessionDto>();
    
    /// <summary>
    /// Targets that were scheduled
    /// </summary>
    public List<ScheduledTargetDto> ScheduledTargets { get; set; } = new List<ScheduledTargetDto>();
    
    /// <summary>
    /// Targets that could not be scheduled (with reasons)
    /// </summary>
    public Dictionary<Guid, string> UnscheduledTargets { get; set; } = new Dictionary<Guid, string>();
    
    /// <summary>
    /// Total number of sessions generated
    /// </summary>
    public int TotalSessions => AllSessions.Count;
    
    /// <summary>
    /// Total imaging time scheduled in minutes
    /// </summary>
    public double TotalScheduledTimeMinutes => AllSessions.Sum(s => s.PlannedDurationMinutes);
    
    /// <summary>
    /// Total imaging time scheduled in hours
    /// </summary>
    public double TotalScheduledTimeHours => Math.Round(TotalScheduledTimeMinutes / 60.0, 2);
    
    /// <summary>
    /// Number of unique dates with sessions
    /// </summary>
    public int NumberOfNights => SessionsByDate.Count;
    
    /// <summary>
    /// Statistics per target
    /// </summary>
    public Dictionary<Guid, TargetSchedulingStats> TargetStats { get; set; } = new Dictionary<Guid, TargetSchedulingStats>();
    
    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Statistics for a single target's scheduling
/// </summary>
public class TargetSchedulingStats
{
    public Guid TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public double TotalScheduledMinutes { get; set; }
    public double TotalScheduledHours => Math.Round(TotalScheduledMinutes / 60.0, 2);
    public Dictionary<string, double> TimePerFilter { get; set; } = new Dictionary<string, double>();
    public int NumberOfNights { get; set; }
}
