namespace Consensus.Api.Models;

/// <summary>
/// Request model for starting a consensus job
/// </summary>
public class PromptRequest
{
    /// <summary>
    /// The prompt text to process
    /// </summary>
    public required string Prompt { get; set; }
}
