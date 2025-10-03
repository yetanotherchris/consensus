using ConsensusAgent.Models;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for handling output operations (saving files)
/// </summary>
public interface IOutputService
{
    /// <summary>
    /// Save the consensus result to a file in Markdown format
    /// </summary>
    Task SaveConsensusResultAsync(ConsensusResult result, string filePath);
}
