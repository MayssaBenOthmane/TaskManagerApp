namespace TaskService.Models
{
    public class TaskInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
    }
}
