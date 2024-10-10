using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskMasterAPI.Data;
using TaskMasterAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TaskMasterAPI.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to get current user ID from claims
        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("id");
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return int.Parse(userIdClaim.Value);
        }

        // Helper method to get current user's role from claims
        private string GetCurrentUserRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                throw new UnauthorizedAccessException("Role claim is missing.");
            }

            return roleClaim.Value;
        }

        // 1. Create a Task (Any user can create tasks)
        public async Task<TaskItem> CreateTask(TaskItem task)
        {
            task.AssignedUserId = GetCurrentUserId();  // Automatically assign the task to the creator
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        // 2. Get All Tasks (Admin gets all tasks, users get only their own tasks)
        public async Task<IEnumerable<TaskItem>> GetTaskList()
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (role == "Admin")
            {
                return await _context.Tasks.ToListAsync();  // Admin can see all tasks
            }
            else
            {
                return await _context.Tasks.Where(t => t.AssignedUserId == userId).ToListAsync();  // Users see only their own tasks
            }
        }

        // 3. Get Task by ID (Admin can access any task, users can access only their own)
        public async Task<TaskItem> GetTaskById(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (role != "Admin" && task.AssignedUserId != userId)
            {
                throw new UnauthorizedAccessException("You cannot access this task.");
            }

            return task;
        }

        // 4. Update Task (Admin or task owner can update)
        public async Task UpdateTask(int taskId, TaskItem updatedTask)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (role != "Admin" && task.AssignedUserId != userId)
            {
                throw new UnauthorizedAccessException("You cannot update this task.");
            }

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Status = updatedTask.Status;

            await _context.SaveChangesAsync();
        }

        // 5. Delete Task (Admin or task owner can delete)
        public async Task DeleteTask(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (role != "Admin" && task.AssignedUserId != userId)
            {
                throw new UnauthorizedAccessException("You cannot delete this task.");
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        // 6. Assign Task (Users can assign tasks they own, Admins can assign any task)
        public async Task AssignTask(int taskId, int assignToUserId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (role != "Admin" && task.AssignedUserId != userId)
            {
                throw new UnauthorizedAccessException("You cannot assign this task.");
            }

            task.AssignedUserId = assignToUserId;
            await _context.SaveChangesAsync();
        }
    }
}
