using Microsoft.Agents.AI;

namespace ConsensusAgent.Services;

/// <summary>
/// Service for managing AI agents and executing queries
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Initialize agent contexts for all models
    /// </summary>
    Task InitializeAgentsAsync(string[] models, string apiEndpoint, string apiKey);
    
    /// <summary>
    /// Query a model using its pre-initialized agent and thread
    /// </summary>
    Task<string> QueryModelAsync(string model, string prompt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Query a model with a one-off agent (for voting and final consensus)
    /// </summary>
    Task<string> QueryModelOneOffAsync(string model, string prompt, string apiEndpoint, string apiKey, CancellationToken cancellationToken = default);
}
