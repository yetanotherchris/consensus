using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Consensus.Configuration;
using Consensus.Logging;
using Consensus.Services;

namespace Consensus.DI;

/// <summary>
/// Extension methods for configuring dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all consensus building services to the service collection
    /// </summary>
    public static IServiceCollection AddConsensusServices(
        this IServiceCollection services,
        ConsensusConfiguration configuration,
        string? outputFilenamesId = null)
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
        services.AddTransient<IOutputWriter>(sp => 
            new FileOutputWriter(
                sp.GetRequiredService<SimpleFileLogger>(), 
                sp.GetRequiredService<ConsensusConfiguration>(),
                outputFilenamesId));
        services.AddTransient<IMarkdownOutputService, MarkdownOutputService>();
        services.AddTransient<IHtmlOutputService, HtmlOutputService>();
        services.AddTransient<ConsensusOrchestrator>();

        return services;
    }

    public static IServiceCollection AddSimpleFileLogger(
        this IServiceCollection services, 
        ConsensusConfiguration configuration,
        string? outputFilenamesId = null)
    {
        // Build log file path
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var filenameIdentifier = outputFilenamesId ?? timestamp;
        var logFilePath = Path.Combine(configuration.OutputDirectory, "output", "logs", $"conversation-log-{filenameIdentifier}.txt");
        
        // Register logger as singleton (shared log file)
        services.AddSingleton(new SimpleFileLogger(logFilePath));

        return services;
    }
}
