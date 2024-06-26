﻿using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace worker.worker
{
    public class RabbitMqClient
    {
        private static RabbitMqClient _instance;
        private static readonly object _lock = new object();

        private readonly Lazy<IConnection> _lazyConnection;

        private RabbitMqClient()
        {

            _lazyConnection = new Lazy<IConnection>(() => CreateConnection());
        }

        public static RabbitMqClient GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new RabbitMqClient();
                    }
                }
            }
            return _instance;
        }

        public IConnection GetConnection()
        {
            return _lazyConnection.Value;
        }

        public void PublishMessage(string queueName, string message)
        {
            using (var channel = _lazyConnection.Value.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine($" [x] Sent '{message}'");
            }
        }

        private IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                // TODO: Get the default value from appsettings.json
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port =  int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "admin",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "admin"
            };

            return factory.CreateConnection();
        }
    }
}
