using Shared.Model.DTO.Settings;
using Shared.Model.Enums;

namespace Shared.Model.DTO.Client;

/// <summary>
/// DTO for session execution log
/// </summary>
public class SessionExecutionLogDto
{
    public Guid Id { get; set; }
    public Guid ScheduledSessionId { get; set; }
    public Guid ClientLicenseId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ESessionExecutionStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public int ExposuresTaken { get; set; }
    public int ExposuresPlanned { get; set; }
    public double ActualDurationMinutes { get; set; }
    public double? AverageTemperature { get; set; }
    public double? AverageGuideRms { get; set; }
    public int? FailedExposures { get; set; }
    public bool IsRunning { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// DTO for starting a session
/// </summary>
public class StartSessionDto
{
    public Guid ClientLicenseId { get; set; }
    public double EstimatedDurationMinutes { get; set; }
    public int ExposuresPlanned { get; set; }
}

/// <summary>
/// DTO for updating session progress
/// </summary>
public class SessionProgressDto
{
    public int ExposuresTaken { get; set; }
    public ECameraFilter CurrentFilter { get; set; }
    public double? Temperature { get; set; }
    public double? GuideRms { get; set; }
    public ESessionExecutionStatus Status { get; set; }
}

/// <summary>
/// DTO for completing a session
/// </summary>
public class CompleteSessionDto
{
    public double ActualDurationMinutes { get; set; }
    public int ExposuresTaken { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public double? AverageTemperature { get; set; }
    public double? AverageGuideRms { get; set; }
}

/// <summary>
/// DTO for aborting a session
/// </summary>
public class AbortSessionDto
{
    public string Reason { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Session execution status enum
/// </summary>
public enum ESessionExecutionStatus
{
    Queued = 0,
    Starting = 1,
    Slewing = 2,
    Focusing = 3,
    Exposing = 4,
    Completed = 5,
    Failed = 6,
    Aborted = 7,
    WeatherAbort = 8
}
