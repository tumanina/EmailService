﻿using Newtonsoft.Json;

namespace EmailService.MessageBroker
{
    public class EmailMessage
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
