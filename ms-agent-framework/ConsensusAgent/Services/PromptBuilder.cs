using System.Text;
using ConsensusAgent.Models;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for building prompts for the parallel-then-synthesize consensus pattern
/// </summary>
public class PromptBuilder : IPromptBuilder
{
    /// <summary>
    /// Builds an enhanced prompt for the divergent phase that requests reasoning, confidence, etc.
    /// </summary>
    public string BuildEnhancedDivergentPrompt(ConsensusRequest request)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Original Question:");
        promptBuilder.AppendLine(request.Prompt);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Please provide:");
        promptBuilder.AppendLine("1. Your answer to the question");
        
        if (request.IncludeReasoning)
        {
            promptBuilder.AppendLine("2. Your reasoning process (step-by-step)");
        }
        
        if (request.IncludeConfidence)
        {
            promptBuilder.AppendLine("3. Your confidence level as a decimal between 0.0 and 1.0");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("IMPORTANT: Include your confidence score in XML tags at the end of your response:");
            promptBuilder.AppendLine("<confidence>0.85</confidence>");
            promptBuilder.AppendLine("Replace 0.85 with your actual confidence (0.0 = no confidence, 1.0 = complete confidence)");
        }
        
        if (request.IncludeTheoreticalFramework && request.Domain.Equals("Psychology", StringComparison.OrdinalIgnoreCase))
        {
            promptBuilder.AppendLine("4. The theoretical framework(s) informing your answer");
        }
        
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Be specific and thorough in your response.");

        return promptBuilder.ToString();
    }

    /// <summary>
    /// Builds the judge/synthesizer prompt for the convergent phase
    /// </summary>
    public string BuildJudgeSynthesisPrompt(string originalPrompt, List<ModelResponse> responses)
    {
        var judgePrompt = new StringBuilder();
        judgePrompt.AppendLine("You are a synthesis judge evaluating multiple AI responses to reach consensus.");
        judgePrompt.AppendLine();
        judgePrompt.AppendLine("Original Question:");
        judgePrompt.AppendLine(originalPrompt);
        judgePrompt.AppendLine();
        judgePrompt.AppendLine($"Responses from {responses.Count} models:");
        judgePrompt.AppendLine();

        for (int i = 0; i < responses.Count; i++)
        {
            var response = responses[i];
            judgePrompt.AppendLine($"[Response {i + 1} from {response.ModelName}]");
            judgePrompt.AppendLine($"Answer: {response.Answer}");
            
            if (!string.IsNullOrEmpty(response.Reasoning))
            {
                judgePrompt.AppendLine($"Reasoning: {response.Reasoning}");
            }
            
            if (response.ConfidenceScore > 0)
            {
                judgePrompt.AppendLine($"Confidence: {response.ConfidenceScore:P0}");
            }
            
            judgePrompt.AppendLine();
        }

        judgePrompt.AppendLine("Your task:");
        judgePrompt.AppendLine("1. Identify points where models agree (consensus points)");
        judgePrompt.AppendLine("2. Identify points of disagreement and analyze why they differ");
        judgePrompt.AppendLine("3. Synthesize the best answer by:");
        judgePrompt.AppendLine("   - Including all consensus points");
        judgePrompt.AppendLine("   - Evaluating conflicting views on merit");
        judgePrompt.AppendLine("   - Combining complementary insights");
        judgePrompt.AppendLine("4. Provide your reasoning for synthesis decisions");
        judgePrompt.AppendLine("5. Assess overall confidence (consider both model confidence and agreement level)");
        judgePrompt.AppendLine("6. Note if disagreements reflect legitimate theoretical differences");
        judgePrompt.AppendLine();
        judgePrompt.AppendLine("Format your response as:");
        judgePrompt.AppendLine("SYNTHESIZED ANSWER: [your synthesis]");
        judgePrompt.AppendLine("REASONING: [your reasoning]");
        judgePrompt.AppendLine("CONFIDENCE: [0-100]");
        judgePrompt.AppendLine("CONSENSUS LEVEL: [Strong/Moderate/Weak/Conflicted]");
        judgePrompt.AppendLine("AGREEMENT POINTS: [bullet list]");
        judgePrompt.AppendLine("DISAGREEMENTS: [description with analysis]");

        return judgePrompt.ToString();
    }
}
