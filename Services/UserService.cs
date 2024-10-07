using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskMasterAPI.Data;
using TaskMasterAPI.Models;
using TaskMasterAPI.services;

namespace TaskMasterAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // 1. User Registration with Password Hashing
        public async Task RegisterUser(User user)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // 2. User Authentication with Password Verification
        public async Task<User> Authenticate(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        // 3. Get User by ID with Role-Based Authorization
       
        public async Task<User> GetUserById(int userId, string role)
        {
        // Admins can access any user, regular users cannot access this endpoint
        if (role != "Admin")
        {
             throw new UnauthorizedAccessException("Only admins can access this information.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        return user;
}


        // 4. Update User Profile (Only for Admin or User's own profile)
        public async Task UpdateUserProfile(int userId, string role, string username, string currentPassword, string newPassword)
        {
            // Only allow users to update their own profile unless they are an admin
            if (role != "Admin")
            {
                throw new UnauthorizedAccessException("Only admins can update profiles of other users.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // Update username
            if (!string.IsNullOrEmpty(username))
                user.Username = username;

            // Handle password change
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
                if (result == PasswordVerificationResult.Failed)
                    throw new Exception("Current password is incorrect");

                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            }

            await _context.SaveChangesAsync();
        }

        // 5. Delete User (Only Admins can delete users)
        public async Task DeleteUser(int userId, string role)
        {
            if (role != "Admin")
                throw new UnauthorizedAccessException("Only admins can delete users.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

       
    }
}