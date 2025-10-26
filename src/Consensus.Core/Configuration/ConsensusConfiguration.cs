namespace Consensus.Configuration;

/// <summary>
/// Configuration settings for the consensus building process
/// </summary>
public class ConsensusConfiguration
{
    // API configuration
    public required string ApiEndpoint { get; init; }
    public required string ApiKey { get; init; }
    
    // Domain configuration
    /// <summary>
    /// The domain of questions (e.g., "Psychology", "General")
    /// Currently not used - reserved for future domain-specific enhancements
    /// </summary>
    public string Domain { get; init; } = "General";

    // Model configuration
    /// <summary>
    /// List of AI models to query for consensus building
    /// </summary>
    public string[] Models { get; init; } = Array.Empty<string>();

    // Timeout and threshold configuration
    /// <summary>
    /// Timeout in seconds for each individual agent query
    /// </summary>
    public int AgentTimeoutSeconds { get; init; } = 120;
    
    // Caching configuration (not yet implemented)
    /// <summary>
    /// Whether to cache responses for identical prompts
    /// Currently not implemented - reserved for future optimization
    /// </summary>
    public bool EnableCaching { get; init; } = false;
    
    /// <summary>
    /// Time-to-live for cached responses in minutes
    /// Currently not implemented - reserved for future optimization
    /// </summary>
    public int CacheTTLMinutes { get; init; } = 60;
    
    // Output configuration
    /// <summary>
    /// Whether to include individual model responses in the output
    /// </summary>
    public bool IncludeIndividualResponses { get; init; } = true;
}
