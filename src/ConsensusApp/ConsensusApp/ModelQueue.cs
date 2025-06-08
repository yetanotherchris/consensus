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
        Func<string, string, string, Task<string>> summarizeChanges)
    {
        var model = _models.Dequeue();

        string previousAnswer = answer;
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
                var revised = ResponseParser.GetRevisedAnswer(answer);
                messages = new()
                {
                    ChatMessage.CreateSystemMessage(string.Format(Prompts.FollowupSystemPrompt, previousModel)),
                    ChatMessage.CreateUserMessage(revised)
                };
            }

            answer = await _client.QueryAsync(model, messages);
        });

        string changeSummary;
        string? initialSummary = null;
        if (previousModel == string.Empty)
        {
            initialSummary = ResponseParser.GetInitialResponseSummary(answer);
            changeSummary = ResponseParser.GetChangesSummary(answer);
            if (string.IsNullOrEmpty(changeSummary))
            {
                changeSummary = "No changes as it's the first response.";
            }
        }
        else
        {
            changeSummary = await summarizeChanges(model, answer, previousAnswer);
        }

        if (logBuilder is not null)
        {
            logBuilder.AppendLine($"# {model}");
            if (logLevel == LogLevel.Full)
            {
                logBuilder.AppendLine(answer);
                logBuilder.AppendLine();
                logBuilder.AppendLine("-----------");
                logBuilder.AppendLine();
            }

            if (!string.IsNullOrEmpty(initialSummary))
            {
                logBuilder.AppendLine(initialSummary.Trim());
                logBuilder.AppendLine();
            }

            logBuilder.AppendLine(changeSummary.Trim());
            logBuilder.AppendLine();
        }

        return new ModelResult(model, answer, changeSummary.Trim(), initialSummary?.Trim());
    }

}

internal sealed record ModelResult(string Model, string Answer, string ChangeSummary, string? InitialSummary);
