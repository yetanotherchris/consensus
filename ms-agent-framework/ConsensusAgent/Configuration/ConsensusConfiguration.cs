namespace ConsensusAgent.Configuration;

/// <summary>
/// Configuration settings for the consensus building process
/// </summary>
public class ConsensusConfiguration
{
    public required string PromptFile { get; init; }
    public required string ModelsFile { get; init; }
    public required string ApiEndpoint { get; init; }
    public required string ApiKey { get; init; }
    public required string[] Models { get; init; }
    public int MaxRounds { get; init; } = 5;
    public int QueryTimeoutSeconds { get; init; } = 90;
    public string OutputDirectory { get; init; } = ".";
    public string Timestamp { get; init; } = DateTime.Now.ToString("yyyyMMdd-HHmmss");
    
    public string LogFile => Path.Combine(OutputDirectory, $"conversation-log-{Timestamp}.txt");
    public string ConsensusFile => Path.Combine(OutputDirectory, $"consensus-{Timestamp}.md");
}
