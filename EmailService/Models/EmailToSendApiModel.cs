namespace EmailService.Models
{
    public class EmailToSendApiModel
    {
        public EmailServiceType Type { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}