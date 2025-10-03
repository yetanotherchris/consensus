using Consensus.Models;
using TextTemplate;

namespace Consensus.Services;

/// <summary>
/// Service for building prompts for the parallel-then-synthesize consensus pattern
/// </summary>
public class PromptBuilder : IPromptBuilder
{
    private readonly string _divergentTemplatePath;
    private readonly string _synthesisTemplatePath;

    public PromptBuilder()
    {
        // Templates are copied to output directory by the build process
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _divergentTemplatePath = Path.Combine(baseDirectory, "Templates", "Prompt-Divergent.tmpl");
        _synthesisTemplatePath = Path.Combine(baseDirectory, "Templates", "Prompt-Synthesis.tmpl");
        
        // Register custom functions for templates
        RegisterTemplateFunctions();
    }

    private void RegisterTemplateFunctions()
    {
        // Register the 'add' function for incrementing indices
        Template.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b));
        
        // Register the 'mul' function for multiplication (used for percentage calculation)
        Template.RegisterFunction("mul", new Func<double, double, double>((a, b) => a * b));
    }

    /// <summary>
    /// Builds an enhanced prompt for the divergent phase that requests reasoning, confidence, etc.
    /// </summary>
    public string BuildEnhancedDivergentPrompt(ConsensusRequest request)
    {
        var templateContent = File.ReadAllText(_divergentTemplatePath);
        var template = Template.New("divergent").Parse(templateContent);
        
        // Handle the Psychology domain check in C# rather than in template
        var includeTheoreticalFramework = request.IncludeTheoreticalFramework && 
                                          request.Domain.Equals("Psychology", StringComparison.OrdinalIgnoreCase);
        
        var data = new
        {
            request.Prompt,
            request.IncludeReasoning,
            request.IncludeConfidence,
            IncludeTheoreticalFramework = includeTheoreticalFramework
        };
        
        return template.Execute(data);
    }

    /// <summary>
    /// Builds the judge/synthesizer prompt for the convergent phase
    /// </summary>
    public string BuildJudgeSynthesisPrompt(string originalPrompt, List<ModelResponse> responses)
    {
        var templateContent = File.ReadAllText(_synthesisTemplatePath);
        var template = Template.New("synthesis").Parse(templateContent);
        
        // Prepare response data with pre-computed values for template
        var responseData = responses.Select(r => new
        {
            r.ModelName,
            r.Answer,
            r.Reasoning,
            HasConfidence = r.ConfidenceScore > 0,
            ConfidenceDisplay = r.ConfidenceScore > 0 ? $"{r.ConfidenceScore:P0}" : ""
        }).ToList();
        
        var data = new
        {
            OriginalPrompt = originalPrompt,
            ResponseCount = responses.Count,
            Responses = responseData
        };
        
        return template.Execute(data);
    }
}
