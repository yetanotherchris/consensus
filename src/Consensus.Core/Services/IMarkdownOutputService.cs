using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Service for handling Markdown output operations
/// </summary>
public interface IMarkdownOutputService
{
    /// <summary>
    /// Save the consensus result in Markdown format
    /// </summary>
    /// <param name="result">The consensus result to save</param>
    /// <param name="id">Optional identifier for the output file</param>
    Task SaveConsensusResultAsync(ConsensusResult result, string? id = null);
}
