using Quartz;

namespace Consensus.Api.Jobs;

/// <summary>
/// Background job that cleans up output files (markdown, HTML, logs).
/// </summary>
public class OutputCleanupJob : IJob
{
    private readonly ILogger<OutputCleanupJob> _logger;
    private readonly string _outputDirectory;

    public OutputCleanupJob(
        ILogger<OutputCleanupJob> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _outputDirectory = configuration["OutputDirectory"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "output");
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("OutputCleanupJob started at {Time}", DateTime.UtcNow);

        try
        {
            var responsesDir = Path.Combine(_outputDirectory, "responses");
            var logsDir = Path.Combine(_outputDirectory, "logs");

            int totalDeleted = 0;

            // Clean up responses directory (markdown and HTML files)
            if (Directory.Exists(responsesDir))
            {
                totalDeleted += await DeleteFilesAsync(responsesDir, "*.md", "markdown");
                totalDeleted += await DeleteFilesAsync(responsesDir, "*.html", "HTML");
            }
            else
            {
                _logger.LogWarning("Responses directory does not exist: {Directory}", responsesDir);
            }

            // Clean up logs directory
            if (Directory.Exists(logsDir))
            {
                totalDeleted += await DeleteFilesAsync(logsDir, "*.log", "log");
                totalDeleted += await DeleteFilesAsync(logsDir, "*.txt", "text log");
            }
            else
            {
                _logger.LogWarning("Logs directory does not exist: {Directory}", logsDir);
            }

            _logger.LogInformation("OutputCleanupJob completed. Total files deleted: {Count}", totalDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OutputCleanupJob failed");
        }
    }

    private async Task<int> DeleteFilesAsync(string directory, string searchPattern, string fileType)
    {
        int deletedCount = 0;

        try
        {
            var files = Directory.GetFiles(directory, searchPattern);

            foreach (var file in files)
            {
                try
                {
                    await Task.Run(() => File.Delete(file));
                    deletedCount++;
                    _logger.LogDebug("Deleted {FileType} file: {FileName}", fileType, Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file: {FileName}", Path.GetFileName(file));
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInformation("Deleted {Count} {FileType} files from {Directory}", deletedCount, fileType, directory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to access directory for cleanup: {Directory}", directory);
        }

        return deletedCount;
    }
}
