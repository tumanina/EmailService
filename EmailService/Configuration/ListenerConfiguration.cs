using EmailService.Configuration;

namespace MultiWalletWorker.Configuration
{
    public class ListenerConfiguration
    {
        public ServerConfiguration Server { get; set; }
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
    }
}
