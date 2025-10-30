using Consensus.Api.Models;

namespace Consensus.Api.Jobs.Scheduling;

/// <summary>
/// Service for scheduling and tracking Quartz jobs
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Schedule a consensus job to run after a delay
    /// </summary>
    /// <param name="runId">The run ID for this job</param>
    /// <param name="prompt">The prompt text to process</param>
    /// <param name="models">The models to use for consensus building</param>
    /// <param name="delaySeconds">Delay in seconds before the job starts (default 5)</param>
    /// <returns>True if job was scheduled successfully, false if job already exists</returns>
    Task<bool> ScheduleConsensusJobAsync(string runId, string prompt, string[] models, int delaySeconds = 5);
    
    /// <summary>
    /// Get the status of a job from Quartz
    /// </summary>
    /// <param name="runId">The run ID</param>
    /// <returns>Job status, or null if job not found</returns>
    Task<JobStatusModel?> GetJobStatusAsync(string runId);
    
    /// <summary>
    /// Check if a job exists in Quartz
    /// </summary>
    /// <param name="runId">The run ID</param>
    /// <returns>True if job exists</returns>
    Task<bool> JobExistsAsync(string runId);
}
