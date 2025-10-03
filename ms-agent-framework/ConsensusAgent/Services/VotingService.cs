using ConsensusAgent.Configuration;
using ConsensusAgent.Logging;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for conducting votes on model responses
/// </summary>
public class VotingService : IVotingService
{
    private readonly IAgentService _agentService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ConsensusConfiguration _config;
    private readonly SimpleLogger _logger;

    public VotingService(
        IAgentService agentService,
        IPromptBuilder promptBuilder,
        ConsensusConfiguration config,
        SimpleLogger logger)
    {
        _agentService = agentService;
        _promptBuilder = promptBuilder;
        _config = config;
        _logger = logger;
    }

    public async Task<string> ConductVoteAsync(List<(string model, string response)> responses, string votingModel, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Conducting vote with {0} responses using model {1}", responses.Count, votingModel);
        
        string votePrompt = _promptBuilder.BuildVotingPrompt(responses);
        
        string voteResult = await _agentService.QueryModelOneOffAsync(
            votingModel,
            votePrompt,
            _config.ApiEndpoint,
            _config.ApiKey,
            cancellationToken
        );
        
        _logger.LogInformation("Vote completed successfully");
        
        return voteResult;
    }
}
