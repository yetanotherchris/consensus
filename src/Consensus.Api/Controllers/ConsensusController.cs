using Consensus.Api.Jobs.Scheduling;
using Consensus.Api.Models;
using Consensus.Api.Services;
using Consensus.Configuration;
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
    private readonly IOutputFileReaderService _outputFileService;
    private readonly ILogReader _logReader;
    private readonly ILogger<ConsensusController> _logger;
    private readonly ConsensusConfiguration _configuration;

    public ConsensusController(
        IJobScheduler jobScheduler,
        IOutputFileReaderService outputFileService,
        ILogReader logReader,
        ILogger<ConsensusController> logger,
        ConsensusConfiguration configuration)
    {
        _jobScheduler = jobScheduler;
        _outputFileService = outputFileService;
        _logReader = logReader;
        _logger = logger;
        _configuration = configuration;
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
            return NotFound(new { message = $"ID '{runId}' not found" });
        }

        // Read HTML output file
        var html = await _outputFileService.ReadHtmlAsync(runId);
        if (html == null)
        {
            return NotFound(new { message = $"HTML output not found for ID '{runId}'. The job may still be running or may have failed." });
        }

        return Content(html, "text/html");
    }

    /// <summary>
    /// Start a new consensus building job
    /// </summary>
    /// <param name="request">The prompt request containing the text to process</param>
    /// <param name="cheatcode">Optional cheatcode to use alternative model set</param>
    /// <returns>Job status information</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(JobStatusModel), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobStatusModel>> StartJob([FromBody] PromptRequest request, [FromQuery] string? cheatcode = null)
    {
        if (string.IsNullOrWhiteSpace(request?.Prompt))
        {
            return BadRequest(new { message = "Prompt text is required" });
        }

        // Determine which models to use based on cheatcode
        string[] modelsToUse = _configuration.Models;

        // Only check cheatcode if it's configured in appsettings
        if (!string.IsNullOrWhiteSpace(_configuration.Cheatcode))
        {
            if (!string.IsNullOrWhiteSpace(cheatcode) && cheatcode == _configuration.Cheatcode)
            {
                if (_configuration.CheatcodeModels != null && _configuration.CheatcodeModels.Length > 0)
                {
                    modelsToUse = _configuration.CheatcodeModels;
                    _logger.LogInformation("Valid cheatcode provided, using cheatcode models");
                }
            }
            else if (!string.IsNullOrWhiteSpace(cheatcode))
            {
                _logger.LogWarning("Invalid cheatcode provided: {Cheatcode}", cheatcode);
            }
        }

        // Generate a unique run ID
        var runId = Guid.NewGuid().ToString();

        _logger.LogInformation("Starting job for runId: {RunId} with prompt: {Prompt}, models: {Models}",
            runId, request.Prompt, string.Join(", ", modelsToUse));

        // Schedule the job with Quartz
        var scheduled = await _jobScheduler.ScheduleConsensusJobAsync(runId, request.Prompt, modelsToUse);

        if (!scheduled)
        {
            return Conflict(new { message = $"Job with ID '{runId}' already exists" });
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
            return NotFound(new { message = $"ID '{runId}' not found" });
        }

        // Read markdown output file
        var markdown = await _outputFileService.ReadMarkdownAsync(runId);
        if (markdown == null)
        {
            return NotFound(new { message = $"Markdown output not found for ID '{runId}'. The job may still be running or may have failed." });
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
            return NotFound(new { message = $"ID '{runId}' not found" });
        }

        return Ok(jobStatus);
    }
}
