using System.Text;

namespace Consensus.Logging;

/// <summary>
/// Simple logger that outputs in format: [LEVEL][DATETIME] Message
/// </summary>
public class SimpleFileLogger
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public SimpleFileLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void LogInformation(string message, params object[] args)
    {
        Log("INFO", message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        Log("WARN", message, args);
    }

    public void LogError(string message, params object[] args)
    {
        Log("ERROR", message, args);
    }

    public void LogError(Exception ex, string message, params object[] args)
    {
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        Log("ERROR", $"{formattedMessage} | Exception: {ex.Message}");
    }

    private void Log(string level, string message, params object[] args)
    {
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[{level}][{timestamp}] {formattedMessage}";

        lock (_lock)
        {
            // Write to file
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            
            // Also write to console for immediate feedback
            Console.WriteLine(logEntry);
        }
    }

    /// <summary>
    /// Static method for writing log entries to run-specific log files.
    /// Thread-safe for concurrent writes to different files.
    /// </summary>
    /// <param name="runId">The unique identifier for the consensus run</param>
    /// <param name="message">The message to log</param>
    public static void WriteLogEntry(string runId, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[INFO][{timestamp}] {message}";
        
        var logFilePath = Path.Combine("output", "logs", $"consensus-{runId}.log");
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Use file-specific lock to allow concurrent writes to different files
        lock (GetFileLock(logFilePath))
        {
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }

    // Static dictionary for file-specific locks
    private static readonly Dictionary<string, object> _fileLocks = new();
    private static readonly object _fileLockDictionaryLock = new();

    private static object GetFileLock(string filePath)
    {
        lock (_fileLockDictionaryLock)
        {
            if (!_fileLocks.ContainsKey(filePath))
            {
                _fileLocks[filePath] = new object();
            }
            return _fileLocks[filePath];
        }
    }
}
