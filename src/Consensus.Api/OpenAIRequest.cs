using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Consensus.Api;

public sealed record OpenAIChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

public sealed record OpenAIChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<OpenAIChatMessage> Messages,
    [property: JsonPropertyName("stream")] bool Stream = false);
