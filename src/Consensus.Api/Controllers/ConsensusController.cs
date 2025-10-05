using Consensus.Api.Jobs.Scheduling;
using Consensus.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Consensus.Api.Controllers;

/// <summary>
/// API controller for consensus building operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConsensusController : ControllerBase
{
    private readonly IJobScheduler _jobScheduler;
    private readonly ILogger<ConsensusController> _logger;

    public ConsensusController(
        IJobScheduler jobScheduler,
        ILogger<ConsensusController> logger)
    {
        _jobScheduler = jobScheduler;
        _logger = logger;
    }

    /// <summary>
    /// Get log entries for a specific run
    /// </summary>
    /// <param name="runId">The run ID</param>
    /// <returns>List of log entries</returns>
    [HttpGet("{runId}/logs")]
    [ProducesResponseType(typeof(IEnumerable<LogEntryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<LogEntryModel>>> GetLogs(string runId)
    {
        _logger.LogInformation("Getting logs for runId: {RunId}", runId);

        // Check if job exists
        var jobStatus = await _jobScheduler.GetJobStatusAsync(runId);
        if (jobStatus == null)
        {
            return NotFound(new { message = $"Run ID '{runId}' not found" });
        }

        // Return stubbed log data
        var logs = new List<LogEntryModel>
        {
            new() { Timestamp = DateTime.UtcNow.AddMinutes(-5), Message = $"[{runId}] Consensus job started" },
            new() { Timestamp = DateTime.UtcNow.AddMinutes(-4), Message = $"[{runId}] Agent 1 responded" },
            new() { Timestamp = DateTime.UtcNow.AddMinutes(-3), Message = $"[{runId}] Agent 2 responded" },
            new() { Timestamp = DateTime.UtcNow.AddMinutes(-2), Message = $"[{runId}] Agent 3 responded" },
            new() { Timestamp = DateTime.UtcNow.AddMinutes(-1), Message = $"[{runId}] Synthesis completed" }
        };

        return Ok(logs);
    }

    /// <summary>
    /// Get HTML output for a specific run
    /// </summary>
    /// <param name="runId">The run ID</param>
    /// <returns>HTML content</returns>
    [HttpGet("{runId}/html")]
    [Produces("text/html")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetHtml(string runId)
    {
        _logger.LogInformation("Getting HTML for runId: {RunId}", runId);

        // Check if job exists
        var jobStatus = await _jobScheduler.GetJobStatusAsync(runId);
        if (jobStatus == null)
        {
            return NotFound(new { message = $"Run ID '{runId}' not found" });
        }

        // Return stubbed HTML data
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Consensus Report - {runId}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        h1 {{ color: #333; }}
        .consensus {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
    </style>
</head>
<body>
    <h1>Consensus Report</h1>
    <p><strong>Run ID:</strong> {runId}</p>
    <div class='consensus'>
        <h2>Consensus Result</h2>
        <p>This is a stubbed HTML response for run {runId}.</p>
        <p>The actual consensus data will be displayed here.</p>
    </div>
</body>
</html>";

        return Content(html, "text/html");
    }

    /// <summary>
    /// Start a new consensus building job
    /// </summary>
    /// <param name="request">The prompt request containing the text to process</param>
    /// <returns>Job status information</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(JobStatusModel), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobStatusModel>> StartJob([FromBody] PromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Prompt))
        {
            return BadRequest(new { message = "Prompt text is required" });
        }

        // Generate a unique run ID
        var runId = Guid.NewGuid().ToString();
        
        _logger.LogInformation("Starting job for runId: {RunId} with prompt: {Prompt}", runId, request.Prompt);

        // Schedule the job with Quartz
        var scheduled = await _jobScheduler.ScheduleConsensusJobAsync(runId);
        
        if (!scheduled)
        {
            return Conflict(new { message = $"Job with run ID '{runId}' already exists" });
        }

        var jobStatus = await _jobScheduler.GetJobStatusAsync(runId);
        return AcceptedAtAction(nameof(GetJobStatus), new { runId }, jobStatus);
    }

    /// <summary>
    /// Get markdown output for a specific run
    /// </summary>
    /// <param name="runId">The run ID</param>
    /// <returns>Markdown content</returns>
    [HttpGet("{runId}/markdown")]
    [Produces("text/markdown")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetMarkdown(string runId)
    {
        _logger.LogInformation("Getting markdown for runId: {RunId}", runId);

        // Check if job exists
        var jobStatus = await _jobScheduler.GetJobStatusAsync(runId);
        if (jobStatus == null)
        {
            return NotFound(new { message = $"Run ID '{runId}' not found" });
        }

        // Return stubbed markdown data
        var markdown = $@"# Consensus Report

**Run ID:** {runId}

## Summary

This is a stubbed markdown response for run `{runId}`.

## Agent Responses

### Agent 1
Response from agent 1 would appear here.

### Agent 2
Response from agent 2 would appear here.

### Agent 3
Response from agent 3 would appear here.

## Consensus

The synthesized consensus would appear here.

## Points of Agreement

- Point 1
- Point 2
- Point 3

## Points of Disagreement

- Disagreement 1
- Disagreement 2
";

        return Content(markdown, "text/markdown");
    }

    /// <summary>
    /// Get the status of a job
    /// </summary>
    /// <param name="runId">The run ID</param>
    /// <returns>Job status information</returns>
    [HttpGet("{runId}/status")]
    [ProducesResponseType(typeof(JobStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobStatusModel>> GetJobStatus(string runId)
    {
        _logger.LogInformation("Getting status for runId: {RunId}", runId);

        var jobStatus = await _jobScheduler.GetJobStatusAsync(runId);
        if (jobStatus == null)
        {
            return NotFound(new { message = $"Run ID '{runId}' not found" });
        }

        return Ok(jobStatus);
    }
}
