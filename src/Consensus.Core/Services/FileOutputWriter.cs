using Consensus.Configuration;
using Consensus.Logging;

namespace Consensus.Services;

/// <summary>
/// Service for writing output files to the file system
/// </summary>
public class FileOutputWriter : IOutputWriter
{
    private readonly SimpleLogger _logger;
    private readonly ConsensusConfiguration _config;

    public FileOutputWriter(SimpleLogger logger, ConsensusConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Write HTML content to a file
    /// </summary>
    /// <param name="content">The HTML content to write</param>
    /// <param name="id">Optional identifier for the output. If null, uses timestamp from configuration</param>
    public async Task WriteHtmlAsync(string content, string? id = null)
    {
        var filenameIdentifier = id ?? _config.OutputFilenamesId ?? DateTime.Now.ToString("yyyyMMdd-HHmmss");
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
    /// <param name="id">Optional identifier for the output. If null, uses timestamp from configuration</param>
    public async Task WriteMarkdownAsync(string content, string? id = null)
    {
        var filenameIdentifier = id ?? _config.OutputFilenamesId ?? DateTime.Now.ToString("yyyyMMdd-HHmmss");
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
