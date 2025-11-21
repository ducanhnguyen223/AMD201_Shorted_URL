using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration; // Add this for IConfiguration

namespace ShortenerService.Messaging
{
    public interface IRabbitMQPublisher
    {
        void PublishMessage<T>(T message, string exchangeName, string routingKey);
    }

    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            var rabbitMQHost = configuration["RabbitMQ:HostName"] ?? "localhost";
            var rabbitMQPort = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
            var rabbitMQUser = configuration["RabbitMQ:UserName"] ?? "guest";
            var rabbitMQPass = configuration["RabbitMQ:Password"] ?? "guest";
            var rabbitMQVHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMQHost,
                Port = rabbitMQPort,
                UserName = rabbitMQUser,
                Password = rabbitMQPass,
                VirtualHost = rabbitMQVHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void PublishMessage<T>(T message, string exchangeName, string routingKey)
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout); // Example: Fanout exchange

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _channel.BasicPublish(exchange: exchangeName,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
            GC.SuppressFinalize(this); // Suppress finalization for the disposed object
        }
    }
}