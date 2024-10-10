using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace TaskMasterAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]   
        public string PasswordHash { get; set; }

        
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("Admin|User", ErrorMessage = "Role must be either 'Admin' or 'User'.")]
        public string Role { get; set; }
    }
}