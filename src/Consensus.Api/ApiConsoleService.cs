using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Spectre.Console;
using Consensus.Console;
using Consensus;

namespace Consensus.Api;

internal sealed class ApiConsoleService : IConsoleService
{
    public Channel<string> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<string>();

    public T Ask<T>(string prompt) => throw new InvalidOperationException("Prompting not supported in API mode.");

    public T Prompt<T>(IPrompt<T> prompt) => throw new InvalidOperationException("Prompting not supported in API mode.");

    public void MarkupLine(string markup)
        => Channel.Writer.TryWrite(markup + "\n");

    public async Task StatusAsync(string status, Func<Task> action)
        => await StatusAsync<string>(status, default!, action);

    public async Task StatusAsync<T>(string statusFormat, T arg, Func<Task> action)
    {
        var markup = TemplateEngine.Render(
            Templates.QueryingTemplate,
            new { Model = arg });
        Channel.Writer.TryWrite(markup + "\n");
        await action();
    }
}
