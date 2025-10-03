using ConsensusAgent.Models;

namespace ConsensusAgent.Services;

/// <summary>
/// Interface for consensus agents that can generate responses
/// </summary>
public interface IConsensusAgent
{
    /// <summary>
    /// The name/identifier of the model
    /// </summary>
    string ModelName { get; }
    
    /// <summary>
    /// Generate a response to the given prompt
    /// </summary>
    /// <param name="prompt">The enhanced prompt to respond to</param>
    /// <param name="cancellationToken">Cancellation token for timeout handling</param>
    /// <returns>A ModelResponse containing the answer and metadata</returns>
    Task<ModelResponse> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
}
