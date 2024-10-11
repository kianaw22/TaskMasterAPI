using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskMasterAPI.Models;
using TaskMasterAPI.Services;
using Microsoft.Extensions.Logging;

namespace TaskMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubIssueController : ControllerBase
    {
        private readonly IGitHubIssueService _gitHubIssueService;
        private readonly ILogger<GitHubIssueController> _logger;

        public GitHubIssueController(IGitHubIssueService gitHubIssueService, ILogger<GitHubIssueController> logger)
        {
            _gitHubIssueService = gitHubIssueService;
            _logger = logger;
        }

        // 1. Link GitHub Issue to Task
        [HttpPost("link")]
        public async Task<IActionResult> LinkGitHubIssueToTask([FromBody] LinkGitHubIssueDto linkDto)
        {
            try
            {
                // Log the action
                _logger.LogInformation("Linking GitHub issue {IssueUrl} to Task ID {TaskId}", linkDto.IssueUrl, linkDto.TaskId);

                var issueLink = await _gitHubIssueService.LinkGitHubIssueToTask(linkDto.TaskId, linkDto.IssueUrl);

                return Ok(issueLink); // Return the issue link object
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Task not found for Task ID {TaskId}", linkDto.TaskId);
                return NotFound(new { Error = ex.Message }); // 404 Not Found
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error linking GitHub issue to task");
                return StatusCode(500, new { Error = "An error occurred while linking the GitHub issue." }); // 500 Internal Server Error
            }
        }

        // 2. Get Linked GitHub Issue for a Task
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetGitHubIssueForTask(int taskId)
        {
            try
            {
                _logger.LogInformation("Fetching GitHub issue for Task ID {TaskId}", taskId);

                var issueLink = await _gitHubIssueService.GetGitHubIssue(taskId);

                return Ok(issueLink); // Return the GitHub issue link object
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("GitHub issue not found for Task ID {TaskId}", taskId);
                return NotFound(new { Error = ex.Message }); // 404 Not Found
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error fetching GitHub issue for task");
                return StatusCode(500, new { Error = "An error occurred while fetching the GitHub issue." }); // 500 Internal Server Error
            }
        }
    }

    // DTO for linking GitHub issue to task
    public class LinkGitHubIssueDto
    {
        public int TaskId { get; set; }      // ID of the task to link the issue to
        public string IssueUrl { get; set; } // URL of the GitHub issue
    }
}
