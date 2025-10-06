using Consensus.Api.Jobs.Scheduling;
using Consensus.Api.Models;
using Consensus.Api.Services;
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
    private readonly IOutputFileService _outputFileService;
    private readonly ILogReader _logReader;
    private readonly ILogger<ConsensusController> _logger;

    public ConsensusController(
        IJobScheduler jobScheduler,
        IOutputFileService outputFileService,
        ILogReader logReader,
        ILogger<ConsensusController> logger)
    {
        _jobScheduler = jobScheduler;
        _outputFileService = outputFileService;
        _logReader = logReader;
        _logger = logger;
    }

    /// <summary>
    /// Get log entries for a specific run
    /// </summary>
    /// <param name="runId">The run ID</param>
    /// <returns>List of log entries</returns>
    [HttpGet("{runId}/logs")]
    [ProducesResponseType(typeof(IEnumerable<LogEntryModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LogEntryModel>>> GetLogs(string runId)
    {
        _logger.LogInformation("Getting logs for runId: {RunId}", runId);

        // Read logs from file
        // Note: FileLogReader will return an error entry if the log file doesn't exist
        var logs = await _logReader.ReadLogsAsync(runId);

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

        // Read HTML output file
        var html = await _outputFileService.ReadHtmlAsync(runId);
        if (html == null)
        {
            return NotFound(new { message = $"HTML output not found for run ID '{runId}'. The job may still be running or may have failed." });
        }

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
        var scheduled = await _jobScheduler.ScheduleConsensusJobAsync(runId, request.Prompt);
        
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

        // Read markdown output file
        var markdown = await _outputFileService.ReadMarkdownAsync(runId);
        if (markdown == null)
        {
            return NotFound(new { message = $"Markdown output not found for run ID '{runId}'. The job may still be running or may have failed." });
        }

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
