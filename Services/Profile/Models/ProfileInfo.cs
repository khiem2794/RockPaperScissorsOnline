using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Profile.Models
{
    public class ProfileInfo
    {
        [JsonPropertyName("user")]
        public UserInfo User { get; set; }
        [JsonPropertyName("games")]
        public List<GameInfo> Games { get; set; }
    }
}
