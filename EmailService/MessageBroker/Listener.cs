using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailService.MessageBroker
{
    public class Listener : IListener
    {
        private readonly IEmailService _service;
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _type;

        public Listener(IConnectionFactory connectionFactory, string queueName, string exchangeName, IEnumerable<IEmailService> services)
        {
            _queueName = queueName;
            _exchangeName = exchangeName;
            _connectionFactory = connectionFactory;

            _service = services.FirstOrDefault(t => t.Type == EmailServiceType.SendGrid);
        }

        public void Run()
        {
            if (_service != null)
            {
                var connection = _connectionFactory.CreateConnection();
                var channel = connection.CreateModel();

                channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, true);
                channel.QueueDeclare(_queueName, true, false, false, null);
                channel.QueueBind(_queueName, _exchangeName, _queueName, null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = JsonConvert.DeserializeObject<EmailMessage>(Encoding.UTF8.GetString(body));
                    _service.SendEmail(message.Email, message.Subject, message.Body);
                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume(_queueName, false, consumer);
            }
            else
            {
                throw new System.Exception($"Message proccesor for type '{_type}' not found");
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
