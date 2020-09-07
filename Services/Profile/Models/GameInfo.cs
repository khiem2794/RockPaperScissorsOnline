using System;
using System.Text.Json.Serialization;

namespace Profile.Models
{
    public class GameInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("user1Id")]
        public int User1Id { get; set; }
        [JsonPropertyName("user2Id")]
        public int User2Id { get; set; }
        [JsonPropertyName("winnerId")]
        public int WinnerId { get; set; }
        [JsonPropertyName("gameDate")]
        public DateTime GameDate { get; set; }
    }
}
