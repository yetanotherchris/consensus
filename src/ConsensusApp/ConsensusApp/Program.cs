using Spectre.Console;
using Spectre.Console.Cli;

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
        var prompt = settings.Prompt ?? AnsiConsole.Ask<string>("Enter your question:");

        var suggestions = new[]
        {
            "openai/gpt-4o",
            "meta-llama/llama-3-70b-instruct",
            "anthropic/claude-3-opus",
            "perplexity/pplx-70b-online",
            "xai/grok-1",
            "google/gemini-pro"
        };

        var models = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select LLMs in the order to consult")
                .InstructionsText("[grey](Press <space> to toggle a model, <enter> to accept)[/]")
                .AddChoices(suggestions));

        if (models.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No models selected.[/]");
            return 1;
        }

        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
            AnsiConsole.Prompt(new TextPrompt<string>("Enter your OpenRouter API key:").Secret());

        var client = new OpenRouterClient(apiKey);
        var processor = new ConsensusProcessor(client);

        var result = await processor.RunAsync(prompt, models);

        AnsiConsole.MarkupLine($"[bold yellow]Summary:[/] {result.Summary}");
        AnsiConsole.MarkupLine($"Full answer written to [green]{result.Path}[/]");

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
