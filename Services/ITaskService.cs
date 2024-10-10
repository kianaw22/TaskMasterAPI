using TaskMasterAPI.Models;

namespace TaskMasterAPI.Services
{
    public interface ITaskService
    {
        Task<TaskItem> CreateTask(TaskItem task);        
        Task<IEnumerable<TaskItem>> GetTaskList();        
        Task<TaskItem> GetTaskById(int taskId);          
        Task UpdateTask(int taskId, TaskItem updatedTask); 
        Task DeleteTask(int taskId);                     
        Task AssignTask(int taskId, int assignToUserId); 
    }

    
}