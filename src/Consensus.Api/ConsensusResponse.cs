namespace Consensus.Api;

public sealed record ConsensusResponse(string Path, string ChangesSummary, string? LogPath);
