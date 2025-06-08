using Spectre.Console.Cli;
using Spectre.Console;
using Microsoft.Extensions.Logging;

namespace ConsensusApp;

public sealed class ConsensusCommand : AsyncCommand<ConsensusCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[prompt]")]
        public string? Prompt { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var console = new SpectreConsoleService();
        ILogger<ConsensusCommand> logger = new AnsiConsoleLogger<ConsensusCommand>();
        ILogger<ConsensusProcessor> processorLogger = new AnsiConsoleLogger<ConsensusProcessor>();

        var prompt = settings.Prompt ?? console.Ask<string>("Enter your question:");

        var suggestions = new[]
        {
            "openai/gpt-4o",
            "meta-llama/llama-4-maverick:free",
            "anthropic/claude-3.7-sonnet",
            "x-ai/grok-3-beta",
            "google/gemini-2.0-flash-001",
            "deepseek/deepseek-r1-0528:free"
        };

        var models = console.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select LLMs in the order to consult")
                .InstructionsText("[grey](Press <space> to toggle a model, <enter> to accept)[/]")
                .AddChoices(suggestions));

        if (models.Count == 0)
        {
            logger.LogError("No models selected.");
            return 1;
        }

        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
            console.Prompt(new TextPrompt<string>("Enter your OpenRouter API key:").Secret());

        var logChoice = console.Prompt(
            new SelectionPrompt<string>()
                .Title("Enable logging of intermediate responses?")
                .AddChoices("None", "Minimal", "Full"));

        var logLevel = logChoice switch
        {
            "Minimal" => LogLevel.Minimal,
            "Full" => LogLevel.Full,
            _ => LogLevel.None
        };

        var client = new OpenRouterClient(apiKey);
        var processor = new ConsensusProcessor(client, console, processorLogger);

        var result = await processor.RunAsync(prompt, models, logLevel);

        logger.LogInformation("\n[bold]Summary:[/]\n{Summary}\n", result.Summary);
        logger.LogInformation("[bold]Full answer written to:[/]\n{Path}\n", result.Path);
        if (result.LogPath is not null)
        {
            logger.LogInformation("Log written to {Path}", result.LogPath);
        }

        return 0;
    }
}

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp<ConsensusCommand>();
        return await app.RunAsync(args);
    }
}
