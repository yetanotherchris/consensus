namespace Consensus.Models;

/// <summary>
/// The final result of the consensus building process
/// </summary>
public class ConsensusResult
{
    /// <summary>
    /// The synthesized answer combining insights from all models
    /// </summary>
    public required string SynthesizedAnswer { get; set; }
    
    /// <summary>
    /// The reasoning behind the synthesis decisions
    /// </summary>
    public string SynthesisReasoning { get; set; } = string.Empty;
    
    /// <summary>
    /// A 2-sentence summary of the synthesized answer
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    /// <summary>
    /// Overall confidence in the synthesized answer (0.0 to 1.0)
    /// </summary>
    public double OverallConfidence { get; set; }
    
    /// <summary>
    /// The level of consensus achieved
    /// </summary>
    public ConsensusLevel ConsensusLevel { get; set; }
    
    /// <summary>
    /// Individual responses from all models
    /// </summary>
    public List<ModelResponse> IndividualResponses { get; set; } = new();
    
    /// <summary>
    /// Points where models agreed
    /// </summary>
    public List<ConsensusPoint> AgreementPoints { get; set; } = new();
    
    /// <summary>
    /// Points where models disagreed
    /// </summary>
    public List<Disagreement> Disagreements { get; set; } = new();
    
    /// <summary>
    /// Total time taken to process the request
    /// </summary>
    public TimeSpan TotalProcessingTime { get; set; }
    
    /// <summary>
    /// The original prompt/question that was submitted
    /// </summary>
    public string OriginalPrompt { get; set; } = string.Empty;
}
