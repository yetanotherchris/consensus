using Consensus;
using Consensus.Console;
using Consensus.Api;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

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
    return new ConsensusResponse(result.Path, result.Answer, result.ChangesSummary, result.LogPath);
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
    response.Headers.Add("Cache-Control", "no-cache");

    ConsensusResponse? finalResponse = null;

    var processing = Task.Run(async () =>
    {
        try
        {
            var result = await processor.RunAsync(request.Prompt, request.Models, logLevel);
            finalResponse = new ConsensusResponse(result.Path, result.Answer, result.ChangesSummary, result.LogPath);
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
        if (message.StartsWith("ERROR:"))
        {
            var errorJson = JsonSerializer.Serialize(new { error = message.Substring(6) });
            await response.WriteAsync($"data: {errorJson}\n\n");
            await response.Body.FlushAsync();
            break;
        }

        var chunk = new
        {
            choices = new[] { new { index = 0, delta = new { content = message }, finish_reason = (string?)null } }
        };
        var json = JsonSerializer.Serialize(chunk);
        await response.WriteAsync($"data: {json}\n\n");
        await response.Body.FlushAsync();
    }

    if (finalResponse is not null)
    {
        var finalJson = JsonSerializer.Serialize(new
        {
            choices = new[] { new { index = 0, delta = new { }, finish_reason = "stop" } },
            consensus_result = finalResponse
        });
        await response.WriteAsync($"data: {finalJson}\n\n");
        await response.Body.FlushAsync();
    }

    await response.WriteAsync("data: [DONE]\n\n");
    await response.Body.FlushAsync();

    await processing;
});

app.MapGet("/log", (string path) =>
{
    if (!File.Exists(path))
    {
        return Results.NotFound();
    }

    var text = File.ReadAllText(path);
    return Results.Text(text, "text/markdown");
});

app.Run();
