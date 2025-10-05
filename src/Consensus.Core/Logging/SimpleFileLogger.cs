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
}
