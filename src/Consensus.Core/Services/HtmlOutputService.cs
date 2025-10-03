using Consensus.Logging;
using Consensus.Models;
using TextTemplate;

namespace Consensus.Services;

/// <summary>
/// Service for handling HTML output operations
/// </summary>
public class HtmlOutputService : IHtmlOutputService
{
    private readonly SimpleLogger _logger;
    private readonly string _htmlTemplatePath;

    public HtmlOutputService(SimpleLogger logger)
    {
        _logger = logger;
        
        // Template is copied to output directory by the build process
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _htmlTemplatePath = Path.Combine(baseDirectory, "Templates", "ConsensusOutput.html.tmpl");
    }

    public async Task SaveConsensusResultAsync(ConsensusResult result, string filePath)
    {
        _logger.LogInformation("Saving consensus result to HTML file: {0}", filePath);
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Format the result as HTML
        string htmlOutput = FormatConsensusResult(result);
        
        await File.WriteAllTextAsync(filePath, htmlOutput);
        
        _logger.LogInformation("HTML consensus result saved successfully");
    }

    private string FormatConsensusResult(ConsensusResult result)
    {
        var templateContent = File.ReadAllText(_htmlTemplatePath);
        var template = Template.New("htmloutput").Parse(templateContent);
        
        // Prepare data for template matching the actual ConsensusResult structure
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
            Title = "Consensus Analysis Report",
            GeneratedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ModelsCount = result.IndividualResponses.Count,
            ProcessingTime = $"{result.TotalProcessingTime.TotalSeconds:F2}s",
            ConsensusLevel = result.ConsensusLevel.ToString(),
            OverallConfidence = $"{result.OverallConfidence:P0}",
            SynthesizedAnswer = result.SynthesizedAnswer,
            SynthesisReasoning = result.SynthesisReasoning,
            AgreementPoints = result.AgreementPoints,
            HasAgreementPoints = result.AgreementPoints.Any(),
            Disagreements = disagreementsData,
            HasDisagreements = result.Disagreements.Any(),
            IndividualResponses = individualResponsesData
        };
        
        return template.Execute(data);
    }
}
