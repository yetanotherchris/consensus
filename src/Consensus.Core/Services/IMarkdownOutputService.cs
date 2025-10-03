using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Service for handling Markdown output operations
/// </summary>
public interface IMarkdownOutputService
{
    /// <summary>
    /// Save the consensus result to a file in Markdown format
    /// </summary>
    Task SaveConsensusResultAsync(ConsensusResult result, string filePath);
}
