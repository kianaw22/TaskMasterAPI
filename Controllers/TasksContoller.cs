using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMasterAPI.Models;
using TaskMasterAPI.Services;
using Microsoft.Extensions.Logging;
using TaskMasterAPI.services;
using System.Security.Claims;

namespace TaskMasterAPI.Controllers
{
    [Authorize]  // Enforce authentication for all actions
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ITaskService taskService, IUserService userService, ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _userService = userService;
            _logger = logger;
        }

        // 1. Get Task by ID (Admin can access any task, users can access their own tasks)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetCurrentUserId();
            var role = await _userService.GetUserRole(userId);

            try
            {
                var task = await _taskService.GetTaskById(id);
                _logger.LogInformation("Task {TaskId} retrieved by User {UserId}", id, userId);
                return Ok(task);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = "Task not found." }); // 404 Not Found
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { Error = " 403 Forbidden" }); // 403 Forbidden
            }
        }

        // 2. Get All Tasks (Admin can get all tasks, users can get their own tasks)
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetCurrentUserId();
            var role = await _userService.GetUserRole(userId);

            var tasks = await _taskService.GetTaskList();
            _logger.LogInformation("Tasks retrieved for User {UserId}", userId);
            return Ok(tasks); // 200 OK
        }

        // 3. Create a Task (Anyone can create tasks)
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem taskItem)
        {
            var userId = GetCurrentUserId();

            try
            {
                taskItem.AssignedUserId = userId; // Set the task creator as the assigned user
                var createdTask = await _taskService.CreateTask(taskItem);
                _logger.LogInformation("Task {TaskId} created by User {UserId}", createdTask.Id, userId);
                return StatusCode(201, createdTask); // 201 Created
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating task: {Message}", ex.Message);
                return BadRequest(new { Error = ex.Message }); // 400 Bad Request
            }
        }

        // 4. Update Task (Admin can update any task, users can update their own tasks)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem updatedTask)
        {
            var userId = GetCurrentUserId();
            var role = await _userService.GetUserRole(userId);

            try
            {
                await _taskService.UpdateTask(id, updatedTask);
                _logger.LogInformation("Task {TaskId} updated by User {UserId}", id, userId);
                return Ok(new { Message = "Task updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = "Task not found." }); // 404 Not Found
            }
            catch (UnauthorizedAccessException )
            {
                return Unauthorized(new { Error = "403 Forbidden" }); // 403 Forbidden
            }
        }

        // 5. Delete Task (Admin can delete any task, users can delete their own tasks)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();
            var role = await _userService.GetUserRole(userId);

            try
            {
                await _taskService.DeleteTask(id);
                _logger.LogInformation("Task {TaskId} deleted by User {UserId}", id, userId);
                return Ok(new { Message = "Task deleted successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = "Task not found." }); // 404 Not Found
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message }); // 403 Forbidden
            }
        }

        // 6. Assign Task to another user (Any user can assign tasks, including admins)
        [HttpPost("{id}/assign/{assignedUserId}")]
        public async Task<IActionResult> AssignTask(int id, int assignedUserId)
        {
            var userId = GetCurrentUserId();

            try
            {
                await _taskService.AssignTask(id,  userId);
                _logger.LogInformation("Task {TaskId} assigned by User {UserId} to User {AssignedUserId}", id, userId, assignedUserId);
                return Ok(new { Message = $"Task {id} assigned to User {assignedUserId}." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = "Task or User not found." }); // 404 Not Found
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message }); // 403 Forbidden
            }
        }

        // Helper method to get the current authenticated user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id");

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return int.Parse(userIdClaim.Value);
        }
    }
}
