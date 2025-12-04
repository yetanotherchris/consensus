using System.Text.RegularExpressions;
using System.Xml.Linq;
using Consensus.Channels;
using Consensus.Configuration;
using Consensus.Models;
using Microsoft.Extensions.Logging;

namespace Consensus.Services;

/// <summary>
/// Judge/Synthesizer service that evaluates all model responses and produces final consensus
/// </summary>
public class SynthesizerService : ISynthesizerService
{
    private readonly IAgentService _agentService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ConsensusConfiguration _config;
    private readonly ConsensusRunTracker _runTracker;
    private readonly ILogger<SynthesizerService> _logger;

    public SynthesizerService(
        IAgentService agentService,
        IPromptBuilder promptBuilder,
        ConsensusConfiguration config,
        ConsensusRunTracker runTracker,
        ILogger<SynthesizerService> logger)
    {
        _agentService = agentService;
        _promptBuilder = promptBuilder;
        _config = config;
        _runTracker = runTracker;
        _logger = logger;
    }

    public async Task<ConsensusResult> SynthesizeAsync(
        string originalPrompt,
        List<ModelResponse> responses,
        string judgeModel,
        string runId,
        CancellationToken cancellationToken = default)
    {
        _runTracker.WriteLog(runId, $"Starting synthesis of {responses.Count} responses...");

        try
        {
            // Build the judge prompt
            string judgePrompt = _promptBuilder.BuildJudgeSynthesisPrompt(originalPrompt, responses);

            // Query the judge model with progress logging
            _runTracker.WriteLog(runId, $"Using {judgeModel} as synthesis judge...");

            var progressStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var progressLoggingTask = LogSynthesisProgressAsync(runId, progressStopwatch, cancellationToken);

            string synthesisResponse = await _agentService.QueryModelOneOffAsync(
                judgeModel,
                judgePrompt,
                _config.ApiEndpoint,
                _config.ApiKey,
                runId,
                cancellationToken);

            progressStopwatch.Stop();

            // Parse the synthesis response
            var result = ParseSynthesisResponse(synthesisResponse, responses, originalPrompt, runId);

            _runTracker.WriteLog(runId, $"Synthesis complete. Consensus level: {result.ConsensusLevel}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during synthesis for runId {RunId}", runId);
            throw;
        }
    }

