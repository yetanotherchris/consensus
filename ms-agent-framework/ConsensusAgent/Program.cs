using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConsensusAgent.Configuration;
using ConsensusAgent.DI;

namespace ConsensusAgent;

class Program
{
    private const int MaxRounds = 5;
    private const int QueryTimeoutSeconds = 90;

    static async Task<int> Main(string[] args)
    {
        try
        {
            // Validate command-line arguments
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ConsensusAgent <prompt-file> <models-file>");
                Console.WriteLine("Example: ConsensusAgent prompt.txt models.txt");
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
                MaxRounds = MaxRounds,
                QueryTimeoutSeconds = QueryTimeoutSeconds,
                OutputDirectory = outputDir
            };

            // Setup dependency injection
            var services = new ServiceCollection();
            services.AddConsensusServices(config);
            var serviceProvider = services.BuildServiceProvider();

            // Get logger for Program
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Starting consensus building with {ModelCount} models over {MaxRounds} rounds...", settings.Models.Length, MaxRounds);
            logger.LogInformation("Total requests: {TotalRequests}", settings.Models.Length * MaxRounds);
            logger.LogInformation("Query timeout: {QueryTimeout} seconds", QueryTimeoutSeconds);
            logger.LogInformation("📝 Log file: {LogFile}", config.LogFile);

            // Get orchestrator and run consensus process
            var orchestrator = serviceProvider.GetRequiredService<ConsensusOrchestrator>();
            string consensus = await orchestrator.BuildConsensusAsync(prompt);

            // Save consensus output
            await orchestrator.SaveConsensusAsync(consensus);

            logger.LogInformation("✓ Consensus saved to: {ConsensusFile}", config.ConsensusFile);
            logger.LogInformation("✓ Conversation log saved to: {LogFile}", config.LogFile);

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
