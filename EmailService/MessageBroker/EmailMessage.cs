using Newtonsoft.Json;

namespace EmailService.MessageBroker
{
    public class EmailMessage
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
