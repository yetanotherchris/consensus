using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Consensus.Configuration;
using Consensus.DI;
using System.CommandLine;

namespace Consensus;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Define command-line options
        var promptFileOption = new Option<string>("--prompt-file")
        {
            Description = "Path to the prompt file",
            Required = true
        };

        var modelsFileOption = new Option<string>("--models-file")
        {
            Description = "Path to the models file",
            Required = true
        };

        var outputFilenamesIdOption = new Option<string?>("--output-filenames-id")
        {
            Description = "Optional unique ID for output filenames (replaces timestamp)"
        };

        // Create root command
        var rootCommand = new RootCommand("Consensus Agent - Build consensus from multiple AI models");
        rootCommand.Options.Add(promptFileOption);
        rootCommand.Options.Add(modelsFileOption);
        rootCommand.Options.Add(outputFilenamesIdOption);

        // Set command handler
        rootCommand.SetAction(async parseResult =>
        {
            try
            {
                var promptFile = parseResult.GetValue(promptFileOption)!;
                var modelsFile = parseResult.GetValue(modelsFileOption)!;
                var outputFilenamesId = parseResult.GetValue(outputFilenamesIdOption);

                // Load and validate settings from arguments and environment
                var settings = ConsensusAgentSettings.CreateFromArgsAndEnvironment(promptFile, modelsFile, outputFilenamesId);

                // Read prompt file content
                string prompt = await File.ReadAllTextAsync(promptFile);

                // Setup configuration
                string outputDir = Path.GetDirectoryName(promptFile) ?? ".";
                var config = new ConsensusConfiguration
                {
                    ApiEndpoint = settings.ApiEndpoint,
                    ApiKey = settings.ApiKey,
                };

                // Setup dependency injection
                var services = new ServiceCollection();
                services.AddConsensus(config)
                        .AddSimpleFileLogger(outputDir, settings.OutputFilenamesId)
                        .AddFileOutputWriter(outputDir, settings.OutputFilenamesId)
                        .AddIntermediateResponsePersistence(outputDir);
                var serviceProvider = services.BuildServiceProvider();

                // Get logger for Program
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Starting parallel-then-synthesize consensus with {ModelCount} models...", settings.Models.Length);
                logger.LogInformation("Agent timeout: {AgentTimeout} seconds", config.AgentTimeoutSeconds);

                // Get orchestrator and run consensus process
                var orchestrator = serviceProvider.GetRequiredService<ConsensusOrchestrator>();
                var result = await orchestrator.GetConsensusAsync(prompt, settings.Models);

                // Save consensus output
                await orchestrator.SaveConsensusAsync(result);

                // Calculate output paths for logging
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var filenameIdentifier = settings.OutputFilenamesId ?? timestamp;
                var consensusFile = Path.Combine(outputDir, "output", "responses", $"consensus-{filenameIdentifier}.md");
                var logFile = Path.Combine(outputDir, "output", "logs", $"conversation-log-{filenameIdentifier}.txt");

                logger.LogInformation("✓ Consensus saved to: {ConsensusFile}", consensusFile);
                logger.LogInformation("✓ Conversation log saved to: {LogFile}", logFile);
                logger.LogInformation("✓ Consensus level: {ConsensusLevel}", result.ConsensusLevel);
                logger.LogInformation("✓ Processing time: {ProcessingTime:F2}s", result.TotalProcessingTime.TotalSeconds);
            }
            catch (SettingsException ex)
            {
                // Settings validation errors - user-friendly output
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                // Unexpected errors - full details
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        // Execute command
        return await rootCommand.Parse(args).InvokeAsync();
    }
}
