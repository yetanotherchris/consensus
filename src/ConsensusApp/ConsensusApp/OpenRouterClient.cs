using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace ConsensusApp;

internal sealed class OpenRouterClient
{
    private readonly string _apiKey;
    private readonly OpenAIClientOptions _options = new() { Endpoint = new Uri("https://openrouter.ai/api/v1") };

    public OpenRouterClient(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> QueryAsync(string model, IEnumerable<ChatMessage> messages)
    {
        var credential = new ApiKeyCredential(_apiKey);
        var client = new ChatClient(model, credential, _options);
        var completion = await client.CompleteChatAsync(messages);
        return string.Join("\n", completion.Value.Content.Select(p => p.Text));
    }
}
