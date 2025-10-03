namespace ConsensusAgent.Services;

/// <summary>
/// Service for building prompts for different consensus scenarios
/// </summary>
public interface IPromptBuilder
{
    /// <summary>
    /// Build a prompt for the next round based on previous responses and vote
    /// </summary>
    string BuildNextRoundPrompt(string originalPrompt, List<(string model, string response)> previousResponses, string voteResult, int currentRound);
    
    /// <summary>
    /// Build a voting prompt to analyze and compare responses
    /// </summary>
    string BuildVotingPrompt(List<(string model, string response)> responses);
    
    /// <summary>
    /// Build the final consensus generation prompt
    /// </summary>
    string BuildFinalConsensusPrompt(string originalPrompt, List<string> conversationHistory, int maxRounds);
}
