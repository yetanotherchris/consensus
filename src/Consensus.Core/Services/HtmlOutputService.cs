using Consensus.Logging;
using Consensus.Models;
using Markdig;
using TextTemplate;

namespace Consensus.Services;

/// <summary>
/// Service for handling HTML output operations
/// </summary>
public class HtmlOutputService : IHtmlOutputService
{
    private readonly SimpleFileLogger _logger;
    private readonly IOutputWriter _fileWriter;
    private readonly string _htmlTemplatePath;
    private readonly MarkdownPipeline _markdownPipeline;

    public HtmlOutputService(SimpleFileLogger logger, IOutputWriter fileWriter)
    {
        _logger = logger;
        _fileWriter = fileWriter;
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
        
        // Template is copied to output directory by the build process
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _htmlTemplatePath = Path.Combine(baseDirectory, "Templates", "ConsensusOutput.html.tmpl");
    }

    public async Task SaveConsensusResultAsync(ConsensusResult result, string? id = null)
    {
        _logger.LogInformation("Saving consensus result to HTML file");
        
        // Format the result as HTML
        string htmlOutput = FormatConsensusResult(result);
        
        // Delegate file writing to the file writer service
        await _fileWriter.WriteHtmlAsync(htmlOutput, id);
        
        _logger.LogInformation("HTML consensus result saved successfully");
    }

    /// <summary>
    /// Preprocesses markdown to fix common issues like bullet points
    /// </summary>
    private string PreprocessMarkdown(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return markdown;
        
        // Replace bullet character (•) with proper markdown list syntax
        // Handle lines that start with "• " and convert them to "- "
        var lines = markdown.Split('\n');
        var processedLines = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith("• "))
            {
                // Replace bullet with markdown list marker, preserving indentation
                var indentation = line.Substring(0, line.Length - trimmedLine.Length);
                processedLines.Add(indentation + "- " + trimmedLine.Substring(2));
            }
            else
            {
                processedLines.Add(line);
            }
        }
        
        return string.Join('\n', processedLines);
    }

    private string FormatConsensusResult(ConsensusResult result)
    {
        var templateContent = File.ReadAllText(_htmlTemplatePath);
        var template = Template.New("htmloutput").Parse(templateContent);
        
        // Convert Markdown to HTML
        var originalPromptHtml = Markdown.ToHtml(PreprocessMarkdown(result.OriginalPrompt), _markdownPipeline);
        var synthesizedAnswerHtml = Markdown.ToHtml(PreprocessMarkdown(result.SynthesizedAnswer), _markdownPipeline);
        var synthesisReasoningHtml = !string.IsNullOrEmpty(result.SynthesisReasoning) 
            ? Markdown.ToHtml(PreprocessMarkdown(result.SynthesisReasoning), _markdownPipeline) 
            : string.Empty;
        
        // Prepare data for template matching the actual ConsensusResult structure
        var individualResponsesData = result.IndividualResponses.Select(r => new
        {
            r.ModelName,
            r.Summary,
            AnswerHtml = Markdown.ToHtml(PreprocessMarkdown(r.Answer), _markdownPipeline),
            ReasoningHtml = !string.IsNullOrEmpty(r.Reasoning) 
                ? Markdown.ToHtml(PreprocessMarkdown(r.Reasoning), _markdownPipeline) 
                : string.Empty,
            ConfidenceDisplay = $"{r.ConfidenceScore * 100:F0}"
        }).ToList();
        
        var disagreementsData = result.Disagreements.Select(d => new
        {
            d.Topic,
            d.Views,
            HasViews = d.Views.Any()
        }).ToList();
        
        // Convert agreement points to just the Point string
        var agreementPointsData = result.AgreementPoints.Select(p => p.Point).ToList();
        
        var data = new
        {
            Title = "Consensus Analysis Report",
            GeneratedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ModelsCount = result.IndividualResponses.Count,
            ProcessingTime = $"{result.TotalProcessingTime.TotalSeconds:F2}s",
            ConsensusLevel = result.ConsensusLevel.ToString(),
            OverallConfidence = $"{result.OverallConfidence * 100:F0}",
            OriginalPromptHtml = originalPromptHtml,
            SynthesizedAnswerHtml = synthesizedAnswerHtml,
            SynthesisReasoningHtml = synthesisReasoningHtml,
            AgreementPoints = agreementPointsData,
            HasAgreementPoints = result.AgreementPoints.Any(),
            Disagreements = disagreementsData,
            HasDisagreements = result.Disagreements.Any(),
            IndividualResponses = individualResponsesData
        };
        
        return template.Execute(data);
    }
}
