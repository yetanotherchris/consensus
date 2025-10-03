namespace ConsensusAgent.Services;

/// <summary>
/// Service for conducting votes on model responses
/// </summary>
public interface IVotingService
{
    /// <summary>
    /// Conduct a vote to analyze and compare responses from different models
    /// </summary>
    Task<string> ConductVoteAsync(List<(string model, string response)> responses, string votingModel, CancellationToken cancellationToken = default);
}
