using Consensus.Configuration;
using Consensus.Logging;

namespace Consensus.Services;

/// <summary>
/// Service for writing output files to the file system
/// </summary>
public class FileOutputWriter : IOutputWriter
{
    private readonly SimpleFileLogger _logger;
    private readonly ConsensusConfiguration _config;
    private readonly string? _outputFilenamesId;
    private readonly string _timestamp;
    private readonly string _filenameIdentifier;

    public FileOutputWriter(SimpleFileLogger logger, ConsensusConfiguration config, string? outputFilenamesId = null)
    {
        _logger = logger;
        _config = config;
        _outputFilenamesId = outputFilenamesId;
        _timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        _filenameIdentifier = outputFilenamesId ?? _timestamp;
    }

    /// <summary>
    /// Write HTML content to a file
    /// </summary>
    /// <param name="content">The HTML content to write</param>
    /// <param name="id">Optional identifier for the output. If null, uses internal filename identifier</param>
    public async Task WriteHtmlAsync(string content, string? id = null)
    {
        var filenameIdentifier = id ?? _filenameIdentifier;
        var fileName = $"output-{filenameIdentifier}.html";
        var filePath = Path.Combine(_config.OutputDirectory, "output", "responses", fileName);
        
        _logger.LogInformation("Writing HTML output to file: {0}", filePath);
        
        EnsureDirectoryExists(filePath);
        
        await File.WriteAllTextAsync(filePath, content);
        
        _logger.LogInformation("HTML file written successfully");
    }

    /// <summary>
    /// Write Markdown content to a file
    /// </summary>
    /// <param name="content">The Markdown content to write</param>
    /// <param name="id">Optional identifier for the output. If null, uses internal filename identifier</param>
    public async Task WriteMarkdownAsync(string content, string? id = null)
    {
        var filenameIdentifier = id ?? _filenameIdentifier;
        var fileName = $"consensus-{filenameIdentifier}.md";
        var filePath = Path.Combine(_config.OutputDirectory, "output", "responses", fileName);
        
        _logger.LogInformation("Writing Markdown output to file: {0}", filePath);
        
        EnsureDirectoryExists(filePath);
        
        await File.WriteAllTextAsync(filePath, content);
        
        _logger.LogInformation("Markdown file written successfully");
    }

    /// <summary>
    /// Ensures the directory for the given file path exists
    /// </summary>
    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
