using System.Text.Json;
using Consensus.Models;
using Microsoft.Extensions.Logging;

namespace Consensus.Services;

/// <summary>
/// File-based implementation for persisting intermediate model responses
/// </summary>
public class IntermediateResponsePersistence : IIntermediateResponsePersistence
{
    private readonly ILogger<IntermediateResponsePersistence> _logger;
    private readonly string _outputDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public IntermediateResponsePersistence(ILogger<IntermediateResponsePersistence> logger, string outputDirectory)
    {
        _logger = logger;
        _outputDirectory = outputDirectory;
    }

    public async Task SaveResponsesAsync(string runId, List<ModelResponse> responses)
    {
        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or empty", nameof(runId));

        if (responses == null || responses.Count == 0)
        {
            _logger.LogWarning("No responses to save for run {RunId}", runId);
            return;
        }

        var runDirectory = GetRunDirectory(runId);
        EnsureDirectoryExists(runDirectory);

        _logger.LogInformation("Saving {Count} responses to {Directory}", responses.Count, runDirectory);

        foreach (var response in responses)
        {
            try
            {
                var sanitizedModelName = SanitizeFileName(response.ModelName);
                var filePath = Path.Combine(runDirectory, $"{sanitizedModelName}.json");

                var json = JsonSerializer.Serialize(response, JsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                _logger.LogDebug("Saved response for model {ModelName} to {FilePath}", response.ModelName, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save response for model {ModelName}", response.ModelName);
                throw;
            }
        }

        _logger.LogInformation("Successfully saved all {Count} responses for run {RunId}", responses.Count, runId);
    }

    public async Task<List<ModelResponse>> LoadResponsesAsync(string runId)
    {
        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or empty", nameof(runId));

        var runDirectory = GetRunDirectory(runId);

        if (!Directory.Exists(runDirectory))
        {
            _logger.LogWarning("Run directory does not exist: {Directory}", runDirectory);
            return new List<ModelResponse>();
        }

        var responses = new List<ModelResponse>();
        var jsonFiles = Directory.GetFiles(runDirectory, "*.json");

        _logger.LogInformation("Loading {Count} response files from {Directory}", jsonFiles.Length, runDirectory);

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var response = JsonSerializer.Deserialize<ModelResponse>(json, JsonOptions);

                if (response != null)
                {
                    responses.Add(response);
                    _logger.LogDebug("Loaded response for model {ModelName} from {FilePath}", response.ModelName, filePath);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize response from {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load response from {FilePath}", filePath);
            }
        }

        _logger.LogInformation("Successfully loaded {Count} responses for run {RunId}", responses.Count, runId);
        return responses;
    }

    public Task<bool> ResponsesExistAsync(string runId)
    {
        if (string.IsNullOrWhiteSpace(runId))
            return Task.FromResult(false);

        var runDirectory = GetRunDirectory(runId);
        var exists = Directory.Exists(runDirectory) && Directory.GetFiles(runDirectory, "*.json").Length > 0;

        return Task.FromResult(exists);
    }

    private string GetRunDirectory(string runId)
    {
        return Path.Combine(_outputDirectory, "responses", runId);
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}
