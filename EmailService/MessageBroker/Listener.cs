using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace EmailService.MessageBroker
{
    public class Listener : IListener
    {
        private readonly IEmailService _service;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConsumerFactory _consumerFactory;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly EmailServiceType _type;
        private readonly ILogger<Listener> _logger;

        public Listener(EmailServiceType type, IConnectionFactory connectionFactory, IConsumerFactory consumerFactory, string queueName, string exchangeName, IEnumerable<IEmailService> services, ILogger<Listener> logger)
        {
            _consumerFactory = consumerFactory;
            _queueName = queueName;
            _exchangeName = exchangeName;
            _connectionFactory = connectionFactory;
            _logger = logger;
            _type = type;

            _service = services.FirstOrDefault(t => t.Type == type);
        }

        public void Run()
        {
            _logger.LogError($"Listener for type '{_type}' have been started");
            if (_service != null)
            {
                var connection = _connectionFactory.CreateConnection();
                var channel = connection.CreateModel();

                channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, true);
                channel.QueueDeclare(_queueName, true, false, false, null);
                channel.QueueBind(_queueName, _exchangeName, _queueName, null);

                var consumer = _consumerFactory.CreateEventConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = JsonConvert.DeserializeObject<EmailMessage>(Encoding.UTF8.GetString(body));
                    _service.SendEmail(message.Email, message.Title, message.Body);
                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume(_queueName, false, consumer);
            }
            else
            {
                _logger.LogError($"Email service for type '{_type}' not found");
            }
        }

        public async Task RunAsync()
        {
            await Task.Run(() =>
            {
                Run();
            });
        }
    }
}
