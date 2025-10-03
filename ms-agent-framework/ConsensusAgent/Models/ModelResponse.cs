namespace ConsensusAgent.Models;

/// <summary>
/// Response from an individual model including metadata
/// </summary>
public class ModelResponse
{
    /// <summary>
    /// Name/identifier of the model that generated this response
    /// </summary>
    public required string ModelName { get; set; }
    
    /// <summary>
    /// The main answer provided by the model
    /// </summary>
    public required string Answer { get; set; }
    
    /// <summary>
    /// The reasoning or chain-of-thought process used by the model
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence score from 0.0 to 1.0
    /// </summary>
    public double ConfidenceScore { get; set; }
    
    /// <summary>
    /// Theoretical framework referenced (e.g., for Psychology domain)
    /// Currently not parsed from responses - reserved for future enhancement
    /// </summary>
    public string? TheoreticalFramework { get; set; }
    
    /// <summary>
    /// Citations or references provided
    /// Currently not parsed from responses - reserved for future enhancement
    /// </summary>
    public List<string> Citations { get; set; } = new();
    
    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Number of tokens used in the response
    /// Currently not tracked - reserved for future cost analysis
    /// </summary>
    public int TokensUsed { get; set; }
}
