using PlayService.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayService.Models
{
    public class Game
    {
        public static readonly int MaxPlayers = 2;
        public static readonly int MaxPoint = 3;
        public static readonly int MaxRound = 3;
        public string GameId { get; set; }
        public GameState State { get; set; }
        public int Round { get; set; }
        public List<Player> Players { get; set; }
        public bool PlayerLeftGame { get; set; }
        public Game()
        {
            Players = new List<Player>();
            GameId = Guid.NewGuid().ToString();
            
        }
        public void Initialize()
        {
            PlayerLeftGame = false;
            State = GameState.Waiting;
            Round = 0;
        }
        public void StartRound()
        {
            if (Round > MaxRound)
            {
                State = GameState.End;
            } else
            {
                Round++;
                Players.ForEach(p => p.ResetHand());
                State = GameState.Start;
            }
        }

        public void PlayHand(string connectionId, Hand hand)
        {
            if (State == GameState.Start)
            {
                var player = GetPlayerFromConnectionId(connectionId);
                if (player.PlayerHand == Hand.Default)
                {
                    player.PlayerHand = hand;
                }
                if (Players.FindAll(p => p.PlayerHand != Hand.Default).Count == MaxPlayers)
                {
                    CompareHand();
                }
            }
        }
        private void CompareHand()
        {
            State = GameState.Compare;
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
                Round = Round,
                PlayerLeftGame = PlayerLeftGame,
            };
            update.PlayersState = Players.Select(p =>
            {
                
                var playerState = new GameUpdate.PlayerState
                {
                    Name = p.User.UserName,
                    Point = p.Point,
                }; 
                if (State == GameState.Compare)
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
        public void LeftGame(string connectionId)
        {
            _ = Players.Remove(Players.Find(p => p.User.ConnectionId == connectionId));
            State = GameState.End;
            PlayerLeftGame = true;
        }
    }
}
