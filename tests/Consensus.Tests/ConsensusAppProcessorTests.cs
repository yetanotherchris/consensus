using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI.Chat;
using Spectre.Console;
using Xunit;
using Consensus.Core;

namespace Consensus.Tests;

public class ConsensusProcessorTests
{
    [Fact]
    public async Task RunAsync_WritesExpectedFiles()
    {
        var responses = new Queue<string>(new[]
        {
            "<InitialResponse>Answer1</InitialResponse><ChangesSummary>No changes as it's the first response.</ChangesSummary><InitialResponseSummary>Summary1</InitialResponseSummary>",
            "<ChangesSummary>Final summary</ChangesSummary>"
        });

        var client = new OpenRouterClient(new StubChatClient(responses));
        var console = new StubConsoleService();
        var processor = new ConsensusProcessor(client, console, NullLogger<ConsensusProcessor>.Instance);

        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDir);

        try
        {
            var result = await processor.RunAsync("TestPrompt", new[] { "model1" }, LogLevel.Minimal);

            Assert.True(File.Exists(result.Path));
            Assert.NotNull(result.LogPath);
            Assert.True(File.Exists(result.LogPath!));

            var answer = File.ReadAllText(result.Path);
            Assert.Equal("## 📗Final Answer\nAnswer1\n\n## Summary of all the changes made\nFinal summary\n", answer);
            Assert.Equal(answer, result.Answer);

            var log = File.ReadAllText(result.LogPath!);
            var expectedLog = "# model1\nSummary1\n\nNo changes as it's the first response.\n\n### Change Summaries\n#### model1\nNo changes as it's the first response.\n\n";
            Assert.Equal(expectedLog, log);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task RunAsync_TwoModels_UsesPreviousAnswerForSummary()
    {
        var responses = new Queue<string>(new[]
        {
            "<InitialResponse>Answer1</InitialResponse><ChangesSummary>No changes as it's the first response.</ChangesSummary><InitialResponseSummary>Summary1</InitialResponseSummary>",
            "<RevisedAnswer>Answer2</RevisedAnswer>",
            "Bullet summary 2",
            "<ChangesSummary>Final summary</ChangesSummary>"
        });

        var stub = new StubChatClient(responses);
        var client = new OpenRouterClient(stub);
        var console = new StubConsoleService();
        var processor = new ConsensusProcessor(client, console, NullLogger<ConsensusProcessor>.Instance);

        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDir);

        try
        {
            var result = await processor.RunAsync("Prompt", new[] { "model1", "model2" }, LogLevel.Minimal);

            Assert.Equal(4, stub.Requests.Count);
            var summaryCall = stub.Requests[2];
            Assert.Equal(3, summaryCall.Count);
            Assert.Contains("Answer1", summaryCall[1].Content.First().Text);
            Assert.Contains("Answer2", summaryCall[2].Content.First().Text);

            var answer = File.ReadAllText(result.Path);
            Assert.Equal("## 📗Final Answer\nAnswer2\n\n## Summary of all the changes made\nFinal summary\n", answer);
            Assert.Equal(answer, result.Answer);

            var log = File.ReadAllText(result.LogPath!);
            var expectedLog = "# model1\nSummary1\n\nNo changes as it's the first response.\n\n# model2\nBullet summary 2\n\n### Change Summaries\n#### model1\nNo changes as it's the first response.\n\n#### model2\nBullet summary 2\n\n";
            Assert.Equal(expectedLog, log);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task RunAsync_EmptyResponse_IsIgnored()
    {
        var responses = new Queue<string>(new[]
        {
            " ",
            "<InitialResponse>Answer2</InitialResponse><ChangesSummary>No changes as it's the first response.</ChangesSummary><InitialResponseSummary>Summary2</InitialResponseSummary>",
            "<ChangesSummary>Final summary</ChangesSummary>"
        });

        var stub = new StubChatClient(responses);
        var client = new OpenRouterClient(stub);
        var console = new StubConsoleService();
        var processor = new ConsensusProcessor(client, console, NullLogger<ConsensusProcessor>.Instance);

        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDir);

        try
        {
            var result = await processor.RunAsync("Prompt", new[] { "model1", "model2" }, LogLevel.Minimal);

            Assert.Equal(3, stub.Requests.Count);

            var answer = File.ReadAllText(result.Path);
            Assert.Equal("## 📗Final Answer\nAnswer2\n\n## Summary of all the changes made\nFinal summary\n", answer);

            var log = File.ReadAllText(result.LogPath!);
            var expectedLog = "# model2\nSummary2\n\nNo changes as it's the first response.\n\n### Change Summaries\n#### model2\nNo changes as it's the first response.\n\n";
            Assert.Equal(expectedLog, log);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    private sealed class StubConsoleService : IModelQueryService
    {
        public ITemplate Templates { get; } = new StubTemplates();
        public T Ask<T>(string prompt) => throw new NotImplementedException();
        public T Prompt<T>(IPrompt<T> prompt) => throw new NotImplementedException();
        public void MarkupLine(string markup) { }
        public Task StatusAsync(string status, Func<Task> action) => action();
        public Task StatusAsync<T>(string statusFormat, T arg, Func<Task> action) => action();
    }

    private sealed class StubTemplates : ITemplate
    {
        public string QueryingTemplate => "**⏳ Querying {{ Model }}...**";
        public string ModelSummaryTemplate => "{{ ModelSummary }}\n\n---";
        public string AnswerTemplate => "## 📗Final Answer\n{{FinalAnswer}}\n\n## Summary of all the changes made\n{{ChangesSummary}}";
        public string ResponseTemplate => "### {{ Model }}\n\n{{ Answer }}\n\n**📖 Changes:**\n{{ ChangeSummary }}\n\n---";
    }

    private sealed class StubChatClient : IChatClient
    {
        private readonly Queue<string> _responses;
        public List<IReadOnlyList<ChatMessage>> Requests { get; } = new();

        public StubChatClient(Queue<string> responses)
        {
            _responses = responses;
        }

        public Task<string> CompleteChatAsync(string model, IEnumerable<ChatMessage> messages)
        {
            Requests.Add(messages.ToList());
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
