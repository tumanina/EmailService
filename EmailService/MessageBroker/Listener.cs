using System.Text;
using EmailService.Configuration;
using EmailService.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailService.MessageBroker
{
    public class Listener : IListener
    {
        private readonly IEnumerable<IEmailService> _services;
        private readonly ListenerConfiguration _configuration;

        public Listener(IEnumerable<IEmailService> services, IOptions<ListenerConfiguration> configuration)
        {
            _services = services;
            _configuration = configuration.Value;
        }

        public void Run()
        {
            var channel = CreateChannel();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = JsonConvert.DeserializeObject<EmailMessage>(Encoding.UTF8.GetString(body.ToArray()));
                if (message != null)
                {
                    var service = _services.FirstOrDefault(s => s.Type == message.Type);
                    if (service != null)
                    {
                        service.SendEmail(message.Email, message.Subject, message.Body);
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        throw new Exception($"Email service for type '{message.Type}' not found");
                    }
                }
            };
            channel.BasicConsume(queue: _configuration.QueueName, autoAck: true, consumer: consumer);
        }

        public async Task RunAsync()
        {
            await Task.Run(() =>
            {
                Run();
            });
        }

        private IModel CreateChannel()
        {
            ConnectionFactory factory = new ConnectionFactory() 
            { 
                HostName = _configuration.Server.Host, 
                Port = _configuration.Server.Port
            };
            factory.UserName = _configuration.Server.UserName;
            factory.Password = _configuration.Server.Password;
            IConnection conn = factory.CreateConnection();
            IModel channel = conn.CreateModel();
            channel.QueueDeclare(queue: _configuration.QueueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            return channel;
        }
    }
}
