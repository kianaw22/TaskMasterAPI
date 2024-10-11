using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaskMasterAPI.Models;
using TaskMasterAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;
using System;

namespace TaskMasterAPI.Services
{
    public class GitHubIssueService : IGitHubIssueService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubIssueService> _logger;

        public GitHubIssueService(AppDbContext context, IHttpClientFactory httpClientFactory, ILogger<GitHubIssueService> logger)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient(); // Use HttpClientFactory
            _logger = logger;
        }

        public async Task<GitHubIssueLink> LinkGitHubIssueToTask(int taskId, string issueUrl)
        {
            // Step 1: Validate the Task Exists
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                throw new KeyNotFoundException("Task not found.");
            }

            // Step 2: Call the GitHub API to get issue details
            var issueData = await FetchGitHubIssueData(issueUrl);
            if (issueData == null)
            {
                throw new Exception("Unable to fetch GitHub issue data.");
            }

            // Step 3: Link the issue to the task
            // Ensure issue state is parsed correctly from the GitHub API response
            var parsedSuccessfully = Enum.TryParse<IssueState>(issueData.State, true, out var state);
            var issueState = parsedSuccessfully ? state : IssueState.Open;  // Default to Open if parsing fails

            var gitHubIssueLink = new GitHubIssueLink
            {
                TaskId = taskId,
                IssueUrl = issueUrl,
                IssueNumber = issueData.Number,
                IssueTitle = issueData.Title,
                IssueState = issueState.ToString(), // Store the enum value
                IssueCreatedAt = issueData.CreatedAt,
                IssueUpdatedAt = issueData.UpdatedAt
            };

            _context.GitHubIssueLinks.Add(gitHubIssueLink);
            await _context.SaveChangesAsync();

            return gitHubIssueLink;
        }

        // Method to retrieve an existing linked GitHub issue
        public async Task<GitHubIssueLink> GetGitHubIssue(int taskId)
        {
            var issueLink = await _context.GitHubIssueLinks.FirstOrDefaultAsync(i => i.TaskId == taskId);
            if (issueLink == null)
            {
                throw new KeyNotFoundException("GitHub issue link not found for the specified task.");
            }

            return issueLink;
        }

       

        // Fetch GitHub issue details using the GitHub API
        public async Task<GitHubIssueData> FetchGitHubIssueData(string issueUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(issueUrl);

                if (response.StatusCode == HttpStatusCode.Forbidden && response.Headers.Contains("X-RateLimit-Remaining"))
                {
                    // Handle rate limiting
                    _logger.LogWarning("Rate limit exceeded for GitHub API.");
                    throw new Exception("GitHub API rate limit exceeded. Try again later.");
                }

                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var issueData = JsonSerializer.Deserialize<GitHubIssueData>(jsonContent);

                return issueData;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching GitHub issue data.");
                throw new Exception("Error fetching GitHub issue data from GitHub.");
            }
        }
    }

    // DTO
    public  class GitHubIssueData
    {
        public string Number { get; set; }    // Issue number
        public string Title { get; set; }     // Issue title
        public string State { get; set; }     // Issue state (open, closed)
        public DateTime CreatedAt { get; set; }  // Issue creation date
        public DateTime? UpdatedAt { get; set; } // Issue update date
    }
}
