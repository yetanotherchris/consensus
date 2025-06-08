using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using OpenAI.Chat;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsensusApp;

internal sealed class ConsensusProcessor
{
    private readonly OpenRouterClient _client;
    private readonly IConsoleService _console;
    private readonly ILogger<ConsensusProcessor> _logger;

    public ConsensusProcessor(OpenRouterClient client, IConsoleService console, ILogger<ConsensusProcessor> logger)
    {
        _client = client;
        _console = console;
        _logger = logger;
    }

    public async Task<ConsensusResult> RunAsync(string prompt, IReadOnlyList<string> models, LogLevel logLevel)
    {
        string answer = prompt;
        string previousModel = string.Empty;
        var logBuilder = logLevel == LogLevel.None ? null : new System.Text.StringBuilder();
        string logPath = string.Empty;
        var results = new List<ModelResult>();

        var queue = new ModelQueue(models, _client, _console);
        while (queue.HasNext)
        {
            bool firstModel = string.IsNullOrEmpty(previousModel);

            var result = await queue.PopAsync(
                prompt,
                answer,
                previousModel,
                logLevel,
                logBuilder,
                SummarizeChangesAsync);

            previousModel = result.Model;
            answer = result.Answer;
            results.Add(result);

            if (firstModel)
            {
                _console.MarkupLine("Initial answer generated.");
                _logger.LogInformation("\n[bold]{Model} answer summary:[/]\n- {Summary}\n", result.Model, result.ChangeSummary);
            }
            else
            {
                _logger.LogInformation("\n[bold]{Model} change summary:[/]\n- {Summary}\n", result.Model, result.ChangeSummary);
            }
        }

        var uniqueFiles = Environment.GetEnvironmentVariable("CONSENSUS_UNIQUE_FILENAMES") is not null;
        var baseName = uniqueFiles
            ? DateTime.Now.ToString("yyyyMMddHHmmss")
            : SanitizeFileName(prompt).ToLowerInvariant();

        if (logBuilder is not null)
        {
            if (results.Count > 0)
            {
                logBuilder.AppendLine("## Change Summaries");
                foreach (var r in results)
                {
                    logBuilder.AppendLine($"### {r.Model}");
                    logBuilder.AppendLine(r.ChangeSummary);
                    logBuilder.AppendLine();
                }
            }

            logPath = Path.Combine(Directory.GetCurrentDirectory(), $"log_{baseName}.md");
            await File.WriteAllTextAsync(logPath, logBuilder.ToString());
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), $"answer_{baseName}.md");
        var finalAnswer = ExtractRevisedAnswer(answer);
        await File.WriteAllTextAsync(path, finalAnswer);

        var summary = await GenerateFinalChangesSummaryAsync(previousModel, results);
        return new(path, summary, logPath == string.Empty ? null : logPath);
    }

    private async Task<string> SummarizeChangesAsync(string model, string answer)
    {
        string summary = string.Empty;
        await _console.StatusAsync("Summarizing response from {0}", model, async () =>
            {
                summary = await _client.QueryAsync(model, new ChatMessage[]
                {
                    ChatMessage.CreateSystemMessage(Prompts.ChangeSummarySystemPrompt),
                    ChatMessage.CreateUserMessage(answer)
                });
            });

        return summary.Split('\n').FirstOrDefault() ?? string.Empty;
    }

    private async Task<string> GenerateFinalChangesSummaryAsync(string model, IEnumerable<ModelResult> results)
    {
        string combined = string.Join(
            "\n------\n",
            results.Select(r => $"Model: {r.Model}\n{r.ChangeSummary}"));
        string summary = string.Empty;
        await _console.StatusAsync("Generating final summary with {0}", model, async () =>
            {
                summary = await _client.QueryAsync(model, new ChatMessage[]
                {
                    ChatMessage.CreateSystemMessage(Prompts.FinalChangesSummaryPrompt),
                    ChatMessage.CreateUserMessage(combined)
                });
            });

        var parser = new AngleSharp.Html.Parser.HtmlParser();
        var doc = parser.ParseDocument(summary);
        return doc.QuerySelector("ChangesSummary")?.TextContent.Trim() ?? summary.Trim();
    }

    private static string SanitizeFileName(string text)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(text
            .Take(10)
            .Select(c => char.IsWhiteSpace(c) || invalid.Contains(c) ? '_' : c)
            .ToArray());
    }

    private static string ExtractRevisedAnswer(string answer)
    {
        var parser = new AngleSharp.Html.Parser.HtmlParser();
        var document = parser.ParseDocument(answer);
        var element = document.QuerySelector("RevisedAnswer") ?? document.QuerySelector("InitialResponse");
        return element?.TextContent.Trim() ?? answer;
    }
}

internal sealed record ConsensusResult(string Path, string ChangesSummary, string? LogPath);
