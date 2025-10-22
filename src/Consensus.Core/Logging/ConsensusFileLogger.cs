using System.Collections.Concurrent;

namespace Consensus.Logging;

/// <summary>
/// Static logger for writing consensus run logs to per-run files.
/// Thread-safe for concurrent writes to different files.
/// Used by the channel-based ConsensusRunTracker.
/// </summary>
public static class ConsensusFileLogger
{
    // Use ConcurrentDictionary for thread-safe lock management with cleanup capability
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileSemaphores = new();
    private static readonly int MaxConcurrentWrites = 1; // One writer per file

    /// <summary>
    /// Asynchronously writes a log entry to a run-specific log file.
    /// Thread-safe for concurrent writes to different files.
    /// </summary>
    /// <param name="logDirectory">The base directory for log files</param>
    /// <param name="runId">The unique identifier for the consensus run</param>
    /// <param name="message">The message to log</param>
    public static async Task WriteLogEntryAsync(string logDirectory, string runId, string message)
    {
        if (string.IsNullOrWhiteSpace(logDirectory))
            throw new ArgumentException("Log directory cannot be null or whitespace", nameof(logDirectory));
        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or whitespace", nameof(runId));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or whitespace", nameof(message));

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[INFO][{timestamp}] {message}";

        var logFilePath = Path.Combine(logDirectory, $"consensus-{runId}.log");

        // Ensure directory exists
        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Get or create semaphore for this file
        var semaphore = _fileSemaphores.GetOrAdd(logFilePath, _ => new SemaphoreSlim(MaxConcurrentWrites, MaxConcurrentWrites));

        await semaphore.WaitAsync();
        try
        {
            // Use async file I/O to prevent thread pool starvation
            await File.AppendAllTextAsync(logFilePath, logEntry + Environment.NewLine);
        }
        finally
        {
            semaphore.Release();
        }

        // Cleanup: Remove semaphore from dictionary if file is "old" (heuristic to prevent memory leak)
        // This is a simple cleanup strategy - in production you might use a more sophisticated approach
        // such as a background cleanup task or LRU cache
        if (_fileSemaphores.Count > 100) // Arbitrary threshold
        {
            CleanupOldSemaphores();
        }
    }

    /// <summary>
    /// Removes semaphores for files that haven't been accessed recently to prevent memory leaks.
    /// This is a simple heuristic - files that are being actively written to will be re-added.
    /// </summary>
    private static void CleanupOldSemaphores()
    {
        // Remove semaphores that currently have no waiters
        // This is safe because GetOrAdd will recreate them if needed
        var keysToRemove = _fileSemaphores
            .Where(kvp => kvp.Value.CurrentCount == MaxConcurrentWrites) // No one is using it
            .Select(kvp => kvp.Key)
            .Take(50) // Remove up to 50 at a time
            .ToList();

        foreach (var key in keysToRemove)
        {
            if (_fileSemaphores.TryRemove(key, out var semaphore))
            {
                semaphore?.Dispose();
            }
        }
    }

    /// <summary>
    /// Cleanup all semaphores (call on application shutdown)
    /// </summary>
    public static void Cleanup()
    {
        foreach (var kvp in _fileSemaphores)
        {
            kvp.Value?.Dispose();
        }
        _fileSemaphores.Clear();
    }
}
