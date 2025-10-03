using ConsensusAgent.Logging;
using ConsensusAgent.Models;
using TextTemplate;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for handling output operations (saving files)
/// </summary>
public class OutputService : IOutputService
{
    private readonly SimpleLogger _logger;
    private readonly string _outputTemplatePath;

    public OutputService(SimpleLogger logger)
    {
        _logger = logger;
        
        // Template is copied to output directory by the build process
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _outputTemplatePath = Path.Combine(baseDirectory, "PromptTemplates", "ConsensusOutput.tmpl");
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
