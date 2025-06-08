using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using OpenAI.Chat;

namespace ConsensusApp;

internal sealed class ModelQueue
{
    private readonly Queue<string> _models;
    private readonly OpenRouterClient _client;
    private readonly IConsoleService _console;
    private static readonly HtmlParser Parser = new();

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
        });

        string changeSummary;
        if (previousModel == string.Empty)
        {
            changeSummary = ExtractInitialResponseSummary(answer);
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

        return new ModelResult(model, answer, changeSummary.Trim());
    }

    private static string ExtractRevisedAnswer(string answer)
    {
        var document = Parser.ParseDocument(answer);
        var element = document.QuerySelector("RevisedAnswer") ?? document.QuerySelector("InitialResponse");
        return element?.TextContent.Trim() ?? answer;
    }

    private static string ExtractInitialResponseSummary(string answer)
    {
        var document = Parser.ParseDocument(answer);
        return document.QuerySelector("InitialResponseSummary")?.TextContent.Trim() ?? string.Empty;
    }

    private static string ExtractInitialResponse(string answer)
    {
        var document = Parser.ParseDocument(answer);
        var element = document.QuerySelector("InitialResponse");
        return element?.TextContent.Trim() ?? answer;
    }
}

internal sealed record ModelResult(string Model, string Answer, string ChangeSummary);
