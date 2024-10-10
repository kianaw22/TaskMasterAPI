using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskMasterAPI.Data;
using TaskMasterAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using TaskMasterAPI.services;
using TaskMasterAPI.Controllers;

namespace TaskMasterAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UsersController> _logger;


        public UserService(AppDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor,ILogger<UsersController> logger)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Helper method to get current user ID from claims
        public int GetCurrentUserId()
        {
        

         
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("id");
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return int.Parse(userIdClaim.Value);
        }

        // Helper method to get current user's role from claims
        public string GetCurrentUserRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                throw new UnauthorizedAccessException("Role claim is missing.");
            }

            return roleClaim.Value;
        }
        public async Task RegisterUser(User user, string password)
        {
            user.Role="User";
            // Hash the plain password before storing it
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            
            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

      
           

        // 2. Authenticate user and generate JWT token
        public async Task<string> Authenticate(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result != PasswordVerificationResult.Success)
                throw new UnauthorizedAccessException("Invalid credentials");

            return GenerateJwtToken(user);  // Return JWT token after successful authentication
        }

        // Generate JWT Token for authenticated user
        public string GenerateJwtToken(User user)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username), // Subject is the username
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token ID
             new Claim("id", user.Id.ToString() ), // User ID as NameIdentifier
              new Claim(ClaimTypes.Role, user.Role) // User role claim
                 });

            // Define the security key and credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: identity.Claims, // Use Claims from the identity
                expires: DateTime.UtcNow.AddMinutes(120), // Set expiration time
                signingCredentials: creds // Set the credentials for signing the token
            );

            // Return the generated token as a string
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        // 3. Get User by ID
        // Admin can access any user, regular users can access their own data
        public async Task<User> GetUserById(int userId)
        {
            var requesterId = GetCurrentUserId();
            var requesterRole = GetCurrentUserRole();

            if (requesterRole != "Admin" && userId != requesterId)
            {
                throw new UnauthorizedAccessException("You are not allowed to access this user's information.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return user;
        }

        // 4. Update User Profile
        // Users can update their own profile, Admins can update any user profile
        public async Task UpdateUserProfile(int userId, string username, string currentPassword, string newPassword, string role = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Update username
            if (!string.IsNullOrEmpty(username))
                user.Username = username;

            // Handle password change
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
                if (result == PasswordVerificationResult.Failed)
                    throw new UnauthorizedAccessException("Current password is incorrect");

                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            }

            // Only Admin can change the role
            if (!string.IsNullOrEmpty(role))
            {
                user.Role = role;
            }

            await _context.SaveChangesAsync();
        }

        // 5. Delete User (Admin Only)
        public async Task DeleteUser(int userId)
        {
            var requesterRole = GetCurrentUserRole();

            if (requesterRole != "Admin")
            {
                throw new UnauthorizedAccessException("Only admins can delete users.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
          public async Task<string> GetUserRole(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return user.Role; // Return the user's role
        }
    }
   
}
