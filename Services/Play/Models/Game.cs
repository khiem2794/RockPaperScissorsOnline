using Play.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Play.Models
{
    public interface IGame
    {
        public string GameId { get; }
        public List<Player> Players { get; }
        public GameState State { get; }
        public int WinnerId { get; }
        bool AddPlayer(Player player);
        bool LeftGame(string connectionId);
        void PlayHand(string connectionId, Hand hand);
        bool HasPlayer(string connectionId);
        GameUpdate UpdateGame();
        void StartRound();
    }
    public class Game : IGame
    {
        public static readonly int MaxPlayers = 2;
        public static readonly int MaxRound = 3;
        public string GameId { get; set; }
        public GameState State { get; set; }
        public int Round { get; set; }
        public List<Player> Players { get; set; }
        public int WinnerId { get; set; }
        public Game()
        {
            Players = new List<Player>();
            GameId = Guid.NewGuid().ToString();
            State = GameState.Waiting;
            Round = 0;
        }

        public void StartRound()
        {
            if (State != GameState.End)
            {
                Round++;
                if (Round > MaxRound)
                {

                    if (Players[0].Point == Players[1].Point)
                    {
                        WinnerId = -1;
                    }
                    else
                    {
                        var winner = Players.First(p => p.Point == Players.Max(p => p.Point));
                        WinnerId = winner.User.Id;
                    }
                    EndGame();
                }
                else
                {
                    Players.ForEach(p => p.ResetHand());
                    State = GameState.Start;
                }
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
                if (Players[0].PlayerHand != Hand.Default && Players[1].PlayerHand != Hand.Default)
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
            }
            else if (player1.PlayerHand - player2.PlayerHand == 1 || player1.PlayerHand - player2.PlayerHand == -2)
            {
                player1.Point++;
            }
            else
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
            };
            update.PlayersState = Players.Select(p =>
            {

                var playerState = new GameUpdate.PlayerState
                {
                    Name = p.User.UserName,
                    Point = p.Point,
                    LeftGame = p.LeftGame,
                };
                if (State == GameState.Compare)
                {
                    playerState.Hand = p.PlayerHand;
                }
                return playerState;
            }).ToList();
            return update;
        }
        public bool HasPlayer(string connectionId)
        {
            return Players.Select(p => p.User.ConnectionId).Contains(connectionId);
        }
        private Player GetPlayerFromConnectionId(string connectionId)
        {
            return Players.SingleOrDefault(p => p.User.ConnectionId == connectionId);
        }
        public bool LeftGame(string connectionId)
        {
            if (State == GameState.Waiting || State == GameState.End) return false;
            var leftPlayer = Players.Find(p => p.User.ConnectionId == connectionId);
            leftPlayer.LeftGame = true;
            WinnerId = Players.Find(p => p.User.ConnectionId != connectionId).User.Id;
            EndGame();
            return true;
        }
        private void EndGame()
        {
            State = GameState.End;
        }

        public bool AddPlayer(Player player)
        {
            if (!this.Players.Contains(player) && this.Players.Count < Game.MaxPlayers && this.State == GameState.Waiting)
            {
                this.Players.Add(player);
                if (this.Players.Count == Game.MaxPlayers) StartRound();
                return true;
            }
            return false;
        }
    }
}
