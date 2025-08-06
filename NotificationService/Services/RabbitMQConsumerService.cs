using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService.Services
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly IHubContext<NotificationHub> hubContext;
        private IConnection connection;
        private IModel channel;

        public RabbitMQConsumerService(IHubContext<NotificationHub> hubContext)
        {
            this.hubContext = hubContext;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: "task_events",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            };

            channel.BasicConsume(queue: "task_events",
                                 autoAck: true,
                                 consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
