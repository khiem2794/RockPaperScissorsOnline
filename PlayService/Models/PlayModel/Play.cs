using PlayService.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayService.Models.PlayModel
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
