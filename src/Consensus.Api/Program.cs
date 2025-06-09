using Consensus;
using Consensus.Console;
using Consensus.Api;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/consensus", async (ConsensusRequest request) =>
{
    var apiKey = request.ApiKey ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? string.Empty;
    var console = new ApiConsoleService();
    var client = new OpenRouterClient(apiKey);
    var processor = new ConsensusProcessor(client, console, NullLogger<ConsensusProcessor>.Instance);

    var logLevel = request.LogLevel?.ToLowerInvariant() switch
    {
        "minimal" => Consensus.LogLevel.Minimal,
        "full" => Consensus.LogLevel.Full,
        _ => Consensus.LogLevel.None
    };

    var result = await processor.RunAsync(request.Prompt, request.Models, logLevel);
    return new ConsensusResponse(result.Path, result.ChangesSummary, result.LogPath);
});

app.MapPost("/consensus/stream", async (ConsensusRequest request, HttpResponse response) =>
{
    var apiKey = request.ApiKey ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? string.Empty;
    var console = new ApiConsoleService();
    var client = new OpenRouterClient(apiKey);
    var processor = new ConsensusProcessor(client, console, NullLogger<ConsensusProcessor>.Instance);

    var logLevel = request.LogLevel?.ToLowerInvariant() switch
    {
        "minimal" => Consensus.LogLevel.Minimal,
        "full" => Consensus.LogLevel.Full,
        _ => Consensus.LogLevel.None
    };

    response.Headers.Add("Content-Type", "text/event-stream");

    var processing = Task.Run(async () =>
    {
        try
        {
            var result = await processor.RunAsync(request.Prompt, request.Models, logLevel);
            var json = JsonSerializer.Serialize(new ConsensusResponse(result.Path, result.ChangesSummary, result.LogPath));
            await console.Channel.Writer.WriteAsync($"FINAL:{json}");
        }
        catch (Exception ex)
        {
            await console.Channel.Writer.WriteAsync($"ERROR:{ex.Message}");
        }
        finally
        {
            console.Channel.Writer.Complete();
        }
    });

    await foreach (var message in console.Channel.Reader.ReadAllAsync())
    {
        await response.WriteAsync($"data: {message}\n\n");
        await response.Body.FlushAsync();
    }

    await processing;
});

app.Run();
