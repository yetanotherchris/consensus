using System.Threading.Channels;
using Consensus.Logging;
using Microsoft.Extensions.Logging;

namespace Consensus.Channels;

/// <summary>
/// Singleton service that manages channel-based logging for consensus runs.
/// Listens to log messages and writes them to per-run log files.
/// </summary>
public class ConsensusRunTracker
{
    private readonly Channel<LogMessage> _channel;
    private readonly Task _listenerTask;
    private readonly ILogger<ConsensusRunTracker> _logger;

    public ConsensusRunTracker(ILogger<ConsensusRunTracker> logger)
    {
        _logger = logger;
        
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
        var logMessage = new LogMessage(runId, message);
        await _channel.Writer.WriteAsync(logMessage);
    }

    /// <summary>
    /// Synchronous version of WriteLogAsync for convenience
    /// </summary>
    public void WriteLog(string runId, string message)
    {
        var logMessage = new LogMessage(runId, message);
        _channel.Writer.TryWrite(logMessage);
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
                SimpleFileLogger.WriteLogEntry(logMessage.RunId, logMessage.Message);
            }
            catch (Exception ex)
            {
                // If file writing fails, log to ILogger to avoid losing the message
                _logger.LogError(ex, "Failed to write log for run {RunId}. Original message: {Message}", 
                    logMessage.RunId, logMessage.Message);
            }
        }
    }
}
