using Consensus;
using Consensus.Console;
using Consensus.Api;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Consensus API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

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
})
    .WithName("CreateConsensus")
    .WithSummary("Generates an aggregated answer from multiple models.")
    .WithOpenApi();

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
})
    .WithName("StreamConsensus")
    .WithSummary("Streams each model's response in OpenAI format.")
    .WithOpenApi();

app.MapPost("/v1/chat/completions", async (HttpRequest httpRequest, HttpResponse response, OpenAIChatRequest request) =>
{
    var auth = httpRequest.Headers["Authorization"].FirstOrDefault();
    var apiKey = string.Empty;
    if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer "))
    {
        apiKey = auth.Substring("Bearer ".Length).Trim();
    }
    if (string.IsNullOrEmpty(apiKey))
    {
        apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? string.Empty;
    }

    var models = request.Model.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var prompt = request.Messages.LastOrDefault()?.Content ?? string.Empty;

    var console = new ApiConsoleService();
    var client = new OpenRouterClient(apiKey);
    var processor = new ConsensusProcessor(client, console, NullLogger<ConsensusProcessor>.Instance);

    if (!request.Stream)
    {
        var result = await processor.RunAsync(prompt, models, Consensus.LogLevel.None);
        var resp = new
        {
            id = Guid.NewGuid().ToString(),
            choices = new[] { new { index = 0, message = new { role = "assistant", content = result.Answer }, finish_reason = "stop" } },
            consensus_result = new ConsensusResponse(result.Path, result.Answer, result.ChangesSummary, result.LogPath)
        };
        response.ContentType = "application/json";
        await response.WriteAsync(JsonSerializer.Serialize(resp));
        return;
    }

    response.Headers.Add("Content-Type", "text/event-stream");
    response.Headers.Add("Cache-Control", "no-cache");

    ConsensusResponse? finalResponse = null;

    var processing = Task.Run(async () =>
    {
        try
        {
            var result = await processor.RunAsync(prompt, models, Consensus.LogLevel.None, outputAnswers: false);
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
})
    .WithName("OpenAIChatCompletions")
    .WithSummary("OpenAI-compatible chat completions endpoint.")
    .WithOpenApi();

app.MapGet("/log", (string path) =>
{
    if (!File.Exists(path))
    {
        return Results.NotFound();
    }

    var text = File.ReadAllText(path);
    return Results.Text(text, "text/markdown");
})
    .WithName("GetLog")
    .WithSummary("Returns the contents of a previous log file.")
    .WithOpenApi();

app.Run();
