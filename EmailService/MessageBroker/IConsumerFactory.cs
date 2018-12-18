using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailService.MessageBroker
{
    public interface IConsumerFactory
    {
        EventingBasicConsumer CreateEventConsumer(IModel model);
    }
}
