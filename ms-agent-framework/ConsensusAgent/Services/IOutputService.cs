namespace ConsensusAgent.Services;

/// <summary>
/// Service for handling output operations (saving files)
/// </summary>
public interface IOutputService
{
    /// <summary>
    /// Save the final consensus to a file
    /// </summary>
    Task SaveConsensusAsync(string consensus, string filePath);
}
