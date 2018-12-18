using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailService.MessageBroker
{
    public class ConsumerFactory : IConsumerFactory
    {
        public EventingBasicConsumer CreateEventConsumer(IModel model)
        {
            return new EventingBasicConsumer(model);
        }
    }
}
