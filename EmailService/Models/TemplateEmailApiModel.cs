namespace EmailService.Models
{
    public class TemplateEmailApiModel
    {
        public EmailProvider Provider { get; set; }
        public string Email { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, string>? TemplateParameters { get; set; }
    }
}