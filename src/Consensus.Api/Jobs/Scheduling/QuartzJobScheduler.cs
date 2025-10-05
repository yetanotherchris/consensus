using Consensus.Api.Models;
using Quartz;

namespace Consensus.Api.Jobs.Scheduling;

/// <summary>
/// Quartz-based implementation of job scheduler
/// </summary>
public class QuartzJobScheduler : IJobScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzJobScheduler> _logger;

    public QuartzJobScheduler(
        ISchedulerFactory schedulerFactory,
        ILogger<QuartzJobScheduler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task<bool> ScheduleConsensusJobAsync(string runId, int delaySeconds = 5)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"consensus-{runId}", "consensus-jobs");
        
        // Check if job already exists
        if (await scheduler.CheckExists(jobKey))
        {
            _logger.LogWarning("Job with runId {RunId} already exists", runId);
            return false;
        }
        
        var createdAt = DateTime.UtcNow;
        var jobDataMap = new JobDataMap
        {
            { "runId", runId },
            { "createdAt", createdAt }
        };
        
        var job = JobBuilder.Create<ConsensusJob>()
            .WithIdentity(jobKey)
            .UsingJobData(jobDataMap)
            .StoreDurably() // Keep job after execution for status tracking
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger-{runId}", "consensus-triggers")
            .ForJob(jobKey)
            .StartAt(DateTimeOffset.UtcNow.AddSeconds(delaySeconds))
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        _logger.LogInformation("Job scheduled for runId: {RunId}, will start in {DelaySeconds} seconds", runId, delaySeconds);
        
        return true;
    }

    public async Task<JobStatusModel?> GetJobStatusAsync(string runId)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"consensus-{runId}", "consensus-jobs");
        
        // Check if job exists
        if (!await scheduler.CheckExists(jobKey))
        {
            return null;
        }

        var jobDetail = await scheduler.GetJobDetail(jobKey);
        if (jobDetail == null)
        {
            return null;
        }

        // Get triggers for this job
        var triggers = await scheduler.GetTriggersOfJob(jobKey);
        var trigger = triggers.FirstOrDefault();

        // Determine job status
        JobStatus status;
        DateTime? startedAt = null;
        DateTime? finishedAt = null;
        
        if (trigger != null)
        {
            var triggerState = await scheduler.GetTriggerState(trigger.Key);
            
            status = triggerState switch
            {
                TriggerState.Normal => JobStatus.NotStarted, // Scheduled but not started
                TriggerState.Paused => JobStatus.NotStarted,
                TriggerState.Complete => JobStatus.Finished,
                TriggerState.Error => JobStatus.Finished,
                TriggerState.Blocked => JobStatus.Running,
                TriggerState.None => JobStatus.Finished,
                _ => JobStatus.NotStarted
            };

            // Get execution times from job data map if stored
            if (jobDetail.JobDataMap.ContainsKey("startedAt"))
            {
                startedAt = jobDetail.JobDataMap.GetDateTime("startedAt");
            }
            
            if (jobDetail.JobDataMap.ContainsKey("finishedAt"))
            {
                finishedAt = jobDetail.JobDataMap.GetDateTime("finishedAt");
            }
        }
        else
        {
            // No trigger means job has completed and trigger was removed
            status = JobStatus.Finished;
            
            if (jobDetail.JobDataMap.ContainsKey("startedAt"))
            {
                startedAt = jobDetail.JobDataMap.GetDateTime("startedAt");
            }
            
            if (jobDetail.JobDataMap.ContainsKey("finishedAt"))
            {
                finishedAt = jobDetail.JobDataMap.GetDateTime("finishedAt");
            }
        }

        return new JobStatusModel
        {
            RunId = runId,
            Status = status,
            CreatedAt = jobDetail.JobDataMap.ContainsKey("createdAt") 
                ? jobDetail.JobDataMap.GetDateTime("createdAt") 
                : DateTime.UtcNow,
            StartedAt = startedAt,
            FinishedAt = finishedAt
        };
    }

    public async Task<bool> JobExistsAsync(string runId)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"consensus-{runId}", "consensus-jobs");
        return await scheduler.CheckExists(jobKey);
    }
}
