using Consensus.Logging;
using Consensus.Models;
using TextTemplate;

namespace Consensus.Services;

/// <summary>
/// Service for handling Markdown output operations
/// </summary>
public class MarkdownOutputService : IMarkdownOutputService
{
    private readonly SimpleLogger _logger;
    private readonly IOutputWriter _fileWriter;
    private readonly string _outputTemplatePath;

    public MarkdownOutputService(SimpleLogger logger, IOutputWriter fileWriter)
    {
        _logger = logger;
        _fileWriter = fileWriter;
        
        // Template is copied to output directory by the build process
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _outputTemplatePath = Path.Combine(baseDirectory, "Templates", "ConsensusOutput.tmpl");
    }

    public async Task SaveConsensusResultAsync(ConsensusResult result, string? id = null)
    {
        _logger.LogInformation("Saving consensus result to Markdown file");
        
        // Format the result as Markdown
        string formattedOutput = FormatConsensusResult(result);
        
        // Delegate file writing to the file writer service
        await _fileWriter.WriteMarkdownAsync(formattedOutput, id);
        
        _logger.LogInformation("Consensus result saved successfully");
    }

    private string FormatConsensusResult(ConsensusResult result)
    {
        var templateContent = File.ReadAllText(_outputTemplatePath);
        var template = Template.New("output").Parse(templateContent);
        
        // Prepare data for template with pre-computed values
        var individualResponsesData = result.IndividualResponses.Select(r => new
        {
            r.ModelName,
            r.Answer,
            r.Reasoning,
            ConfidenceDisplay = $"{r.ConfidenceScore:P0}"
        }).ToList();
        
        var disagreementsData = result.Disagreements.Select(d => new
        {
            d.Topic,
            d.Views,
            HasViews = d.Views.Any()
        }).ToList();
        
        var data = new
        {
            GeneratedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ModelsCount = result.IndividualResponses.Count,
            ProcessingTime = $"{result.TotalProcessingTime.TotalSeconds:F2}s",
            ConsensusLevel = result.ConsensusLevel.ToString(),
            OverallConfidence = $"{result.OverallConfidence:P0}",
            result.SynthesizedAnswer,
            result.SynthesisReasoning,
            result.AgreementPoints,
            HasAgreementPoints = result.AgreementPoints.Any(),
            Disagreements = disagreementsData,
            HasDisagreements = result.Disagreements.Any(),
            IndividualResponses = individualResponsesData
        };
        
        return template.Execute(data);
    }
}
