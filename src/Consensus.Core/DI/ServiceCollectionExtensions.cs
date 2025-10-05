using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Consensus.Configuration;
using Consensus.Services;
using Consensus.Logging;

namespace Consensus.DI;

/// <summary>
/// Extension methods for configuring dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all consensus building services to the service collection
    /// </summary>
    public static IServiceCollection AddConsensus(
        this IServiceCollection services,
        ConsensusConfiguration configuration)
    {
        // Register configuration as singleton
        services.AddSingleton(configuration);

        // Add Microsoft.Extensions.Logging support
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
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
        var logFilePath = Path.Combine(outputDirectory, "output", "logs", $"conversation-log-{filenameIdentifier}.txt");
        
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
