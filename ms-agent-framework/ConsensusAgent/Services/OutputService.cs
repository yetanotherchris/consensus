using System.Text;
using ConsensusAgent.Logging;
using ConsensusAgent.Models;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for handling output operations (saving files)
/// </summary>
public class OutputService : IOutputService
{
    private readonly SimpleLogger _logger;

    public OutputService(SimpleLogger logger)
    {
        _logger = logger;
    }

    public async Task SaveConsensusResultAsync(ConsensusResult result, string filePath)
    {
        _logger.LogInformation("Saving consensus result to file: {0}", filePath);
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Format the result as Markdown
        string formattedOutput = FormatConsensusResult(result);
        
        await File.WriteAllTextAsync(filePath, formattedOutput);
        
        _logger.LogInformation("Consensus result saved successfully");
    }

    private string FormatConsensusResult(ConsensusResult result)
    {
        var output = new StringBuilder();
        
        // Header
        output.AppendLine("# Consensus Result");
        output.AppendLine();
        output.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine($"**Models Consulted:** {result.IndividualResponses.Count}");
        output.AppendLine($"**Processing Time:** {result.TotalProcessingTime.TotalSeconds:F2}s");
        output.AppendLine($"**Consensus Level:** {result.ConsensusLevel}");
        output.AppendLine($"**Overall Confidence:** {result.OverallConfidence:P0}");
        output.AppendLine();
        output.AppendLine("---");
        output.AppendLine();
        
        // Synthesized Answer
        output.AppendLine("## Synthesized Answer");
        output.AppendLine();
        output.AppendLine(result.SynthesizedAnswer);
        output.AppendLine();
        
        // Synthesis Reasoning
        if (!string.IsNullOrWhiteSpace(result.SynthesisReasoning))
        {
            output.AppendLine("## Synthesis Reasoning");
            output.AppendLine();
            output.AppendLine(result.SynthesisReasoning);
            output.AppendLine();
        }
        
        // Agreement Points
        if (result.AgreementPoints.Any())
        {
            output.AppendLine("## Points of Agreement");
            output.AppendLine();
            foreach (var point in result.AgreementPoints)
            {
                output.AppendLine($"- {point.Point}");
            }
            output.AppendLine();
        }
        
        // Disagreements
        if (result.Disagreements.Any())
        {
            output.AppendLine("## Points of Disagreement");
            output.AppendLine();
            foreach (var disagreement in result.Disagreements)
            {
                output.AppendLine($"### {disagreement.Topic}");
                if (disagreement.Views.Any())
                {
                    foreach (var view in disagreement.Views)
                    {
                        output.AppendLine($"- **{view.ModelName}:** {view.Position}");
                    }
                }
                output.AppendLine();
            }
        }
        
        // Individual Responses
        output.AppendLine("## Individual Model Responses");
        output.AppendLine();
        foreach (var response in result.IndividualResponses)
        {
            output.AppendLine($"### {response.ModelName}");
            output.AppendLine();
            output.AppendLine($"**Confidence:** {response.ConfidenceScore:P0}");
            output.AppendLine();
            output.AppendLine("**Answer:**");
            output.AppendLine(response.Answer);
            output.AppendLine();
            
            if (!string.IsNullOrWhiteSpace(response.Reasoning))
            {
                output.AppendLine("**Reasoning:**");
                output.AppendLine(response.Reasoning);
                output.AppendLine();
            }
        }
        
        return output.ToString();
    }
}
