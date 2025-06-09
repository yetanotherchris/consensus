using System.Collections.Generic;

namespace Consensus.Api;

public sealed record ConsensusRequest(string Prompt, List<string> Models, bool Stream = false, string? ApiKey = null, string? LogLevel = null);
