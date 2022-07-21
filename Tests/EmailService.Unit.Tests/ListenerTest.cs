using System.Collections.Generic;
using System.Text;
using EmailService.Configuration;
using EmailService.Interfaces;
using EmailService.MessageBroker;
using EmailService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailService.Unit.Tests
{
    [TestClass]
    public class ListenerTest
    {
        private static readonly Mock<IConnectionFactory> ConnectionFactory = new Mock<IConnectionFactory>();
        private static readonly Mock<IConnection> Connection = new Mock<IConnection>();
        private static readonly Mock<IModel> Model = new Mock<IModel>();
        private static readonly Mock<IConsumerFactory> ConsumerFactory = new Mock<IConsumerFactory>();
        private static readonly Mock<IEmailService> EmailService1 = new Mock<IEmailService>();
        private static readonly Mock<IEmailService> EmailService2 = new Mock<IEmailService>();
        private static readonly Mock<ILogger<Listener>> Logger = new Mock<ILogger<Listener>>();
        private static readonly Mock<IOptions<ListenerConfiguration>> Configuration = new Mock<IOptions<ListenerConfiguration>>();

        [TestMethod]
        public void Run_ProcessorExists_ShouldRunAndListenMessages()
        {
            ResetCalls();

            var email = "test@test.com";
            var title = "invitation email";
            var body = "welcome onboard!";
            var queueName1 = string.Empty;
            bool? durable = null;
            bool? exclusive = null;
            bool? autoDelete = null;
            bool? exchangeDurable = null;
            bool? exchangeAutoDelete = null;
            var exchange = string.Empty;
            var messageBody = Encoding.UTF8.GetBytes("{" + $"\"email\": \"{email}\", \"title\": \"{title}\", \"body\": \"{body}\"" + "}");

            var queueName = "126_queue";
            var exchangeName = "exchange name";
            var processedEmail = string.Empty;
            var processedTitle = string.Empty;
            var processedBody = string.Empty;

            var consumer = new EventingBasicConsumer(Model.Object);
            ConsumerFactory.Setup(x => x.CreateEventConsumer(Model.Object)).Returns(consumer);

            Model.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                .Callback<string, bool, bool, bool, IDictionary<string, object>>((queueParam, durableParam, exclusiveParam, autoDeleteParam, param) =>
                {
                    queueName1 = queueParam;
                    durable = durableParam;
                    exclusive = exclusiveParam;
                    autoDelete = autoDeleteParam;
                });

            Model.Setup(x => x.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null))
                .Callback<string, string, bool, bool, IDictionary<string, object>>((exchangeParam, directParam, durableParam, autoDeleteParam, param) =>
                {
                    exchange = exchangeParam;
                    exchangeDurable = durableParam;
                    exchangeAutoDelete = autoDeleteParam;
                });
            Model.Setup(x => x.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null));

            Connection.Setup(x => x.CreateModel()).Returns(Model.Object);
            ConnectionFactory.Setup(x => x.CreateConnection()).Returns(Connection.Object);

            EmailService1.Setup(x => x.Provider).Returns(EmailProvider.SendGrid);
            EmailService1.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((emailParam, titleParam, bodyParam) =>
                {
                    processedEmail = emailParam;
                    processedTitle = titleParam;
                    processedBody = bodyParam;
                });
            EmailService2.Setup(x => x.Provider).Returns(EmailProvider.Elastic);
            EmailService2.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((emailParam, titleParam, bodyParam) =>
                {
                    processedEmail = emailParam;
                    processedTitle = titleParam;
                    processedBody = bodyParam;
                });

            var listener = new Listener(new List<IEmailService> { EmailService1.Object, EmailService2.Object }, Configuration.Object);

            listener.Run();

            consumer.HandleBasicDeliver("consumer tag", 1, false, exchangeName, exchangeName, null, messageBody);

            Assert.AreEqual(processedEmail, email);
            Assert.AreEqual(processedTitle, title);
            Assert.AreEqual(processedBody, body);
            Assert.AreEqual(durable, true);
            Assert.AreEqual(exclusive, false);
            Assert.AreEqual(autoDelete, false);
            Assert.AreEqual(exchange, exchangeName);
            Assert.AreEqual(exchangeDurable, true);
            Assert.AreEqual(exchangeAutoDelete, false);

            ConnectionFactory.Verify(x => x.CreateConnection(), Times.Once);
            Connection.Verify(x => x.CreateModel(), Times.Once);
            Model.Verify(x => x.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null), Times.Once);
            Model.Verify(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),It.IsAny<IDictionary<string, object>>()), Times.Once);
            Model.Verify(x => x.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null), Times.Once);
            Model.Verify(x => x.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once);
            EmailService1.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            EmailService2.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void Run_ServiceNotExisted_BreakAndDontReadMessagesNoException()
        {
            ResetCalls();

            var email = "test@test.com";
            var title = "invitation email";
            var body = "welcome onboard!";
            var queueName1 = string.Empty;
            bool? durable = null;
            bool? exclusive = null;
            bool? autoDelete = null;
            bool? exchangeDurable = null;
            bool? exchangeAutoDelete = null;
            var exchange = string.Empty;
            var messageBody = Encoding.UTF8.GetBytes("{" + $"\"email\": \"{email}\", \"title\": \"{title}\", \"body\": \"{body}\"" + "}");

            var queueName = "126_queue";
            var exchangeName = "exchange name";
            var processedEmail = string.Empty;
            var processedTitle = string.Empty;
            var processedBody = string.Empty;

            var consumer = new EventingBasicConsumer(Model.Object);
            ConsumerFactory.Setup(x => x.CreateEventConsumer(Model.Object)).Returns(consumer);

            Model.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                .Callback<string, bool, bool, bool, IDictionary<string, object>>((queueParam, durableParam, exclusiveParam, autoDeleteParam, param) =>
                {
                    queueName1 = queueParam;
                    durable = durableParam;
                    exclusive = exclusiveParam;
                    autoDelete = autoDeleteParam;
                });

            Model.Setup(x => x.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null))
                .Callback<string, string, bool, bool, IDictionary<string, object>>((exchangeParam, directParam, durableParam, autoDeleteParam, param) =>
                {
                    exchange = exchangeParam;
                    exchangeDurable = durableParam;
                    exchangeAutoDelete = autoDeleteParam;
                });
            Model.Setup(x => x.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null));

            Connection.Setup(x => x.CreateModel()).Returns(Model.Object);
            ConnectionFactory.Setup(x => x.CreateConnection()).Returns(Connection.Object);

            EmailService1.Setup(x => x.Provider).Returns(EmailProvider.SendGrid);
            EmailService1.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((emailParam, titleParam, bodyParam) =>
                {
                    processedEmail = emailParam;
                    processedTitle = titleParam;
                    processedBody = bodyParam;
                });
            EmailService2.Setup(x => x.Provider).Returns(EmailProvider.Gmail);
            EmailService2.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((emailParam, titleParam, bodyParam) =>
                {
                    processedEmail = emailParam;
                    processedTitle = titleParam;
                    processedBody = bodyParam;
                });

            var listener = new Listener(new List<IEmailService> { EmailService1.Object, EmailService2.Object }, Configuration.Object);

            listener.Run();
            consumer.HandleBasicDeliver("consumer tag", 1, false, exchangeName, exchangeName, null, messageBody);

            ConnectionFactory.Verify(x => x.CreateConnection(), Times.Never);
            Connection.Verify(x => x.CreateModel(), Times.Never);
            Model.Verify(x => x.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null), Times.Never);
            Model.Verify(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
            Model.Verify(x => x.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null), Times.Never);
            Model.Verify(x => x.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Never);
            EmailService1.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            EmailService2.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private void ResetCalls()
        {
            ConnectionFactory.Invocations.Clear();
            Connection.Invocations.Clear();
            Model.Invocations.Clear();
            ConsumerFactory.Invocations.Clear();
            EmailService1.Invocations.Clear();
            EmailService2.Invocations.Clear();
        }
    }
}
