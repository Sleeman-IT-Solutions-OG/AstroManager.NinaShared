namespace Shared.Model.DTO.Client;

/// <summary>
/// Log level for sequencer entries
/// </summary>
public enum SequencerLogLevel
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Success = 3
}

/// <summary>
/// DTO for a single sequencer log entry
/// </summary>
public class SequencerLogEntryDto
{
    public Guid Id { get; set; }
    public Guid ClientLicenseId { get; set; }
    public Guid? ImagingSessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public SequencerLogLevel Level { get; set; }
    public string? TargetName { get; set; }
    public string? Filter { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// DTO for creating a log entry from NINA
/// </summary>
public class CreateSequencerLogDto
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public SequencerLogLevel Level { get; set; }
    public string? TargetName { get; set; }
    public string? Filter { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// DTO for batch sending logs
/// </summary>
public class BatchSequencerLogsDto
{
    public List<CreateSequencerLogDto> Entries { get; set; } = new();
}

/// <summary>
/// Query parameters for fetching logs
/// </summary>
public class SequencerLogQueryDto
{
    public Guid ClientLicenseId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public DateTime? SinceTimestamp { get; set; }
    public SequencerLogLevel? MinLevel { get; set; }
    public string? Category { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 100;
}
