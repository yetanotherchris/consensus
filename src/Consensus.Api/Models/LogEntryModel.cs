namespace Consensus.Api.Models;

/// <summary>
/// Represents a single log entry
/// </summary>
public class LogEntryModel
{
    /// <summary>
    /// The timestamp of the log entry
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// The log message
    /// </summary>
    public required string Message { get; set; }
}
