using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Consensus.Configuration;
using Consensus.DI;

namespace Consensus;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Validate command-line arguments
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: consensus <prompt-file> <models-file>");
                Console.WriteLine("Example: consensus prompt.txt models.txt");
                return 1;
            }

            // Load and validate settings from arguments and environment
            var settings = ConsensusAgentSettings.CreateFromArgsAndEnvironment(args[0], args[1]);

            // Read prompt file content
            string prompt = await File.ReadAllTextAsync(settings.PromptFile);

            // Setup configuration
            string outputDir = Path.GetDirectoryName(settings.PromptFile) ?? ".";
            var config = new ConsensusConfiguration
            {
                PromptFile = settings.PromptFile,
                ModelsFile = settings.ModelsFile,
                ApiEndpoint = settings.ApiEndpoint,
                ApiKey = settings.ApiKey,
                Models = settings.Models,
                MinimumAgentsRequired = Math.Max(3, settings.Models.Length * 2 / 3), // At least 2/3 of models
                OutputDirectory = outputDir
            };

            // Setup dependency injection
            var services = new ServiceCollection();
            services.AddConsensusServices(config);
            var serviceProvider = services.BuildServiceProvider();

            // Get logger for Program
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting parallel-then-synthesize consensus with {ModelCount} models...", settings.Models.Length);
            logger.LogInformation("Agent timeout: {AgentTimeout} seconds", config.AgentTimeoutSeconds);
            logger.LogInformation("Minimum agents required: {MinimumAgents}", config.MinimumAgentsRequired);
            logger.LogInformation("📝 Log file: {LogFile}", config.LogFile);

            // Get orchestrator and run consensus process
            var orchestrator = serviceProvider.GetRequiredService<ConsensusOrchestrator>();
            var result = await orchestrator.GetConsensusAsync(prompt);

            // Save consensus output
            await orchestrator.SaveConsensusAsync(result);

            logger.LogInformation("✓ Consensus saved to: {ConsensusFile}", config.ConsensusFile);
            logger.LogInformation("✓ Conversation log saved to: {LogFile}", config.LogFile);
            logger.LogInformation("✓ Consensus level: {ConsensusLevel}", result.ConsensusLevel);
            logger.LogInformation("✓ Processing time: {ProcessingTime:F2}s", result.TotalProcessingTime.TotalSeconds);

            return 0;
        }
        catch (SettingsException ex)
        {
            // Settings validation errors - user-friendly output
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            // Unexpected errors - full details
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
