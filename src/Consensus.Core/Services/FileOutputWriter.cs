using Microsoft.Extensions.Logging;

namespace Consensus.Services;

/// <summary>
/// Service for writing output files to the file system
/// </summary>
public class FileOutputWriter : IOutputWriter
{
    private readonly ILogger<FileOutputWriter> _logger;
    private readonly string _outputDirectory;
    private readonly string _timestamp;
    private readonly string _filenameIdentifier;

    public FileOutputWriter(ILogger<FileOutputWriter> logger, string outputDirectory, string? outputFilenamesId = null)
    {
        _logger = logger;
        _outputDirectory = outputDirectory;
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
        var filePath = Path.Combine(_outputDirectory, "responses", fileName);
        
        _logger.LogInformation("Writing HTML output to file: {FilePath}", filePath);
        
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
        var filePath = Path.Combine(_outputDirectory, "responses", fileName);
        
        _logger.LogInformation("Writing Markdown output to file: {FilePath}", filePath);
        
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
