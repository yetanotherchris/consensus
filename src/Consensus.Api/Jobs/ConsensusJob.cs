using Consensus;
using Quartz;

namespace Consensus.Api.Jobs;

/// <summary>
/// Background job that executes the consensus building process
/// </summary>
public class ConsensusJob : IJob
{
    private readonly ILogger<ConsensusJob> _logger;
    private readonly ConsensusOrchestrator _orchestrator;

    public ConsensusJob(
        ILogger<ConsensusJob> logger,
        ConsensusOrchestrator orchestrator)
    {
        _logger = logger;
        _orchestrator = orchestrator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var runId = context.JobDetail.JobDataMap.GetString("runId");
        var prompt = context.JobDetail.JobDataMap.GetString("prompt");
        var models = context.JobDetail.JobDataMap.Get("models") as string[];

        if (string.IsNullOrEmpty(runId))
        {
            _logger.LogError("ConsensusJob executed without a runId");
            return;
        }

        if (string.IsNullOrEmpty(prompt))
        {
            _logger.LogError("ConsensusJob executed without a prompt for runId: {RunId}", runId);
            return;
        }

        if (models == null || models.Length == 0)
        {
            _logger.LogError("ConsensusJob executed without models for runId: {RunId}", runId);
            return;
        }

        _logger.LogInformation("ConsensusJob started for runId: {RunId} with {ModelCount} models: {Models}",
            runId, models.Length, string.Join(", ", models));

        // Store start time in job data map
        context.JobDetail.JobDataMap.Put("startedAt", DateTime.UtcNow);

        try
        {
            // Execute consensus building process
            var result = await _orchestrator.GetConsensusAsync(prompt, models, runId);

            // Save the output (markdown, HTML, logs) with runId as filename identifier
            _logger.LogInformation("Saving consensus results for runId: {RunId}", runId);
            await _orchestrator.SaveConsensusAsync(result, runId);

            _logger.LogInformation("ConsensusJob completed successfully for runId: {RunId}, Consensus Level: {ConsensusLevel}",
                runId, result.ConsensusLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConsensusJob failed for runId: {RunId}", runId);
            // Don't rethrow - mark as finished even on error
        }
        finally
        {
            // Store finish time in job data map
            context.JobDetail.JobDataMap.Put("finishedAt", DateTime.UtcNow);
        }
    }
}
