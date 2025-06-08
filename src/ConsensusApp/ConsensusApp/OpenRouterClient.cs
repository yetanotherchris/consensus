using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace ConsensusApp;

internal sealed class OpenRouterClient
{
    private readonly IChatClient _client;

    public OpenRouterClient(string apiKey) : this(new OpenAIChatClient(apiKey))
    {
    }

    internal OpenRouterClient(IChatClient client)
    {
        _client = client;
    }

    public Task<string> QueryAsync(string model, IEnumerable<ChatMessage> messages)
        => _client.CompleteChatAsync(model, messages);
}
