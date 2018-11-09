using System.Threading.Tasks;

namespace EmailService.MessageBroker
{
    public interface IListener
    {
        void Run();
        Task RunAsync();
    }
}
