namespace Consensus.Api;

/// <summary>
/// Response returned by consensus generation endpoints.
/// </summary>
/// <param name="Path">Path to the markdown file containing the final answer.</param>
/// <param name="Answer">Combined answer text produced by the models.</param>
/// <param name="ChangesSummary">Summary of which model suggested each change.</param>
/// <param name="LogPath">Optional path to the generated log file.</param>
public sealed record ConsensusResponse(string Path, string Answer, string ChangesSummary, string? LogPath);
