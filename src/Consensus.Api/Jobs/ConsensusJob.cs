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

    // Hardcoded models from models.txt
    private static readonly string[] Models = new[]
    {
        "anthropic/claude-sonnet-4",
        "x-ai/grok-3",
        "qwen/qwen3-vl-235b-a22b-thinking",
        "alibaba/tongyi-deepresearch-30b-a3b",
        "google/gemini-2.5-pro",
        "openai/gpt-5"
    };

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
            // Execute consensus building process
            _logger.LogInformation("Building consensus for runId: {RunId} with {ModelCount} models", runId, Models.Length);
            
            var result = await _orchestrator.GetConsensusAsync(prompt, Models);
            
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
