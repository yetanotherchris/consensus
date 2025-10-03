using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConsensusAgent.Configuration;
using ConsensusAgent.Logging;
using ConsensusAgent.Services;

namespace ConsensusAgent.DI;

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
        ConsensusConfiguration configuration)
    {
        // Register configuration as singleton
        services.AddSingleton(configuration);
        
        // Register logger as singleton (shared log file)
        services.AddSingleton(new SimpleLogger(configuration.LogFile));
        
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
        services.AddTransient<IOutputService, OutputService>();
        services.AddTransient<ConsensusOrchestrator>();
        
        return services;
    }
}
