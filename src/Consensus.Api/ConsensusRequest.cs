using System.Collections.Generic;

namespace Consensus.Api;

/// <summary>
/// Request payload for consensus generation endpoints.
/// </summary>
/// <param name="Prompt">The question or instruction to send to all models.</param>
/// <param name="Models">The list of model names to query in sequence.</param>
/// <param name="Stream">When true, the /consensus/stream endpoint will emit events as they are produced.</param>
/// <param name="ApiKey">Optional OpenRouter API key if not provided via environment variable.</param>
/// <param name="LogLevel">Controls the logging verbosity: "minimal", "full" or none.</param>
public sealed record ConsensusRequest(string Prompt, List<string> Models, bool Stream = false, string? ApiKey = null, string? LogLevel = null);
