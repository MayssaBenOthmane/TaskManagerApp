using TaskService.Models;

namespace TaskService.Interfaces
{
    public interface ITaskRepository
    {
        List<TaskInfo> GetAll();
        TaskInfo GetById(Guid id);
        TaskInfo Add(TaskInfo task);
        bool Update(Guid id, TaskInfo updatedTask);
        bool Delete(Guid id);
    }
}
