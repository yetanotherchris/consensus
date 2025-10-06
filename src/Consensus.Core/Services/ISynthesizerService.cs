using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Interface for the judge/synthesizer service that produces the final consensus
/// </summary>
public interface ISynthesizerService
{
    /// <summary>
    /// Synthesize all model responses into a final consensus result
    /// </summary>
    /// <param name="originalPrompt">The original user prompt</param>
    /// <param name="responses">All model responses collected</param>
    /// <param name="judgeModel">The model to use as the judge/synthesizer</param>
    /// <param name="runId">The run identifier for tracking</param>
    /// <param name="cancellationToken">Cancellation token for timeout handling</param>
    /// <returns>A ConsensusResult with synthesis, reasoning, and analysis</returns>
    Task<ConsensusResult> SynthesizeAsync(string originalPrompt, List<ModelResponse> responses, string judgeModel, string runId, CancellationToken cancellationToken = default);
}
