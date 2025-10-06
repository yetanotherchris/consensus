using System.Diagnostics;
using System.Text;
using Consensus.Configuration;
using Consensus.Logging;
using Consensus.Models;
using Consensus.Services;
using Consensus.Channels;

namespace Consensus;

/// <summary>
/// Orchestrates the parallel-then-synthesize consensus building process
/// </summary>
public class ConsensusOrchestrator
{
    private readonly ConsensusConfiguration _config;
    private readonly ConsensusRunTracker _runTracker;
    private readonly IAgentService _agentService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ISynthesizerService _synthesizerService;
    private readonly IMarkdownOutputService _markdownOutputService;
    private readonly IHtmlOutputService _htmlOutputService;

    public ConsensusOrchestrator(
        ConsensusConfiguration config,
        ConsensusRunTracker runTracker,
        IAgentService agentService,
        IPromptBuilder promptBuilder,
        ISynthesizerService synthesizerService,
        IMarkdownOutputService markdownOutputService,
        IHtmlOutputService htmlOutputService)
    {
        _config = config;
        _runTracker = runTracker;
        _agentService = agentService;
        _promptBuilder = promptBuilder;
        _synthesizerService = synthesizerService;
        _markdownOutputService = markdownOutputService;
        _htmlOutputService = htmlOutputService;
    }

    /// <summary>
    /// Execute the parallel-then-synthesize consensus workflow
    /// </summary>
    /// <param name="prompt">The prompt to send to all models</param>
    /// <param name="models">Array of model names to query</param>
    /// <param name="runId">Optional run identifier for tracking. If not provided, a new GUID will be generated.</param>
    public async Task<ConsensusResult> GetConsensusAsync(string prompt, string[] models, string? runId = null)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Generate run ID if not provided
        runId ??= Guid.NewGuid().ToString();
        
        // Calculate minimum agents required: at least 2/3 of models, minimum of 3
        int minimumAgentsRequired = Math.Max(3, models.Length * 2 / 3);
        
        _runTracker.WriteLog(runId, "=== CONSENSUS BUILDING STARTED (Parallel-then-Synthesize) ===");
        _runTracker.WriteLog(runId, $"Run ID: {runId}");
        _runTracker.WriteLog(runId, $"Models: {string.Join(", ", models)}");
        _runTracker.WriteLog(runId, $"Timeout per agent: {_config.AgentTimeoutSeconds}s");
        _runTracker.WriteLog(runId, $"Minimum agents required: {minimumAgentsRequired}");

        // Create consensus request
        var request = new ConsensusRequest
        {
            Prompt = prompt,
            Domain = _config.Domain,
            IncludeReasoning = true,
            IncludeConfidence = true,
            IncludeTheoreticalFramework = false,
            MinimumAgents = minimumAgentsRequired
        };

        // Initialize agents
        await _agentService.InitializeAgentsAsync(models, _config.ApiEndpoint, _config.ApiKey, runId);

        // Phase 1: Divergent Collection (Parallel)
        _runTracker.WriteLog(runId, "=== PHASE 1: DIVERGENT COLLECTION (PARALLEL) ===");
        var responses = await CollectResponsesAsync(request, models, runId);

        // Check minimum threshold
        if (responses.Count < minimumAgentsRequired)
        {
            var errorMsg = $"Only {responses.Count} of {models.Length} agents responded successfully. " +
                $"Minimum required: {minimumAgentsRequired}";
            _runTracker.WriteLog(runId, $"ERROR: {errorMsg}");
            throw new InvalidOperationException(errorMsg);
        }

        _runTracker.WriteLog(runId, $"✓ Collected {responses.Count} responses (minimum: {minimumAgentsRequired})");

        // Phase 2: Convergent Synthesis
        _runTracker.WriteLog(runId, "=== PHASE 2: CONVERGENT SYNTHESIS ===");
        using var synthesisCts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.AgentTimeoutSeconds * 2));
        string judgeModel = models[0];
        var result = await _synthesizerService.SynthesizeAsync(prompt, responses, judgeModel, runId, synthesisCts.Token);

        stopwatch.Stop();
        result.TotalProcessingTime = stopwatch.Elapsed;

        _runTracker.WriteLog(runId, "=== CONSENSUS BUILDING COMPLETED ===");
        _runTracker.WriteLog(runId, $"Total time: {result.TotalProcessingTime.TotalSeconds:F2}s");
        _runTracker.WriteLog(runId, $"Consensus level: {result.ConsensusLevel}");

        return result;
    }

    /// <summary>
    /// Collect responses from all models in parallel (Phase 1: Divergent)
    /// </summary>
    /// <param name="request">The consensus request</param>
    /// <param name="models">Array of model names to query</param>
    /// <param name="runId">The run identifier for tracking</param>
    public async Task<List<ModelResponse>> CollectResponsesAsync(ConsensusRequest request, string[] models, string runId)
    {
        // Build enhanced prompt
        string enhancedPrompt = _promptBuilder.BuildEnhancedDivergentPrompt(request);
        
        _runTracker.WriteLog(runId, $"Querying {models.Length} models in parallel...");

        // Create tasks for all models with individual timeouts
        var queryTasks = models.Select(async model =>
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.AgentTimeoutSeconds));
                _runTracker.WriteLog(runId, $"Querying {model}...");
                
                var response = await _agentService.QueryModelWithResponseAsync(model, enhancedPrompt, runId, cts.Token);
                
                _runTracker.WriteLog(runId, $"✓ {model} completed successfully (Confidence: {response.ConfidenceScore:P0})");
                
                return (success: true, response: response);
            }
            catch (OperationCanceledException)
            {
                _runTracker.WriteLog(runId, $"✗ {model} timed out after {_config.AgentTimeoutSeconds}s");
                return (success: false, response: (ModelResponse?)null);
            }
            catch (Exception ex)
            {
                _runTracker.WriteLog(runId, $"✗ {model} failed: {ex.Message}");
                return (success: false, response: (ModelResponse?)null);
            }
        }).ToList();

        // Wait for all tasks to complete (or timeout)
        var results = await Task.WhenAll(queryTasks);

        // Extract successful responses
        var responses = results
            .Where(r => r.success && r.response != null)
            .Select(r => r.response!)
            .ToList();

        return responses;
    }

    /// <summary>
    /// Save consensus result (both Markdown and HTML)
    /// </summary>
    /// <param name="result">The consensus result to save</param>
    /// <param name="id">Optional identifier for the output filenames</param>
    public async Task SaveConsensusAsync(ConsensusResult result, string? id = null)
    {
        // Save Markdown output with optional id
        await _markdownOutputService.SaveConsensusResultAsync(result, id);
        
        // Save HTML output with optional id
        await _htmlOutputService.SaveConsensusResultAsync(result, id);
    }
}
