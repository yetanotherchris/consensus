using Microsoft.Agents.AI;
using ConsensusAgent.Models;

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
    /// Query a model and return a structured ModelResponse with parsed metadata
    /// </summary>
    Task<ModelResponse> QueryModelWithResponseAsync(string model, string prompt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Query a model with a one-off agent (for synthesis/judge operations)
    /// Returns raw string response
    /// </summary>
    Task<string> QueryModelOneOffAsync(string model, string prompt, string apiEndpoint, string apiKey, CancellationToken cancellationToken = default);
}
