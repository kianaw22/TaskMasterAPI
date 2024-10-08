using System.ComponentModel.DataAnnotations;

namespace TaskMasterAPI.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        public string Title { get; set; }

        
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("Pending|Completed", ErrorMessage = "Status must be either 'Pending' or 'Completed'.")]
        public string Status { get; set; }  // E.g., Pending, Completed

        [Required(ErrorMessage = "AssignedUserId is required.")]
        public int AssignedUserId { get; set; }
    }
}