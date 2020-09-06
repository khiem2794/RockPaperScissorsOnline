using System;
using System.Text.Json.Serialization;

namespace Profile.Models
{
    public class UserInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("username")]
        public string UserName { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("registeredAt")]
        public DateTime RegisteredAt { get; set; }
    }
}
