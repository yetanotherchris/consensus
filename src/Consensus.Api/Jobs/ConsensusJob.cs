using Quartz;

namespace Consensus.Api.Jobs;

/// <summary>
/// Background job that executes the consensus building process
/// </summary>
public class ConsensusJob : IJob
{
    private readonly ILogger<ConsensusJob> _logger;

    public ConsensusJob(ILogger<ConsensusJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var runId = context.JobDetail.JobDataMap.GetString("runId");
        
        if (string.IsNullOrEmpty(runId))
        {
            _logger.LogError("ConsensusJob executed without a runId");
            return;
        }

        _logger.LogInformation("ConsensusJob started for runId: {RunId}", runId);
        
        // Store start time in job data map
        context.JobDetail.JobDataMap.Put("startedAt", DateTime.UtcNow);

        try
        {
            // TODO: Execute actual consensus building process
            // This is where you would:
            // 1. Call ConsensusOrchestrator
            // 2. Pass the prompt/request
            // 3. Get the consensus result
            // 4. Save the output (markdown, HTML, logs)
            
            // For now, just simulate work with a delay
            await Task.Delay(100);
            
            _logger.LogInformation("ConsensusJob completed for runId: {RunId}", runId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConsensusJob failed for runId: {RunId}", runId);
            throw;
        }
        finally
        {
            // Store finish time in job data map
            context.JobDetail.JobDataMap.Put("finishedAt", DateTime.UtcNow);
        }
    }
}
