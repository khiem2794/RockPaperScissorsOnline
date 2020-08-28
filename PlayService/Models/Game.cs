using PlayService.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayService.Models
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
        };
        public List<PlayerState> PlayersState { get; set; }
    }
    public class Game
    {
        public static readonly int MaxPlayers = 2;
        public static readonly int MaxPoint = 3;
        public static readonly int MaxRound = 3;
        public string GameId { get; set; }
        public GameState State { get; set; }
        public List<Player> Players { get; set; }
        public Game()
        {
            Players = new List<Player>();
            GameId = Guid.NewGuid().ToString();
            
        }
        public void Initialize()
        {
            State = GameState.WaitingPlayer;
        }
        public void StartGame()
        {
            State = GameState.Start;
        }
        public void EndGame()
        {
            State = GameState.End;
        }
        public void PlayHand(string connectionId, Hand hand)
        {
            var player = GetPlayerFromConnectionId(connectionId);
            if (player.PlayerHand == Hand.Default)
            {
                player.PlayerHand = hand;
            }
            if (Players.FindAll(p=>p.PlayerHand != Hand.Default).Count==MaxPlayers)
            {
                CompareHand();
            }
        }
        private void CompareHand()
        {
            State = GameState.CompareHand;
            var player1 = Players[0];
            var player2 = Players[1];
            if (player1.PlayerHand - player2.PlayerHand == 0)
            {
                player1.Point++;
                player2.Point++;
            } else if (player1.PlayerHand - player2.PlayerHand == 1  || player1.PlayerHand - player2.PlayerHand == -2)
            {
                player1.Point++;
            } else
            {
                player2.Point++;
            }
        }
        public GameUpdate UpdateGame()
        {
            GameUpdate update = new GameUpdate
            {
                GameId = GameId,
                State = State,
            };
            update.PlayersState = Players.Select(p =>
            {
                
                var playerState = new GameUpdate.PlayerState
                {
                    Name = p.User.UserName,
                    Point = p.Point,
                }; 
                if (State == GameState.CompareHand)
                {
                    playerState.Hand = p.PlayerHand;
                }
                return playerState;
            }).ToList();
            return update;
        }
        private Player GetPlayerFromConnectionId(string connectionId)
        {
            return Players.SingleOrDefault(p => p.User.ConnectionId == connectionId);
        }
    }
}
