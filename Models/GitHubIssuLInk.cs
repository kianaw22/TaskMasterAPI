using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMasterAPI.Models
{
    public class GitHubIssueLink
    {
       
        public int Id { get; set; }  // Primary key

        [Required]
        [ForeignKey("TaskItem")]
        public int TaskId { get; set; }  // Links to a Task

        public TaskItem Task { get; set; }  // Navigation property to the TaskItem

        [Required]
        [Url]
        public string IssueUrl { get; set; }  // URL of the GitHub issue

        [Required]
        public string IssueNumber { get; set; }  // GitHub issue number

        public string IssueTitle { get; set; }  // The title of the GitHub issue

        public string IssueState { get; set; }  // The state of the issue (e.g., open, closed)

        public DateTime IssueCreatedAt { get; set; }  // When the GitHub issue was created

        public DateTime? IssueUpdatedAt { get; set; }  // Last update time for the GitHub issue (optional)
    }
}
public enum IssueState
{
    Open,
    Closed,
    InProgress,
    Resolved
}