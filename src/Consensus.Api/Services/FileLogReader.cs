using Consensus.Api.Models;

namespace Consensus.Api.Services;

/// <summary>
/// Service for reading log files from the file system
/// </summary>
public class FileLogReader : ILogReader
{
    private readonly string _outputDirectory;
    private readonly ILogger<FileLogReader> _logger;

    public FileLogReader(string outputDirectory, ILogger<FileLogReader> logger)
    {
        _outputDirectory = outputDirectory;
        _logger = logger;
    }

    public async Task<IEnumerable<LogEntryModel>> ReadLogsAsync(string runId)
    {
        var filePath = GetLogFilePath(runId);
        
        if (!File.Exists(filePath))
        {
            _logger.LogInformation("Log file not yet available for runId: {RunId} at path: {FilePath}", runId, filePath);
            return new List<LogEntryModel>();
        }

        try
        {
            _logger.LogInformation("Reading log file for runId: {RunId}", runId);
            var lines = await File.ReadAllLinesAsync(filePath);
            
            var logs = lines.Select(line => new LogEntryModel
            {
                Timestamp = DateTime.UtcNow,
                Message = line
            }).ToList();

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading log file for runId: {RunId}", runId);
            return new List<LogEntryModel>
            {
                new LogEntryModel
                {
                    Timestamp = DateTime.UtcNow,
                    Message = $"Error reading log file: {ex.Message}"
                }
            };
        }
    }

    private string GetLogFilePath(string runId)
    {
        return Path.Combine(_outputDirectory, "logs", $"consensus-{runId}.log");
    }
}
