using TaskMasterAPI.Models;

namespace TaskMasterAPI.services
{
    public interface IUserService
    {
        Task RegisterUser(User user);
        Task<User> Authenticate(string username, string password);
        Task<User> GetUserById(int userId, string role);
        Task UpdateUserProfile(int userId, string role, string username, string currentPassword, string newPassword);
        Task DeleteUser(int userId, string role);
    }
    
}