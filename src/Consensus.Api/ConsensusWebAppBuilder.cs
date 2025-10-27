using Consensus.Api.Jobs.Scheduling;
using Consensus.Configuration;
using Consensus.DI;
using Quartz;

namespace Consensus.Api;

public class ConsensusWebAppBuilder
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public ConsensusWebAppBuilder(WebApplicationBuilder builder)
    {
        SetupConfiguration(builder);
        SetupLogging(builder);

        _configuration = builder.Configuration;
        _environment = builder.Environment;
    }

    private void SetupConfiguration(WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    private void SetupLogging(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
    }

    public ConsensusConfiguration GetConfiguration()
    {
        var apiEndpoint = GetConfigValue("Consensus:ApiEndpoint", "CONSENSUS_API_ENDPOINT");
        if (string.IsNullOrEmpty(apiEndpoint))
        {
            throw new InvalidOperationException("Consensus:ApiEndpoint (or CONSENSUS_API_ENDPOINT) is required");
        }

        var apiKey = GetConfigValue("Consensus:ApiKey", "CONSENSUS_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Consensus:ApiKey (or CONSENSUS_API_KEY) is required");
        }

        var domain = GetConfigValue("Consensus:Domain", "CONSENSUS_DOMAIN");
        if (string.IsNullOrEmpty(domain))
        {
            domain = "General";
        }

        var agentTimeoutValue = GetConfigValue("Consensus:AgentTimeoutSeconds");
        var agentTimeoutSeconds = string.IsNullOrEmpty(agentTimeoutValue) ? 120 : int.Parse(agentTimeoutValue);

        var includeIndividualResponsesValue = GetConfigValue("Consensus:IncludeIndividualResponses");
        var includeIndividualResponses = string.IsNullOrEmpty(includeIndividualResponsesValue) || bool.Parse(includeIndividualResponsesValue);

        // Read Models array from configuration
        var models = _configuration.GetSection("Consensus:Models").Get<string[]>() ?? Array.Empty<string>();

        var consensusConfig = new ConsensusConfiguration
        {
            ApiEndpoint = apiEndpoint,
            ApiKey = apiKey,
            Domain = domain,
            Models = models,
            AgentTimeoutSeconds = agentTimeoutSeconds,
            IncludeIndividualResponses = includeIndividualResponses
        };

        return consensusConfig;
    }

    public void RegisterServices(IServiceCollection services)
    {
        ConsensusConfiguration consensusConfig = GetConfiguration();

        // Register ConsensusConfiguration as singleton for injection
        services.AddSingleton(consensusConfig);
        string outputDirectory = GetConfigValue("OutputDirectory") ?? Path.Combine(Directory.GetCurrentDirectory(), "output");
        string logDirectory = Path.Combine(outputDirectory, "logs");

        // Add consensus services with configurable log directory
        services.AddConsensus(consensusConfig, logDirectory)
                .AddSimpleFileLogger(outputDirectory)
                .AddFileOutputWriter(outputDirectory);

        services.AddSingleton<IJobScheduler, QuartzJobScheduler>();
        services.AddSingleton<Api.Services.IOutputFileReaderService>(sp => 
            new Api.Services.OutputFileReaderService(outputDirectory, sp.GetRequiredService<ILogger<Api.Services.OutputFileReaderService>>()));
        services.AddSingleton<Api.Services.ILogReader>(sp => 
            new Api.Services.FileLogReader(outputDirectory, sp.GetRequiredService<ILogger<Api.Services.FileLogReader>>()));
        RegisterQuartzServices(services);
        
        services.AddControllers();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                    _configuration["Cors:AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:5173", "http://localhost:3000" })
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    private void RegisterQuartzServices(IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseInMemoryStore();

            // Configure Output Cleanup Job if CleanupCrontab is specified
            var cleanupCrontab = GetConfigValue("Consensus:CleanupCrontab") ?? "0 0/30 * * * ?"; // Default: every 30 minutes

            if (!string.IsNullOrWhiteSpace(cleanupCrontab))
            {
                var cleanupJobKey = new JobKey("output-cleanup-job", "maintenance");
                q.AddJob<Jobs.OutputCleanupJob>(opts => opts
                    .WithIdentity(cleanupJobKey)
                    .StoreDurably());

                q.AddTrigger(opts => opts
                    .ForJob(cleanupJobKey)
                    .WithIdentity("output-cleanup-trigger", "maintenance")
                    .WithCronSchedule(cleanupCrontab)
                    .StartNow());
            }
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }

    public void SetupMiddleware(WebApplication app)
    {
        app.UseCors("AllowFrontend");

        // Serve static files from wwwroot
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        // Fallback to index.html for client-side routing
        app.MapFallbackToFile("index.html");
    }

    private string? GetConfigValue(string key, string? altKey = null)
    {
        var value = _configuration[key];
        if (value == null && altKey != null)
        {
            value = _configuration[altKey];
        }
        return value;
    }
}
