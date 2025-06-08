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
                    ChatMessage.CreateSystemMessage(Prompts.InitialSystemPrompt),
                    ChatMessage.CreateUserMessage(prompt)
                };
            }
            else
            {
                var revised = ExtractRevisedAnswer(answer);
                messages = new()
                {
                    ChatMessage.CreateSystemMessage(string.Format(Prompts.FollowupSystemPrompt, previousModel)),
                    ChatMessage.CreateUserMessage(revised)
                };
            }

            answer = await _client.QueryAsync(model, messages);
            summaryForConsensus = ExtractConsensusSummary(answer);
        });

        string changeSummary;
        if (previousModel == string.Empty)
        {
            changeSummary = "Initial answer generated.";
        }
        else
        {
            changeSummary = await summarizeChanges(model, answer);
        }

        if (logBuilder is not null)
        {
            logBuilder.AppendLine($"### {model}");
            if (logLevel == LogLevel.Full)
            {
                logBuilder.AppendLine(answer);
                logBuilder.AppendLine();
                logBuilder.AppendLine("-----------");
                logBuilder.AppendLine();
            }

            logBuilder.AppendLine(changeSummary.Trim());
            logBuilder.AppendLine();
        }

        return new ModelResult(model, answer, summaryForConsensus, changeSummary.Trim());
    }

    private static string ExtractConsensusSummary(string answer)
    {
        const string startMarker = "<ConsensusSummary>";
        const string endMarker = "</ConsensusSummary>";

        var start = answer.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (start == -1)
        {
            return string.Empty;
        }

        start += startMarker.Length;
        var end = answer.IndexOf(endMarker, start, StringComparison.OrdinalIgnoreCase);
        if (end == -1)
        {
            end = answer.Length;
        }

        return answer[start..end].Trim();
    }

    private static string ExtractRevisedAnswer(string answer)
    {
        const string startMarker = "<RevisedAnswer>";
        const string endMarker = "</RevisedAnswer>";

        var start = answer.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (start == -1)
        {
            return answer;
        }

        start += startMarker.Length;
        var end = answer.IndexOf(endMarker, start, StringComparison.OrdinalIgnoreCase);
        if (end == -1)
        {
            end = answer.Length;
        }

        return answer[start..end].Trim();
    }
}

internal sealed record ModelResult(string Model, string Answer, string SummaryForConsensus, string ChangeSummary);
