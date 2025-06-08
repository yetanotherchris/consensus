using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace ConsensusApp;

internal sealed class ModelQueue
{
    private readonly Queue<string> _models;
    private readonly OpenRouterClient _client;
    private readonly IConsoleService _console;
    private static readonly string InitialSystemPrompt =
        ResourceHelper.GetString("ConsensusApp.Resources.InitialSystemPrompt.txt");
    private static readonly string FollowupSystemPrompt =
        ResourceHelper.GetString("ConsensusApp.Resources.FollowupSystemPrompt.txt");

    public ModelQueue(IEnumerable<string> models, OpenRouterClient client, IConsoleService console)
    {
        _models = new Queue<string>(models);
        _client = client;
        _console = console;
    }

    public bool HasNext => _models.Count > 0;

    public async Task<ModelResult> PopAsync(
        string prompt,
        string answer,
        string previousModel,
        LogLevel logLevel,
        StringBuilder? logBuilder,
        Func<string, string, Task<string>> summarizeChanges)
    {
        var model = _models.Dequeue();

        string summaryForConsensus = string.Empty;
        await _console.StatusAsync("Querying {0}", model, async () =>
        {
            List<ChatMessage> messages;
            if (answer == prompt)
            {
                messages = new()
                {
                    ChatMessage.CreateSystemMessage(InitialSystemPrompt),
                    ChatMessage.CreateUserMessage(prompt)
                };
            }
            else
            {
                messages = new()
                {
                    ChatMessage.CreateSystemMessage(string.Format(FollowupSystemPrompt, previousModel)),
                    ChatMessage.CreateUserMessage(answer)
                };
            }

            answer = await _client.QueryAsync(model, messages);
            summaryForConsensus = ExtractConsensusSummary(answer);

            if (logBuilder is not null)
            {
                if (logLevel == LogLevel.Full)
                {
                    logBuilder.AppendLine($"### {model}");
                    logBuilder.AppendLine(answer);
                    logBuilder.AppendLine();
                    logBuilder.AppendLine("-----------");
                    logBuilder.AppendLine();
                }
                else if (logLevel == LogLevel.Minimal)
                {
                    string summary;
                    if (previousModel == string.Empty)
                    {
                        summary = "Initial answer generated.";
                    }
                    else
                    {
                        summary = await summarizeChanges(model, answer);
                    }

                    logBuilder.AppendLine($"### {model}");
                    logBuilder.AppendLine(summary.Trim());
                    logBuilder.AppendLine();
                }
            }
        });

        return new ModelResult(model, answer, summaryForConsensus);
    }

    private static string ExtractConsensusSummary(string answer)
    {
        var marker = "Summary for Consensus";
        var index = answer.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index == -1)
        {
            return string.Empty;
        }

        var summary = answer[(index + marker.Length)..].TrimStart(':', ' ', '\n', '\r');
        return summary.Trim();
    }
}

internal sealed record ModelResult(string Model, string Answer, string SummaryForConsensus);
