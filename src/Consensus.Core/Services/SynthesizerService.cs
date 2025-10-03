using System.Text.RegularExpressions;
using Consensus.Configuration;
using Consensus.Logging;
using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Judge/Synthesizer service that evaluates all model responses and produces final consensus
/// </summary>
public class SynthesizerService : ISynthesizerService
{
    private readonly IAgentService _agentService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ConsensusConfiguration _config;
    private readonly SimpleLogger _logger;

    public SynthesizerService(
        IAgentService agentService,
        IPromptBuilder promptBuilder,
        ConsensusConfiguration config,
        SimpleLogger logger)
    {
        _agentService = agentService;
        _promptBuilder = promptBuilder;
        _config = config;
        _logger = logger;
    }

    public async Task<ConsensusResult> SynthesizeAsync(
        string originalPrompt, 
        List<ModelResponse> responses, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting synthesis of {0} responses...", responses.Count);

        // Build the judge prompt
        string judgePrompt = _promptBuilder.BuildJudgeSynthesisPrompt(originalPrompt, responses);

        // Query the judge model (use first model as judge)
        string judgeModelName = _config.Models[0];
        _logger.LogInformation("Using {0} as synthesis judge...", judgeModelName);

        string synthesisResponse = await _agentService.QueryModelOneOffAsync(
            judgeModelName,
            judgePrompt,
            _config.ApiEndpoint,
            _config.ApiKey,
            cancellationToken);

        // Parse the synthesis response
        var result = ParseSynthesisResponse(synthesisResponse, responses);
        
        _logger.LogInformation("Synthesis complete. Consensus level: {0}", result.ConsensusLevel);
        
        return result;
    }

    private ConsensusResult ParseSynthesisResponse(string synthesisResponse, List<ModelResponse> originalResponses)
    {
        // Extract structured sections from the synthesis response
        var result = new ConsensusResult
        {
            SynthesizedAnswer = ExtractSection(synthesisResponse, "SYNTHESIZED ANSWER"),
            SynthesisReasoning = ExtractSection(synthesisResponse, "REASONING"),
            OverallConfidence = ParseConfidence(ExtractSection(synthesisResponse, "CONFIDENCE")),
            ConsensusLevel = ParseConsensusLevel(ExtractSection(synthesisResponse, "CONSENSUS LEVEL")),
            IndividualResponses = originalResponses,
            AgreementPoints = ParseAgreementPoints(ExtractSection(synthesisResponse, "AGREEMENT POINTS")),
            Disagreements = ParseDisagreements(ExtractSection(synthesisResponse, "DISAGREEMENTS"))
        };

        // If parsing failed, use the full response as the answer
        if (string.IsNullOrWhiteSpace(result.SynthesizedAnswer))
        {
            result.SynthesizedAnswer = synthesisResponse;
            _logger.LogWarning("Could not parse structured synthesis response, using full response");
        }

        return result;
    }

    private string ExtractSection(string response, string sectionName)
    {
        // Try to extract content after "SECTION_NAME:"
        var pattern = $@"{Regex.Escape(sectionName)}:\s*(.+?)(?=\n[A-Z\s]+:|$)";
        var match = Regex.Match(response, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return string.Empty;
    }

    private double ParseConfidence(string confidenceText)
    {
        if (string.IsNullOrWhiteSpace(confidenceText))
            return 0.0;

        // Try to extract a number (0-100)
        var match = Regex.Match(confidenceText, @"(\d+(?:\.\d+)?)");
        if (match.Success && double.TryParse(match.Groups[1].Value, out double value))
        {
            // Convert to 0.0-1.0 range if it looks like a percentage
            return value > 1.0 ? value / 100.0 : value;
        }

        return 0.0;
    }

    private ConsensusLevel ParseConsensusLevel(string levelText)
    {
        if (string.IsNullOrWhiteSpace(levelText))
            return ConsensusLevel.Conflicted;

        levelText = levelText.ToLowerInvariant();

        if (levelText.Contains("strong"))
            return ConsensusLevel.StrongConsensus;
        if (levelText.Contains("moderate"))
            return ConsensusLevel.ModerateConsensus;
        if (levelText.Contains("weak"))
            return ConsensusLevel.WeakConsensus;

        return ConsensusLevel.Conflicted;
    }

    private List<ConsensusPoint> ParseAgreementPoints(string agreementText)
    {
        var points = new List<ConsensusPoint>();

        if (string.IsNullOrWhiteSpace(agreementText))
            return points;

        // Split by bullet points or numbered lists
        var lines = agreementText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            // Remove bullet characters (•), dashes, asterisks, numbers, dots, and whitespace from the start
            var cleaned = Regex.Replace(line, @"^[\s\-\*\d\.\u2022]+", "").Trim();
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                points.Add(new ConsensusPoint
                {
                    Point = cleaned,
                    SupportingModels = 0, // Would need semantic analysis to determine
                    ModelNames = new List<string>()
                });
            }
        }

        return points;
    }

    private List<Disagreement> ParseDisagreements(string disagreementText)
    {
        var disagreements = new List<Disagreement>();

        if (string.IsNullOrWhiteSpace(disagreementText))
            return disagreements;

        // For now, create a single disagreement entry with the full text
        // A more sophisticated parser could extract multiple disagreements
        disagreements.Add(new Disagreement
        {
            Topic = "Model Disagreements",
            Views = new List<DissentingView>(),
            IsLegitimateTheoretical = false // Would need deeper analysis
        });

        return disagreements;
    }
}
