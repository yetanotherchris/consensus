namespace Consensus.Core;

using Spectre.Console;

public interface IConsoleService
{
    T Ask<T>(string prompt);
    T Prompt<T>(IPrompt<T> prompt);
    void MarkupLine(string markup);
    Task StatusAsync(string status, Func<Task> action);
    Task StatusAsync<T>(string statusFormat, T arg, Func<Task> action);
}
