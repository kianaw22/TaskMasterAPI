using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMasterAPI.Models;
using TaskMasterAPI.services;
using TaskMasterAPI.Services;

namespace TaskMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
         private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService,ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger; 
        }
      
        // 1. User Registration (Open to anyone)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            try
            {
                // Map the DTO to the User model
                var user = new User
                {
                    Username = registerUserDto.Username,
                    PasswordHash = registerUserDto.Password,
                    // PasswordHash will be handled in the service (password hashing)
                };

                // Pass the User model to the service
                await _userService.RegisterUser(user, registerUserDto.Password);
                return StatusCode(201, new { Message = "User registered successfully." }); // 201 Created
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message }); // 400 Bad Request for client-side errors
            }
        }


        // 2. Authenticate user and return JWT token using DTO
        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = await _userService.Authenticate(loginDto.Username, loginDto.Password);
                if (token == null)
                {
                    return Unauthorized(new { Error = "Invalid credentials." }); // 401 Unauthorized
                }

                return Ok(new { Token = token }); // 200 OK for successful authentication
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Error = ex.Message }); // 401 Unauthorized for invalid login attempts
            }
        }

        // 3. Get user by ID (Only Admins or users accessing their own profile)
        [HttpGet("{id}")]
        [Authorize] 
        public async Task<IActionResult> GetUserById(int id)
        {
             _logger.LogInformation("GetUserById method called for User ID: {UserId}", id);
            try
            {
                var user = await _userService.GetUserById(id);
                return Ok(user); // 200 OK for successful retrieval
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = "User not found." }); // 404 Not Found
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Error = ex.Message }); // 403 Forbidden for unauthorized access
            }
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileDto profileDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("id")?.Value); // Fetch current user ID from JWT claims
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value; // Fetch role from JWT claims

                if (profileDto.UserId.HasValue && profileDto.UserId != currentUserId && currentUserRole != "Admin")
                {
                    return StatusCode(403, new { Error = "You are not allowed to update this profile." }); // 403 Forbidden
                }

                // Update profile (admins can pass role, others cannot)
                await _userService.UpdateUserProfile(profileDto.UserId ?? currentUserId, profileDto.Username, profileDto.CurrentPassword, profileDto.NewPassword, currentUserRole == "Admin" ? profileDto.Role : null);

                return Ok(new { Message = "Profile updated successfully." }); // 200 OK
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Error = ex.Message }); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message }); // 400 Bad Request
            }
        }

    [Authorize(Roles = "Admin")] // Only admins can access this endpoint
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteUser(id);
            return Ok(new { Message = "User deleted successfully." }); // 200 OK on successful deletion
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "User not found." }); // 404 Not Found if the user does not exist
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message }); // 400 Bad Request for client-side errors
        }
    }
  // DTOs for User registration and authentication
    public class RegisterUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }

    }
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }


    public class UpdateProfileDto
    {
        public int? UserId { get; set; } // Optional, used by Admin to specify which user's profile to update
        public string Username { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string Role { get; set; } // Only Admin can set this
    }
    }

}   