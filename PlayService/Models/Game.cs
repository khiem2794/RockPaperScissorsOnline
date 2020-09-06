using Play.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Play.Models
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
        public int WinnerId { get; set; }
        public Game()
        {
            Players = new List<Player>();
            GameId = Guid.NewGuid().ToString();

        }
        public void Initialize()
        {
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
        private Player GetPlayerFromConnectionId(string connectionId)
        {
            return Players.SingleOrDefault(p => p.User.ConnectionId == connectionId);
        }
        public void LeftGame(string connectionId)
        {
            if (State == GameState.Waiting) return;
            var leftPlayer = Players.Find(p => p.User.ConnectionId == connectionId);
            leftPlayer.LeftGame = true;
            WinnerId = Players.Find(p => p.User.ConnectionId != connectionId).User.Id;
            EndGame();
        }
        private void EndGame()
        {
            State = GameState.End;
        }
    }
}
