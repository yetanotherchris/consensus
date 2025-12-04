using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Consensus.Configuration;
using Consensus.Services;
using Consensus.Logging;
using Consensus.Channels;

namespace Consensus.DI;

/// <summary>
/// Extension methods for configuring dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all consensus building services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Consensus configuration settings</param>
    /// <param name="logDirectory">Directory where consensus run logs will be stored. Defaults to "./output/logs"</param>
    public static IServiceCollection AddConsensus(
        this IServiceCollection services,
        ConsensusConfiguration configuration,
        string? logDirectory = null)
    {
        // Register configuration as singleton
        services.AddSingleton(configuration);

        // Add Microsoft.Extensions.Logging support
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Determine log directory
        var effectiveLogDirectory = logDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "output", "logs");

        // Register channel-based run tracker as singleton with disposal
        services.AddSingleton<ConsensusRunTracker>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ConsensusRunTracker>>();
            return new ConsensusRunTracker(logger, effectiveLogDirectory);
        });

        // Register services as transient (new instance per resolution)
        services.AddTransient<IAgentService, AgentService>();
        services.AddTransient<IPromptBuilder, PromptBuilder>();
        services.AddTransient<ISynthesizerService, SynthesizerService>();
        services.AddTransient<IMarkdownOutputService, MarkdownOutputService>();
        services.AddTransient<IHtmlOutputService, HtmlOutputService>();
        services.AddTransient<ConsensusOrchestrator>();

        return services;
    }

    /// <summary>
    /// Register IIntermediateResponsePersistence for saving/loading model responses
    /// </summary>
    public static IServiceCollection AddIntermediateResponsePersistence(
        this IServiceCollection services,
        string outputDirectory)
    {
        services.AddTransient<IIntermediateResponsePersistence>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IntermediateResponsePersistence>>();
            return new IntermediateResponsePersistence(logger, outputDirectory);
        });

        return services;
    }

    /// <summary>
    /// Register SimpleFileLogger as a singleton with a specific log file path
    /// </summary>
    public static IServiceCollection AddSimpleFileLogger(
        this IServiceCollection services, 
        string outputDirectory,
        string? outputFilenamesId = null)
    {
        // Build log file path
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var filenameIdentifier = outputFilenamesId ?? timestamp;
        var logFilePath = Path.Combine(outputDirectory, "logs", $"conversation-log-{filenameIdentifier}.txt");
        
        // Register logger as singleton (shared log file)
        services.AddSingleton(new SimpleFileLogger(logFilePath));

        return services;
    }

    /// <summary>
    /// Register IOutputWriter (FileOutputWriter) for file system output operations
    /// </summary>
    public static IServiceCollection AddFileOutputWriter(
        this IServiceCollection services,
        string outputDirectory,
        string? outputFilenamesId = null)
    {
        services.AddTransient<IOutputWriter>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FileOutputWriter>>();
            return new FileOutputWriter(logger, outputDirectory, outputFilenamesId);
        });

        return services;
    }
}
