using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Service for building prompts for the parallel-then-synthesize consensus pattern
/// </summary>
public interface IPromptBuilder
{
    /// <summary>
    /// Build an enhanced prompt for the divergent phase requesting reasoning, confidence, etc.
    /// </summary>
    string BuildEnhancedDivergentPrompt(ConsensusRequest request);
    
    /// <summary>
    /// Build the judge/synthesizer prompt for the convergent phase
    /// </summary>
    string BuildJudgeSynthesisPrompt(string originalPrompt, List<ModelResponse> responses);
}
