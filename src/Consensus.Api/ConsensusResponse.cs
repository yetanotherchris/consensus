namespace Consensus.Api;

public sealed record ConsensusResponse(string Path, string Answer, string ChangesSummary, string? LogPath);
