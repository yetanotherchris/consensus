using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenAI.Chat;
using Spectre.Console;

namespace ConsensusApp;

internal sealed class ConsensusProcessor
{
    private readonly OpenRouterClient _client;
    private readonly IConsoleService _console;

    public ConsensusProcessor(OpenRouterClient client, IConsoleService console)
    {
        _client = client;
        _console = console;
    }

    public async Task<ConsensusResult> RunAsync(string prompt, IReadOnlyList<string> models, LogLevel logLevel)
    {
        string answer = prompt;
        string previousModel = string.Empty;
        var logBuilder = logLevel == LogLevel.None ? null : new System.Text.StringBuilder();
        string logPath = string.Empty;

        var queue = new ModelQueue(models, _client, _console);
        while (queue.HasNext)
        {
            var result = await queue.PopAsync(
                prompt,
                answer,
                previousModel,
                logLevel,
                logBuilder,
                SummarizeChangesAsync);

            previousModel = result.Model;
            answer = result.Answer;
        }

        if (logBuilder is not null)
        {
            logPath = Path.Combine(Directory.GetCurrentDirectory(), $"log_{DateTime.Now:yyyyMMddHHmmss}.md");
            await File.WriteAllTextAsync(logPath, logBuilder.ToString());
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), $"answer_{DateTime.Now:yyyyMMddHHmmss}.md");
        await File.WriteAllTextAsync(path, answer);

        var summary = await SummarizeAsync(previousModel, answer);
        return new(path, summary, logPath == string.Empty ? null : logPath);
    }

    private async Task<string> SummarizeAsync(string model, string answer)
    {
        string summary = string.Empty;
        await _console.StatusAsync("Summarizing final answer", async () =>
            {
                summary = await _client.QueryAsync(model, new ChatMessage[]
                {
                    ChatMessage.CreateSystemMessage("Summarize the following answer in one short paragraph."),
                    ChatMessage.CreateUserMessage(answer)
                });
            });

        return summary.Split('\n').FirstOrDefault() ?? string.Empty;
    }

    private async Task<string> SummarizeChangesAsync(string model, string answer)
    {
        string summary = string.Empty;
        await _console.StatusAsync("Summarizing response from {0}", model, async () =>
            {
                summary = await _client.QueryAsync(model, new ChatMessage[]
                {
                    ChatMessage.CreateSystemMessage(
                        "Summarize the changes you made compared to the previous answer. " +
                        "List pros and cons as well as agreements and disagreements in one short paragraph."),
                    ChatMessage.CreateUserMessage(answer)
                });
            });

        return summary.Split('\n').FirstOrDefault() ?? string.Empty;
    }
}

internal sealed record ConsensusResult(string Path, string Summary, string? LogPath);
