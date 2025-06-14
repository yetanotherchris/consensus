namespace Consensus.Console;

using Spectre.Console;
using Consensus.Core;

internal sealed class SpectreConsoleService : IModelQueryService
{
    public ITemplate Templates { get; } = new ConsoleTemplates();
    public T Ask<T>(string prompt) => AnsiConsole.Ask<T>(prompt);

    public T Prompt<T>(IPrompt<T> prompt) => AnsiConsole.Prompt(prompt);

    public void MarkupLine(string markup) => AnsiConsole.MarkupLine(markup);

    public Task StatusAsync(string status, Func<Task> action)
        => AnsiConsole.Status().StartAsync(status, _ => action());

    public Task StatusAsync<T>(string statusFormat, T arg, Func<Task> action)
        => AnsiConsole.Status().StartAsync(
            string.Format(statusFormat, $"[green]{arg}[/]"),
            _ => action());
}
