namespace Consensus.Configuration;

/// <summary>
/// Settings loaded from command-line arguments and environment variables.
/// </summary>
public class ConsensusAgentSettings
{
    public string ApiEndpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string[] Models { get; init; } = Array.Empty<string>();
    public string? OutputFilenamesId { get; init; }

    /// <summary>
    /// Creates settings from file paths and environment variables.
    /// </summary>
    /// <param name="promptFile">Path to the prompt file (used only for validation)</param>
    /// <param name="modelsFile">Path to the models file (read but not stored)</param>
    /// <param name="outputFilenamesId">Optional custom ID for output filenames</param>
    /// <returns>Validated settings instance</returns>
    /// <exception cref="SettingsException">Thrown when validation fails</exception>
    public static ConsensusAgentSettings CreateFromArgsAndEnvironment(string promptFile, string modelsFile, string? outputFilenamesId = null)
    {

        // Validate files exist
        if (!File.Exists(promptFile))
        {
            throw new SettingsException($"Prompt file '{promptFile}' not found.");
        }

        if (!File.Exists(modelsFile))
        {
            throw new SettingsException($"Models file '{modelsFile}' not found.");
        }

        // Read and parse models file
        string[] models;
        try
        {
            string modelsContent = File.ReadAllText(modelsFile);
            models = modelsContent
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (models.Length == 0)
            {
                throw new SettingsException("No models found in models file.");
            }
        }
        catch (Exception ex) when (ex is not SettingsException)
        {
            throw new SettingsException($"Failed to read models file '{modelsFile}': {ex.Message}", ex);
        }

        // Get environment variables
        string? apiEndpoint = Environment.GetEnvironmentVariable("CONSENSUS_API_ENDPOINT");
        string? apiKey = Environment.GetEnvironmentVariable("CONSENSUS_API_KEY");

        if (string.IsNullOrWhiteSpace(apiEndpoint))
        {
            throw new SettingsException("CONSENSUS_API_ENDPOINT environment variable not set.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new SettingsException("CONSENSUS_API_KEY environment variable not set.");
        }

        return new ConsensusAgentSettings
        {
            ApiEndpoint = apiEndpoint,
            ApiKey = apiKey,
            Models = models,
            OutputFilenamesId = outputFilenamesId
        };
    }
}
