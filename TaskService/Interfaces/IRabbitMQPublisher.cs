namespace TaskService.Interfaces
{
    public interface IRabbitMQPublisher
    {
        void Publish(string message);
    }
}
