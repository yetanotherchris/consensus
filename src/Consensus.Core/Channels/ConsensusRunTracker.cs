using System.Threading.Channels;
using Consensus.Logging;
using Microsoft.Extensions.Logging;

namespace Consensus.Channels;

/// <summary>
/// Singleton service that manages channel-based logging for consensus runs.
/// Listens to log messages and writes them to per-run log files.
/// Implements IDisposable for proper resource cleanup.
/// </summary>
public class ConsensusRunTracker : IDisposable
{
    private readonly Channel<LogMessage> _channel;
    private readonly Task _listenerTask;
    private readonly ILogger<ConsensusRunTracker> _logger;
    private readonly string _logDirectory;
    private bool _disposed;

    public ConsensusRunTracker(ILogger<ConsensusRunTracker> logger, string logDirectory)
    {
        _logger = logger;
        _logDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));

        // Create unbounded channel for log messages
        _channel = Channel.CreateUnbounded<LogMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false // Multiple components can write
        });

        // Start background listener task
        _listenerTask = Task.Run(async () => await ListenToChannelAsync());
    }

    /// <summary>
    /// Writes a log message for a specific consensus run
    /// </summary>
    /// <param name="runId">The unique identifier for the consensus run</param>
    /// <param name="message">The message to log</param>
    public async Task WriteLogAsync(string runId, string message)
    {
        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or whitespace", nameof(runId));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or whitespace", nameof(message));

        if (_disposed)
        {
            _logger.LogWarning("Attempted to write log after disposal for runId: {RunId}", runId);
            return;
        }

        var logMessage = new LogMessage(runId, message);
        await _channel.Writer.WriteAsync(logMessage);
    }

    /// <summary>
    /// Synchronous version of WriteLogAsync for convenience
    /// </summary>
    public void WriteLog(string runId, string message)
    {
        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or whitespace", nameof(runId));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or whitespace", nameof(message));

        if (_disposed)
        {
            _logger.LogWarning("Attempted to write log after disposal for runId: {RunId}", runId);
            return;
        }

        var logMessage = new LogMessage(runId, message);
        if (!_channel.Writer.TryWrite(logMessage))
        {
            _logger.LogWarning("Failed to write log message to channel for runId: {RunId}. Message: {Message}", runId, message);
        }
    }

    /// <summary>
    /// Background task that continuously reads from the channel and writes to log files
    /// </summary>
    private async Task ListenToChannelAsync()
    {
        await foreach (var logMessage in _channel.Reader.ReadAllAsync())
        {
            try
            {
                // Write to file using static logger method
                await ConsensusFileLogger.WriteLogEntryAsync(_logDirectory, logMessage.RunId, logMessage.Message);
            }
            catch (Exception ex)
            {
                // If file writing fails, log to ILogger to avoid losing the message
                _logger.LogError(ex, "Failed to write log for run {RunId}. Original message: {Message}",
                    logMessage.RunId, logMessage.Message);
            }
        }
    }

    /// <summary>
    /// Dispose of resources and ensure all pending logs are written
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            // Signal that no more messages will be written
            _channel.Writer.Complete();

            // Wait for the listener task to finish processing remaining messages
            // Use a reasonable timeout to prevent hanging on shutdown
            if (!_listenerTask.Wait(TimeSpan.FromSeconds(10)))
            {
                _logger.LogWarning("Listener task did not complete within timeout during disposal");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ConsensusRunTracker disposal");
        }
    }
}
