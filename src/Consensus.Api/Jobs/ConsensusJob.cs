using Consensus;
using Consensus.Configuration;
using Quartz;

namespace Consensus.Api.Jobs;

/// <summary>
/// Background job that executes the consensus building process
/// </summary>
public class ConsensusJob : IJob
{
    private readonly ILogger<ConsensusJob> _logger;
    private readonly ConsensusOrchestrator _orchestrator;
    private readonly ConsensusConfiguration _configuration;

    public ConsensusJob(
        ILogger<ConsensusJob> logger,
        ConsensusOrchestrator orchestrator,
        ConsensusConfiguration configuration)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _configuration = configuration;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var runId = context.JobDetail.JobDataMap.GetString("runId");
        var prompt = context.JobDetail.JobDataMap.GetString("prompt");
        
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

        _logger.LogInformation("ConsensusJob started for runId: {RunId}", runId);
        
        // Store start time in job data map
        context.JobDetail.JobDataMap.Put("startedAt", DateTime.UtcNow);

        try
        {
            // Validate that models are configured
            if (_configuration.Models == null || _configuration.Models.Length == 0)
            {
                _logger.LogError("No models configured in appsettings.json for runId: {RunId}", runId);
                throw new InvalidOperationException("No models configured. Please add Consensus:Models to appsettings.json or set CONSENSUS__MODELS environment variable.");
            }

            // Execute consensus building process
            _logger.LogInformation("Building consensus for runId: {RunId} with {ModelCount} models", runId, _configuration.Models.Length);

            var result = await _orchestrator.GetConsensusAsync(prompt, _configuration.Models, runId);

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
