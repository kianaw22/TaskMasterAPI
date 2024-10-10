using TaskMasterAPI.Models;

namespace TaskMasterAPI.services
{
    public interface IUserService
    {
          Task<string> GetUserRole(int userId);
        Task RegisterUser(User user, string password);
        Task<string> Authenticate(string username, string password);
        Task<User> GetUserById(int userId);
        Task UpdateUserProfile(int id,string username, string currentPassword, string newPassword,string role=null);
        Task DeleteUser(int userId);

        string GenerateJwtToken(User user);
    }
    
}