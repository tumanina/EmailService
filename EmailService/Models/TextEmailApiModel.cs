namespace EmailService.Models
{
    public class TextEmailApiModel
    {
        public EmailProvider Provider { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}