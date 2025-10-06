namespace Consensus.Api.Models;

/// <summary>
/// Represents the status of a background job
/// </summary>
public class JobStatusModel
{
    /// <summary>
    /// The run ID of the job
    /// </summary>
    public required string RunId { get; set; }
    
    /// <summary>
    /// The current status of the job
    /// </summary>
    public JobStatus Status { get; set; }
    
    /// <summary>
    /// When the job was created/queued
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the job started execution (if started)
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When the job finished execution (if finished)
    /// </summary>
    public DateTime? FinishedAt { get; set; }
}

/// <summary>
/// Possible job statuses
/// </summary>
public enum JobStatus
{
    NotStarted,
    Running,
    Finished,
    Timeout
}
