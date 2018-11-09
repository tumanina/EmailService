namespace EmailService
{
    public interface IEmailService
    {
        EmailServiceType Type { get; }
        void SendEmail(string email, string subject, string body);
    }
}
