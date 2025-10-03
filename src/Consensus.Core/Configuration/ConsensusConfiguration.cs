namespace Consensus.Configuration;

/// <summary>
/// Configuration settings for the consensus building process
/// </summary>
public class ConsensusConfiguration
{
    // File and API configuration
    public required string PromptFile { get; init; }
    public required string ModelsFile { get; init; }
    public required string ApiEndpoint { get; init; }
    public required string ApiKey { get; init; }
    public required string[] Models { get; init; }
    
    // Domain configuration
    /// <summary>
    /// The domain of questions (e.g., "Psychology", "General")
    /// Currently not used - reserved for future domain-specific enhancements
    /// </summary>
    public string Domain { get; init; } = "General";
    
    // Timeout and threshold configuration
    /// <summary>
    /// Timeout in seconds for each individual agent query
    /// </summary>
    public int AgentTimeoutSeconds { get; init; } = 120;
    
    /// <summary>
    /// Minimum number of agents that must successfully respond to proceed
    /// </summary>
    public int MinimumAgentsRequired { get; init; } = 3;
    
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
    
    public string OutputDirectory { get; init; } = ".";
    public string Timestamp { get; init; } = DateTime.Now.ToString("yyyyMMddHHmmss");
    
    public string LogFile => Path.Combine(OutputDirectory, "output", "logs", $"conversation-log-{Timestamp}.txt");
    public string ConsensusFile => Path.Combine(OutputDirectory, "output", "responses", $"consensus-{Timestamp}.md");
}
