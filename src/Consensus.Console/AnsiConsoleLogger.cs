using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Consensus.Console;

/// <summary>
/// Simple <see cref="ILogger"/> implementation that writes markup
/// strings to <see cref="Spectre.Console.AnsiConsole"/>.
/// </summary>
internal sealed class AnsiConsoleLogger<T> : ILogger<T>
{
    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();
        public void Dispose() { }
    }

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        if (state is IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (var kvp in values)
            {
                if (kvp.Key == "{OriginalFormat}")
                    continue;

                var valueText = kvp.Value?.ToString();

                if (string.IsNullOrEmpty(valueText))
                    continue;

                message = message.Replace(valueText, $"[green]{valueText}[/]");
            }
        }

        var color = logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Error => "red",
            Microsoft.Extensions.Logging.LogLevel.Warning => "yellow",
            _ => null
        };

        if (color is not null)
        {
            Spectre.Console.AnsiConsole.MarkupLine($"[{color}]{message}[/]");
        }
        else
        {
            Spectre.Console.AnsiConsole.MarkupLine(message);
        }
    }
}
