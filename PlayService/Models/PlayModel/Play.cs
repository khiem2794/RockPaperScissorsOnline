using System;

namespace Play.Models.PlayModel
{
    public class PlayData
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        public int WinnerId { get; set; }
        public DateTime GameDate { get; set; }
    }
}
