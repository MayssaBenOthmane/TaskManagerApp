using RabbitMQ.Client;
using System.Text;

namespace TaskService.Services;

public class RabbitMQPublisher
{
    private readonly IModel channel;

    public RabbitMQPublisher()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        channel = connection.CreateModel();
        channel.QueueDeclare(queue: "task_events", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publish(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: "", routingKey: "task_events", basicProperties: null, body: body);
    }
}
