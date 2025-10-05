using Consensus.Api.Jobs;
using Consensus.Api.Jobs.Scheduling;
using Consensus.Configuration;
using Consensus.DI;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add configuration from appsettings.json and environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure consensus settings from configuration
var consensusConfig = new ConsensusConfiguration
{
    ApiEndpoint = builder.Configuration["Consensus:ApiEndpoint"] ?? throw new InvalidOperationException("Consensus:ApiEndpoint is required"),
    ApiKey = builder.Configuration["Consensus:ApiKey"] ?? throw new InvalidOperationException("Consensus:ApiKey is required"),
    Domain = builder.Configuration["Consensus:Domain"] ?? "General",
    AgentTimeoutSeconds = int.Parse(builder.Configuration["Consensus:AgentTimeoutSeconds"] ?? "120"),
    EnableCaching = bool.Parse(builder.Configuration["Consensus:EnableCaching"] ?? "false"),
    CacheTTLMinutes = int.Parse(builder.Configuration["Consensus:CacheTTLMinutes"] ?? "60"),
    IncludeIndividualResponses = bool.Parse(builder.Configuration["Consensus:IncludeIndividualResponses"] ?? "true")
};

// Add consensus services
builder.Services.AddConsensus(consensusConfig);

// Add job scheduler (uses Quartz for scheduling and tracking)
builder.Services.AddSingleton<IJobScheduler, QuartzJobScheduler>();

// Add Quartz services
builder.Services.AddQuartz(q =>
{
    // Use in-memory job store
    q.UseInMemoryStore();
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(options =>
{
    // Wait for jobs to complete on shutdown
    options.WaitForJobsToComplete = true;
});

// Add controllers
builder.Services.AddControllers();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
