using Consensus.Models;

namespace Consensus.Services;

/// <summary>
/// Service for persisting and loading intermediate model responses during consensus runs
/// </summary>
public interface IIntermediateResponsePersistence
{
    /// <summary>
    /// Save individual model responses to disk for a given run
    /// </summary>
    /// <param name="runId">The run identifier</param>
    /// <param name="responses">List of model responses to save</param>
    Task SaveResponsesAsync(string runId, List<ModelResponse> responses);

    /// <summary>
    /// Load individual model responses from disk for a given run
    /// </summary>
    /// <param name="runId">The run identifier</param>
    /// <returns>List of model responses, or empty list if not found</returns>
    Task<List<ModelResponse>> LoadResponsesAsync(string runId);

    /// <summary>
    /// Check if responses exist on disk for a given run
    /// </summary>
    /// <param name="runId">The run identifier</param>
    /// <returns>True if responses exist, false otherwise</returns>
    Task<bool> ResponsesExistAsync(string runId);
}
