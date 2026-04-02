namespace Shared.Model.DTO.Client;

/// <summary>
/// Represents the execution status of a sequence item
/// </summary>
public enum SequenceItemStatus
{
    /// <summary>Not started yet</summary>
    Pending,
    /// <summary>Currently executing</summary>
    Running,
    /// <summary>Completed successfully</summary>
    Completed,
    /// <summary>Skipped</summary>
    Skipped,
    /// <summary>Failed with error</summary>
    Failed,
    /// <summary>Disabled by user</summary>
    Disabled
}

/// <summary>
/// Represents a node in the NINA sequence tree
/// </summary>
public class SequenceTreeNodeDto
{
    /// <summary>Name of the sequence item/container</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Type of item (e.g., "Sequential", "Parallel", "Take Exposure", etc.)</summary>
    public string ItemType { get; set; } = string.Empty;
    
    /// <summary>Current execution status</summary>
    public SequenceItemStatus Status { get; set; } = SequenceItemStatus.Pending;
    
    /// <summary>Progress info if applicable (e.g., "3/10" for loop iterations)</summary>
    public string? Progress { get; set; }
    
    /// <summary>Additional description or details</summary>
    public string? Description { get; set; }
    
    /// <summary>Whether this is a container (has children)</summary>
    public bool IsContainer { get; set; }
    
    /// <summary>Child items if this is a container</summary>
    public List<SequenceTreeNodeDto> Children { get; set; } = new();
    
    /// <summary>Nesting level (0 = root)</summary>
    public int Level { get; set; }
}

/// <summary>
/// Complete sequence tree structure for display
/// </summary>
public class SequenceTreeDto
{
    /// <summary>Name of the sequence file</summary>
    public string SequenceName { get; set; } = string.Empty;
    
    /// <summary>Whether the sequence is currently running</summary>
    public bool IsRunning { get; set; }
    
    /// <summary>Root nodes of the sequence tree</summary>
    public List<SequenceTreeNodeDto> RootNodes { get; set; } = new();
    
    /// <summary>Timestamp when this tree was captured</summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Total count of all items</summary>
    public int TotalItems { get; set; }
    
    /// <summary>Count of completed items</summary>
    public int CompletedItems { get; set; }
    
    /// <summary>Count of running items</summary>
    public int RunningItems { get; set; }
}

/// <summary>
/// Single available sequence file entry from NINA sequence folder.
/// </summary>
public class SequenceFileEntryDto
{
    public string FullPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Sequence file list snapshot captured from NINA.
/// </summary>
public class SequenceFileListDto
{
    public string SequenceFolder { get; set; } = string.Empty;
    public List<SequenceFileEntryDto> Files { get; set; } = new();
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}
