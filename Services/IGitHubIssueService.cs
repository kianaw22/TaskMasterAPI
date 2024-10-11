using System.Threading.Tasks;
using TaskMasterAPI.Models;

namespace TaskMasterAPI.Services
{
    public interface IGitHubIssueService
    {
        // Links a GitHub issue to a task
        Task<GitHubIssueLink> LinkGitHubIssueToTask(int taskId, string issueUrl);

        // Retrieves the linked GitHub issue for a task
        Task<GitHubIssueLink> GetGitHubIssue(int taskId);

        // (Optional) Fetch GitHub issue data directly from GitHub (could be internal to the service)
        Task<GitHubIssueData> FetchGitHubIssueData(string issueUrl);
    }
}
