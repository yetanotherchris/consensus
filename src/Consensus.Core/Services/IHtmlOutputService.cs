using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Service for handling HTML output operations
/// </summary>
public interface IHtmlOutputService
{
    /// <summary>
    /// Save the consensus result to a file in HTML format
    /// </summary>
    Task SaveConsensusResultAsync(ConsensusResult result, string filePath);
}
