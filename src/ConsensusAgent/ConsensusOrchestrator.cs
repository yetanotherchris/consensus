using System.Diagnostics;
using System.Text;
using ConsensusAgent.Configuration;
using ConsensusAgent.Logging;
using ConsensusAgent.Models;
using ConsensusAgent.Services;

namespace ConsensusAgent;

/// <summary>
/// Orchestrates the parallel-then-synthesize consensus building process
/// </summary>
public class ConsensusOrchestrator
{
    private readonly ConsensusConfiguration _config;
    private readonly SimpleLogger _logger;
    private readonly IAgentService _agentService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ISynthesizerService _synthesizerService;
    private readonly IOutputService _outputService;

    public ConsensusOrchestrator(
        ConsensusConfiguration config,
        SimpleLogger logger,
        IAgentService agentService,
        IPromptBuilder promptBuilder,
        ISynthesizerService synthesizerService,
        IOutputService outputService)
    {
        _config = config;
        _logger = logger;
        _agentService = agentService;
        _promptBuilder = promptBuilder;
        _synthesizerService = synthesizerService;
        _outputService = outputService;
    }

    /// <summary>
    /// Execute the parallel-then-synthesize consensus workflow
    /// </summary>
    public async Task<ConsensusResult> GetConsensusAsync(string prompt)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("=== CONSENSUS BUILDING STARTED (Parallel-then-Synthesize) ===");
        _logger.LogInformation("Models: {0}", string.Join(", ", _config.Models));
        _logger.LogInformation("Timeout per agent: {0}s", _config.AgentTimeoutSeconds);
        _logger.LogInformation("Minimum agents required: {0}", _config.MinimumAgentsRequired);

        // Create consensus request
        var request = new ConsensusRequest
        {
            Prompt = prompt,
            Domain = _config.Domain,
            IncludeReasoning = true,
            IncludeConfidence = true,
            IncludeTheoreticalFramework = false,
            MinimumAgents = _config.MinimumAgentsRequired
        };

        // Initialize agents
        _logger.LogInformation("Initializing agents for each model...");
        await _agentService.InitializeAgentsAsync(_config.Models, _config.ApiEndpoint, _config.ApiKey);
        _logger.LogInformation("✓ Initialized {0} agent contexts", _config.Models.Length);

        // Phase 1: Divergent Collection (Parallel)
        _logger.LogInformation("=== PHASE 1: DIVERGENT COLLECTION (PARALLEL) ===");
        var responses = await CollectResponsesAsync(request);

        // Check minimum threshold
        if (responses.Count < _config.MinimumAgentsRequired)
        {
            throw new InvalidOperationException(
                $"Only {responses.Count} of {_config.Models.Length} agents responded successfully. " +
                $"Minimum required: {_config.MinimumAgentsRequired}");
        }

        _logger.LogInformation("✓ Collected {0} responses (minimum: {1})", 
            responses.Count, _config.MinimumAgentsRequired);

        // Phase 2: Convergent Synthesis
        _logger.LogInformation("=== PHASE 2: CONVERGENT SYNTHESIS ===");
        using var synthesisCts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.AgentTimeoutSeconds * 2));
        var result = await _synthesizerService.SynthesizeAsync(prompt, responses, synthesisCts.Token);

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
    public async Task<List<ModelResponse>> CollectResponsesAsync(ConsensusRequest request)
    {
        // Build enhanced prompt
        string enhancedPrompt = _promptBuilder.BuildEnhancedDivergentPrompt(request);
        
        _logger.LogInformation("Querying {0} models in parallel...", _config.Models.Length);

        // Create tasks for all models with individual timeouts
        var queryTasks = _config.Models.Select(async model =>
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
    /// Save consensus result to file
    /// </summary>
    public async Task SaveConsensusAsync(ConsensusResult result)
    {
        await _outputService.SaveConsensusResultAsync(result, _config.ConsensusFile);
    }
}
