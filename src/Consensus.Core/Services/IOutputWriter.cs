namespace Consensus.Services;

/// <summary>
/// Service interface for writing output to storage (filesystem, database, etc.)
/// </summary>
public interface IOutputWriter
{
    /// <summary>
    /// Write HTML content to storage
    /// </summary>
    /// <param name="content">The HTML content to write</param>
    /// <param name="id">Optional identifier for the output. If null, implementation may use a timestamp or default naming</param>
    Task WriteHtmlAsync(string content, string? id = null);
    
    /// <summary>
    /// Write Markdown content to storage
    /// </summary>
    /// <param name="content">The Markdown content to write</param>
    /// <param name="id">Optional identifier for the output. If null, implementation may use a timestamp or default naming</param>
    Task WriteMarkdownAsync(string content, string? id = null);
}
