namespace Consensus;

using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;

internal interface IChatClient
{
    Task<string> CompleteChatAsync(string model, IEnumerable<ChatMessage> messages);
}
