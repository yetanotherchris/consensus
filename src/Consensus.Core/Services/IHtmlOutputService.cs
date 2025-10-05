using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Service for handling HTML output operations
/// </summary>
public interface IHtmlOutputService
{
    /// <summary>
    /// Save the consensus result in HTML format
    /// </summary>
    /// <param name="result">The consensus result to save</param>
    /// <param name="id">Optional identifier for the output file</param>
    Task SaveConsensusResultAsync(ConsensusResult result, string? id = null);
}
