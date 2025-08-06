using System.Text.Json;
using TaskService.Models;

namespace TaskService.Services
{
    public class TaskRepository
    {
        private readonly string file = "Data/tasks.json";
        private List<TaskInfo> tasks;

        public TaskRepository()
        {
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                tasks = JsonSerializer.Deserialize<List<TaskInfo>>(json) ?? new List<TaskInfo>();
            }
            else
            {
                tasks = new List<TaskInfo>();
            }
        }

        private void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(file, json);
        }

        public List<TaskInfo> GetAll()
        {
            return tasks;
        }

        public TaskInfo GetById(Guid id)
        {
            foreach (var task in tasks)
            {
                if (task.Id == id)
                {
                    return task;
                }
            }
            return null;
        }

        public TaskInfo Add(TaskInfo task)
        {
            tasks.Add(task);
            Save();
            return task;
        }

        public bool Update(Guid id, TaskInfo updatedTask)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].Id == id)
                {
                    tasks[i].Title = updatedTask.Title;
                    tasks[i].Description = updatedTask.Description;
                    tasks[i].Done = updatedTask.Done;
                    Save();
                    return true;
                }
            }
            return false;
        }

        public bool Delete(Guid id)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].Id == id)
                {
                    tasks.RemoveAt(i);
                    Save();
                    return true;
                }
            }
            return false;
        }
    }
}
