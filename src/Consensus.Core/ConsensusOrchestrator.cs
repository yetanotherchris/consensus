using System.Diagnostics;
using System.Text;
using Consensus.Configuration;
using Consensus.Logging;
using Consensus.Models;
using Consensus.Services;

namespace Consensus;

/// <summary>
/// Orchestrates the parallel-then-synthesize consensus building process
/// </summary>
public class ConsensusOrchestrator
{
    private readonly ConsensusConfiguration _config;
    private readonly SimpleFileLogger _logger;
    private readonly IAgentService _agentService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ISynthesizerService _synthesizerService;
    private readonly IMarkdownOutputService _markdownOutputService;
    private readonly IHtmlOutputService _htmlOutputService;

    public ConsensusOrchestrator(
        ConsensusConfiguration config,
        SimpleFileLogger logger,
        IAgentService agentService,
        IPromptBuilder promptBuilder,
        ISynthesizerService synthesizerService,
        IMarkdownOutputService markdownOutputService,
        IHtmlOutputService htmlOutputService)
    {
        _config = config;
        _logger = logger;
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
    public async Task<ConsensusResult> GetConsensusAsync(string prompt, string[] models)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Calculate minimum agents required: at least 2/3 of models, minimum of 3
        int minimumAgentsRequired = Math.Max(3, models.Length * 2 / 3);
        
        _logger.LogInformation("=== CONSENSUS BUILDING STARTED (Parallel-then-Synthesize) ===");
        _logger.LogInformation("Models: {0}", string.Join(", ", models));
        _logger.LogInformation("Timeout per agent: {0}s", _config.AgentTimeoutSeconds);
        _logger.LogInformation("Minimum agents required: {0}", minimumAgentsRequired);

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
        _logger.LogInformation("Initializing agents for each model...");
        await _agentService.InitializeAgentsAsync(models, _config.ApiEndpoint, _config.ApiKey);
        _logger.LogInformation("✓ Initialized {0} agent contexts", models.Length);

        // Phase 1: Divergent Collection (Parallel)
        _logger.LogInformation("=== PHASE 1: DIVERGENT COLLECTION (PARALLEL) ===");
        var responses = await CollectResponsesAsync(request, models);

        // Check minimum threshold
        if (responses.Count < minimumAgentsRequired)
        {
            throw new InvalidOperationException(
                $"Only {responses.Count} of {models.Length} agents responded successfully. " +
                $"Minimum required: {minimumAgentsRequired}");
        }

        _logger.LogInformation("✓ Collected {0} responses (minimum: {1})", 
            responses.Count, minimumAgentsRequired);

        // Phase 2: Convergent Synthesis
        _logger.LogInformation("=== PHASE 2: CONVERGENT SYNTHESIS ===");
        using var synthesisCts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.AgentTimeoutSeconds * 2));
        string judgeModel = models[0];
        var result = await _synthesizerService.SynthesizeAsync(prompt, responses, judgeModel, synthesisCts.Token);

        stopwatch.Stop();
        result.TotalProcessingTime = stopwatch.Elapsed;

        _logger.LogInformation("=== CONSENSUS BUILDING COMPLETED ===");
        _logger.LogInformation("Total time: {0:F2}s", result.TotalProcessingTime.TotalSeconds);
        _logger.LogInformation("Consensus level: {0}", result.ConsensusLevel);

        return result;
    }

    /// <summary>
    /// Collect responses from all models in parallel (Phase 1: Divergent)
    /// </summary>
    /// <param name="request">The consensus request</param>
    /// <param name="models">Array of model names to query</param>
    public async Task<List<ModelResponse>> CollectResponsesAsync(ConsensusRequest request, string[] models)
    {
        // Build enhanced prompt
        string enhancedPrompt = _promptBuilder.BuildEnhancedDivergentPrompt(request);
        
        _logger.LogInformation("Querying {0} models in parallel...", models.Length);

        // Create tasks for all models with individual timeouts
        var queryTasks = models.Select(async model =>
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.AgentTimeoutSeconds));
                _logger.LogInformation("Querying {0}...", model);
                
                var response = await _agentService.QueryModelWithResponseAsync(model, enhancedPrompt, cts.Token);
                
                _logger.LogInformation("✓ {0} completed successfully (Confidence: {1:P0})", 
                    model, response.ConfidenceScore);
                
                return (success: true, response: response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("✗ {0} timed out after {1}s", model, _config.AgentTimeoutSeconds);
                return (success: false, response: (ModelResponse?)null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ {0} failed", model);
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
    public async Task SaveConsensusAsync(ConsensusResult result)
    {
        // Save Markdown output - id is handled by the output services/writer
        await _markdownOutputService.SaveConsensusResultAsync(result);
        
        // Save HTML output - id is handled by the output services/writer
        await _htmlOutputService.SaveConsensusResultAsync(result);
    }
}
