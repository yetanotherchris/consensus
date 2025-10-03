namespace ConsensusAgent.Models;

/// <summary>
/// Request object for initiating a consensus building process
/// </summary>
public class ConsensusRequest
{
    /// <summary>
    /// The prompt/question to be answered by consensus
    /// </summary>
    public required string Prompt { get; set; }
    
    /// <summary>
    /// The domain of the question (e.g., "Psychology", "General")
    /// Currently not used - reserved for future domain-specific enhancements
    /// </summary>
    public string Domain { get; set; } = "General";
    
    /// <summary>
    /// Whether to request reasoning/chain-of-thought from models
    /// </summary>
    public bool IncludeReasoning { get; set; } = true;
    
    /// <summary>
    /// Whether to request confidence scores from models
    /// </summary>
    public bool IncludeConfidence { get; set; } = true;
    
    /// <summary>
    /// Whether to request theoretical framework information (domain-specific)
    /// Currently not used - reserved for Psychology and other academic domains
    /// </summary>
    public bool IncludeTheoreticalFramework { get; set; } = false;
    
    /// <summary>
    /// Minimum number of agents that must successfully respond
    /// </summary>
    public int MinimumAgents { get; set; } = 3;
}
