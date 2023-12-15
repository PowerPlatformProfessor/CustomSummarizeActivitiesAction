using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CustomSummarizeActivitiesAction.DTO
{
    public class RequestPayload
    {
        [JsonPropertyName("@odatatype")]
        public string odatatype { get; set; }
        public string version { get; set; }
        public Conversation conversation { get; set; }
        [JsonPropertyName("kbarticles @odata.type")]
        public string kbarticlesodatatype { get; set; }
        public Kbarticle[] kbarticles { get; set; }
    }

    public class Conversation
    {
        [JsonPropertyName("@odatatype")]
        public string odatatype { get; set; }
        [JsonPropertyName("@messages@odata.type")]
        public string messagesodatatype { get; set; }
        public Message[] messages { get; set; }
    }

    public class Message
    {
        public string user { get; set; }
        public string text { get; set; }
        public DateTime datetime { get; set; }
    }

    public class Kbarticle
    {
        public string id { get; set; }
        public string extract { get; set; }
        public float relevance { get; set; }
        public string title { get; set; }
        public string source { get; set; }
    }
}


