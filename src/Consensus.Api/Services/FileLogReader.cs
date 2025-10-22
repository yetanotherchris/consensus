using Consensus.Api.Models;
using System.Text.RegularExpressions;

namespace Consensus.Api.Services;

/// <summary>
/// Service for reading log files from the file system
/// </summary>
public class FileLogReader : ILogReader
{
    private readonly string _outputDirectory;
    private readonly ILogger<FileLogReader> _logger;

    // Regex to parse log format: [LEVEL][DATETIME] Message
    private static readonly Regex LogLineRegex = new(@"^\[(\w+)\]\[([^\]]+)\]\s(.*)$", RegexOptions.Compiled);

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

            var logs = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(ParseLogLine)
                .ToList();

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

    private LogEntryModel ParseLogLine(string line)
    {
        var match = LogLineRegex.Match(line);

        if (match.Success)
        {
            // Extract level, timestamp, and message
            var level = match.Groups[1].Value;
            var timestampStr = match.Groups[2].Value;
            var message = match.Groups[3].Value;

            // Try to parse the timestamp
            if (DateTime.TryParseExact(timestampStr, "yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeLocal,
                out DateTime timestamp))
            {
                return new LogEntryModel
                {
                    Timestamp = timestamp,
                    Message = $"[{level}] {message}"
                };
            }
        }

        // If parsing fails, return the line as-is with current timestamp
        return new LogEntryModel
        {
            Timestamp = DateTime.UtcNow,
            Message = line
        };
    }

    private string GetLogFilePath(string runId)
    {
        return Path.Combine(_outputDirectory, "logs", $"consensus-{runId}.log");
    }
}
