using System.Text;
using ConsensusAgent.Configuration;
using ConsensusAgent.Logging;
using ConsensusAgent.Services;
using ConsensusAgent.Utilities;

namespace ConsensusAgent;

/// <summary>
/// Orchestrates the entire consensus building process
/// </summary>
public class ConsensusOrchestrator
{
    private readonly ConsensusConfiguration _config;
    private readonly SimpleLogger _logger;
    private readonly IAgentService _agentService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IVotingService _votingService;
    private readonly IOutputService _outputService;
    private readonly List<string> _conversationHistory = new();

    public ConsensusOrchestrator(
        ConsensusConfiguration config,
        SimpleLogger logger,
        IAgentService agentService,
        IPromptBuilder promptBuilder,
        IVotingService votingService,
        IOutputService outputService)
    {
        _config = config;
        _logger = logger;
        _agentService = agentService;
        _promptBuilder = promptBuilder;
        _votingService = votingService;
        _outputService = outputService;
    }

    public async Task<string> BuildConsensusAsync(string initialPrompt)
    {
        _logger.LogInformation("=== CONSENSUS BUILDING STARTED ===");
        _logger.LogInformation("Models: {0}", string.Join(", ", _config.Models));
        _logger.LogInformation("Rounds: {0}", _config.MaxRounds);
        _logger.LogInformation("Timeout per query: {0}s", _config.QueryTimeoutSeconds);

        // Initialize agents for each model
        _logger.LogInformation("Initializing agents for each model...");
        await _agentService.InitializeAgentsAsync(_config.Models, _config.ApiEndpoint, _config.ApiKey);
        _logger.LogInformation("✓ Initialized {AgentCount} agent contexts", _config.Models.Length);

        string currentPrompt = initialPrompt;

        // Run consensus rounds
        for (int round = 1; round <= _config.MaxRounds; round++)
        {
            _logger.LogInformation("=== ROUND {0}/{1} ===", round, _config.MaxRounds);

            var roundResponses = await QueryAllModelsAsync(currentPrompt, round);

            // Voting phase (skip on last round)
            if (round < _config.MaxRounds && roundResponses.Count > 0)
            {
                _logger.LogInformation("Conducting vote...");
                string vote = await ConductVoteWithTimeoutAsync(roundResponses);
                
                // Update prompt for next round
                currentPrompt = _promptBuilder.BuildNextRoundPrompt(initialPrompt, roundResponses, vote, round);
            }
            else if (roundResponses.Count == 0)
            {
                _logger.LogWarning("Round {0} had no successful responses", round);
            }
        }

        // Generate final consensus
        _logger.LogInformation("Generating final consensus...");
        
        string finalConsensus = await GenerateFinalConsensusAsync(initialPrompt);
        
        _logger.LogInformation("=== CONSENSUS BUILDING COMPLETED ===");

        return finalConsensus;
    }

    private async Task<List<(string model, string response)>> QueryAllModelsAsync(string prompt, int round)
    {
        var roundResponses = new List<(string model, string response)>();

        foreach (string model in _config.Models)
        {
            _logger.LogInformation("Querying {Model}...", model);
            
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.QueryTimeoutSeconds));
                string response = await _agentService.QueryModelAsync(model, prompt, cts.Token);
                
                roundResponses.Add((model, response));
                _logger.LogInformation("✓ {Model} completed successfully", model);
                
                // Add to conversation history
                _conversationHistory.Add($"[{model}] {response}");
                
                // Log with truncation for readability
                string truncatedResponse = TextHelper.Truncate(response, 200);
                _logger.LogInformation("Model: {0} | Response: {1}", model, truncatedResponse);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Model {0} timed out after {1}s - skipping", model, _config.QueryTimeoutSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Model {0} failed", model);
            }
        }

        return roundResponses;
    }

    private async Task<string> ConductVoteWithTimeoutAsync(List<(string model, string response)> responses)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.QueryTimeoutSeconds));
            var vote = await _votingService.ConductVoteAsync(responses, _config.Models[0], cts.Token);
            
            _logger.LogInformation("Vote result: {0}", TextHelper.Truncate(vote, 150));
            
            return vote;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Vote timed out - proceeding without vote result");
            return "Vote timed out";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vote failed");
            return "Vote failed";
        }
    }

    private async Task<string> GenerateFinalConsensusAsync(string originalPrompt)
    {
        try
        {
            // Double timeout for final consensus
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.QueryTimeoutSeconds * 2));
            
            string consensusPrompt = _promptBuilder.BuildFinalConsensusPrompt(
                originalPrompt,
                _conversationHistory,
                _config.MaxRounds);
            
            string consensus = await _agentService.QueryModelOneOffAsync(
                _config.Models[0],
                consensusPrompt,
                _config.ApiEndpoint,
                _config.ApiKey,
                cts.Token);
            
            _logger.LogInformation("Final consensus generated: {0}", TextHelper.Truncate(consensus, 300));
            
            // Format as Markdown with metadata
            return FormatConsensusOutput(consensus);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Final consensus generation timed out");
            return "# Consensus Generation Timeout\n\nThe final consensus generation timed out. Please review the conversation log for individual model responses.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Final consensus generation failed");
            return $"# Consensus Generation Error\n\nAn error occurred: {ex.Message}";
        }
    }

    private string FormatConsensusOutput(string consensus)
    {
        var output = new StringBuilder();
        output.AppendLine("# Consensus Response");
        output.AppendLine();
        output.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine($"**Models Consulted:** {_config.Models.Length}");
        output.AppendLine($"**Discussion Rounds:** {_config.MaxRounds}");
        output.AppendLine();
        output.AppendLine("---");
        output.AppendLine();
        output.AppendLine(consensus);

        return output.ToString();
    }

    public async Task SaveConsensusAsync(string consensus)
    {
        await _outputService.SaveConsensusAsync(consensus, _config.ConsensusFile);
    }
}
