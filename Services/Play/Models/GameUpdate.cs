using Play.Enum;
using System.Collections.Generic;

namespace Play.Models
{
    public class GameUpdate
    {
        public string GameId { get; set; }
        public GameState State { get; set; }
        public struct PlayerState
        {
            public string Name { get; set; }
            public int Point { get; set; }
            public Hand Hand { get; set; }
            public bool LeftGame { get; set; }
        };
        public List<PlayerState> PlayersState { get; set; }
        public int Round { get; set; }
    }
}