    private async Task LogSynthesisProgressAsync(string runId, System.Diagnostics.Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        try
        {
            const int logIntervalSeconds = 15;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(logIntervalSeconds), cancellationToken);
                var elapsed = stopwatch.Elapsed;
                _runTracker.WriteLog(runId, $"Synthesis in progress... elapsed: {elapsed.TotalSeconds:F0}s");
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in synthesis progress logging for runId {RunId}", runId);
        }
    }

    private ConsensusResult ParseSynthesisResponse(string synthesisResponse, List<ModelResponse> originalResponses, string originalPrompt, string runId)
    {
        // Try XML parsing first
        try
        {
            return ParseXmlSynthesisResponse(synthesisResponse, originalResponses, originalPrompt, runId);
        }
        catch (Exception ex)
        {
            _runTracker.WriteLog(runId, $"XML parsing failed: {ex.Message}, falling back to text parsing");
        }

        // Fall back to text-based parsing for backwards compatibility
        return ParseTextSynthesisResponse(synthesisResponse, originalResponses, originalPrompt, runId);
    }

    private ConsensusResult ParseXmlSynthesisResponse(string synthesisResponse, List<ModelResponse> originalResponses, string originalPrompt, string runId)
    {
        // Extract the <synthesis> block if it exists
        var synthesisMatch = Regex.Match(synthesisResponse, @"<synthesis>(.+?)</synthesis>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        string xmlContent = synthesisMatch.Success ? $"<synthesis>{synthesisMatch.Groups[1].Value}</synthesis>" : synthesisResponse;

        var doc = XDocument.Parse(xmlContent);
        var synthesis = doc.Root ?? throw new Exception("No root element found");

        var result = new ConsensusResult
        {
            SynthesizedAnswer = synthesis.Element("synthesized_answer")?.Value.Trim() ?? string.Empty,
            SynthesisReasoning = synthesis.Element("reasoning")?.Value.Trim() ?? string.Empty,
            Summary = synthesis.Element("summary")?.Value.Trim() ?? "No summary provided",
            OverallConfidence = ParseConfidence(synthesis.Element("confidence")?.Value ?? "0"),
            ConsensusLevel = ParseConsensusLevel(synthesis.Element("consensus_level")?.Value ?? "Conflicted"),
            IndividualResponses = originalResponses,
            AgreementPoints = ParseXmlAgreementPoints(synthesis.Element("agreement_points")),
            Disagreements = ParseXmlDisagreements(synthesis.Element("disagreements")),
            OriginalPrompt = originalPrompt
        };

        if (string.IsNullOrWhiteSpace(result.SynthesizedAnswer))
        {
            throw new Exception("Synthesized answer is empty");
        }

        return result;
    }

    private List<ConsensusPoint> ParseXmlAgreementPoints(XElement? agreementPointsElement)
    {
        var points = new List<ConsensusPoint>();

        if (agreementPointsElement == null)
            return points;

        foreach (var pointElement in agreementPointsElement.Elements("point"))
        {
            var pointText = pointElement.Value.Trim();
            if (!string.IsNullOrWhiteSpace(pointText))
            {
                points.Add(new ConsensusPoint
                {
                    Point = pointText,
                    SupportingModels = 0,
                    ModelNames = new List<string>()
                });
            }
        }

        return points;
    }

    private List<Disagreement> ParseXmlDisagreements(XElement? disagreementsElement)
    {
        var disagreements = new List<Disagreement>();

        if (disagreementsElement == null)
            return disagreements;

        foreach (var disagreementElement in disagreementsElement.Elements("disagreement"))
        {
            var topic = disagreementElement.Element("topic")?.Value.Trim();
            if (string.IsNullOrWhiteSpace(topic))
                continue;

            var disagreement = new Disagreement
            {
                Topic = topic,
                Views = new List<DissentingView>()
            };

            var viewsElement = disagreementElement.Element("views");
            if (viewsElement != null)
            {
                foreach (var viewElement in viewsElement.Elements("view"))
                {
                    var modelName = viewElement.Element("model")?.Value.Trim();
                    var position = viewElement.Element("position")?.Value.Trim();

                    if (!string.IsNullOrWhiteSpace(modelName) && !string.IsNullOrWhiteSpace(position))
                    {
                        disagreement.Views.Add(new DissentingView
                        {
                            ModelName = modelName,
                            Position = position
                        });
                    }
                }
            }

            if (disagreement.Views.Any())
            {
                disagreements.Add(disagreement);
            }
        }

        return disagreements;
    }

    private ConsensusResult ParseTextSynthesisResponse(string synthesisResponse, List<ModelResponse> originalResponses, string originalPrompt, string runId)
    {
        // Extract summary from XML tags
        string summary = "No summary provided by model";
        var summaryMatch = Regex.Match(synthesisResponse, @"<summary>(.+?)</summary>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (summaryMatch.Success)
        {
            summary = summaryMatch.Groups[1].Value.Trim();
        }

        // Extract structured sections from the synthesis response
        var result = new ConsensusResult
        {
            SynthesizedAnswer = ExtractSection(synthesisResponse, "SYNTHESIZED ANSWER"),
            SynthesisReasoning = ExtractSection(synthesisResponse, "REASONING"),
            Summary = summary,
            OverallConfidence = ParseConfidence(ExtractSection(synthesisResponse, "CONFIDENCE")),
            ConsensusLevel = ParseConsensusLevel(ExtractSection(synthesisResponse, "CONSENSUS LEVEL")),
            IndividualResponses = originalResponses,
            AgreementPoints = ParseAgreementPoints(ExtractSection(synthesisResponse, "AGREEMENT POINTS")),
            Disagreements = ParseDisagreements(ExtractSection(synthesisResponse, "DISAGREEMENTS")),
            OriginalPrompt = originalPrompt
        };

        // If parsing failed, use the full response as the answer
        if (string.IsNullOrWhiteSpace(result.SynthesizedAnswer))
        {
            result.SynthesizedAnswer = synthesisResponse;
            _runTracker.WriteLog(runId, "Could not parse structured synthesis response, using full response");
        }

        return result;
    }

    private string ExtractSection(string response, string sectionName)
    {
        // Try format 1: "SECTION_NAME:" followed by content until next section or end
        var pattern = $@"{Regex.Escape(sectionName)}:\s*(.+?)(?=\n[A-Z\s]+:|$)";
        var match = Regex.Match(response, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Try format 2: Markdown header "## SECTION_NAME" or "# SECTION_NAME"
        var markdownPattern = $@"^#{1,3}\s+{Regex.Escape(sectionName)}\s*$\n(.+?)(?=^#{1,3}\s+|\Z)";
        var markdownMatch = Regex.Match(response, markdownPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        if (markdownMatch.Success)
        {
            return markdownMatch.Groups[1].Value.Trim();
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

        // Check if explicitly marked as "None"
        if (disagreementText.Trim().Equals("None", StringComparison.OrdinalIgnoreCase))
            return disagreements;

        // Parse structured format:
        // - TOPIC: [topic description]
        //   MODEL: [model name] - [their position]
        //   MODEL: [model name] - [their position]
        var lines = disagreementText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Disagreement? currentDisagreement = null;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check for topic line (starts with - TOPIC:)
            if (trimmedLine.StartsWith("- TOPIC:", StringComparison.OrdinalIgnoreCase))
            {
                // Save previous disagreement if exists
                if (currentDisagreement != null && currentDisagreement.Views.Any())
                {
                    disagreements.Add(currentDisagreement);
                }

                // Start new disagreement
                var topic = trimmedLine.Substring("- TOPIC:".Length).Trim();
                currentDisagreement = new Disagreement
                {
                    Topic = topic,
                    Views = new List<DissentingView>()
                };
            }
            // Check for model view line (starts with MODEL:)
            else if (trimmedLine.StartsWith("MODEL:", StringComparison.OrdinalIgnoreCase) && currentDisagreement != null)
            {
                var viewText = trimmedLine.Substring("MODEL:".Length).Trim();

                // Split on " - " to separate model name from position
                var parts = viewText.Split(new[] { " - " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    currentDisagreement.Views.Add(new DissentingView
                    {
                        ModelName = parts[0].Trim(),
                        Position = parts[1].Trim()
                    });
                }
            }
        }

        // Add the last disagreement if exists
        if (currentDisagreement != null && currentDisagreement.Views.Any())
        {
            disagreements.Add(currentDisagreement);
        }

        return disagreements;
    }

}
