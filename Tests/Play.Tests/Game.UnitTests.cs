using Play.Enum;
using Play.Models;
using Xunit;

namespace Play.FunctionalTests
{
    public class GameTests
    {
        private readonly IGame game;
        private readonly Player PlayerA;
        private readonly Player PlayerB;
        public GameTests()
        {
            game = new Game();
            PlayerA = new Player(new User("conn1", 1, "A"));
            PlayerB = new Player(new User("conn2", 2, "B"));
        }
        private void PrepareGame()
        {
            game.AddPlayer(PlayerA);
            game.AddPlayer(PlayerB);
        }
        [Fact]
        public void Add1Player_State_Should_Change_To_Waiting()
        {
            game.AddPlayer(PlayerA);
            Assert.Equal(GameState.Waiting, game.State);
        }
        [Fact]
        public void AddSamePlayer_State_Should_Not_Change()
        {
            game.AddPlayer(PlayerA);
            game.AddPlayer(PlayerA);
            Assert.Equal(GameState.Waiting, game.State);
        }
        [Fact]
        public void AddSamePlayer_Should_Not_Update()
        {
            game.AddPlayer(PlayerA);
            bool shouldUpdateGame = game.AddPlayer(PlayerA);
            Assert.False(shouldUpdateGame);
        }
        [Fact]
        public void Add2Player_Should_Change_State_To_Start()
        {
            game.AddPlayer(PlayerA);
            game.AddPlayer(PlayerB);
            Assert.Equal(GameState.Start, game.State);
        }
        [Fact]
        public void PlayHand_Once_State_Should_Not_Change()
        {
            PrepareGame();
            var state = game.State;
            game.PlayHand(PlayerA.User.ConnectionId, Hand.Paper);
            Assert.Equal(state, game.State);
        }
        [Fact]
        public void PlayHand_Same_Player_Cant_Play_Twice_Same_Round()
        {
            PrepareGame();
            game.PlayHand(PlayerA.User.ConnectionId, Hand.Paper);
            game.PlayHand(PlayerA.User.ConnectionId, Hand.Rock);
            Assert.Equal(Hand.Paper, PlayerA.PlayerHand);
        }
        [Fact]
        public void PlayHand_Twice_State_Change_To_Compare()
        {
            PrepareGame();
            game.PlayHand(PlayerA.User.ConnectionId, Hand.Paper);
            game.PlayHand(PlayerB.User.ConnectionId, Hand.Paper);
            Assert.Equal(GameState.Compare, game.State);
        }
        [Theory]
        [InlineData(Hand.Rock, Hand.Scissors)]
        [InlineData(Hand.Paper, Hand.Rock)]
        [InlineData(Hand.Scissors, Hand.Paper)]
        public void PlayHand_Winner_Should_Get_Point(Hand handA, Hand handB)
        {
            PrepareGame();
            int pointA = PlayerA.Point;
            game.PlayHand(PlayerA.User.ConnectionId, handA);
            game.PlayHand(PlayerB.User.ConnectionId, handB);
            Assert.Equal(GameState.Compare, game.State);
            Assert.Equal(pointA + 1, PlayerA.Point);
        }
        [Theory]
        [InlineData(Hand.Rock, Hand.Rock)]
        [InlineData(Hand.Paper, Hand.Paper)]
        [InlineData(Hand.Scissors, Hand.Scissors)]
        public void PlayerHand_B_and_A_Gain_Point_If_Draw(Hand handA, Hand handB)
        {
            PrepareGame();
            game.PlayHand(PlayerA.User.ConnectionId, handA);
            game.PlayHand(PlayerB.User.ConnectionId, handB);
            Assert.Equal(GameState.Compare, game.State);
            Assert.Equal(1, PlayerB.Point);
            Assert.Equal(PlayerA.Point, PlayerB.Point);
        }
        [Fact]
        public void StartRound_Should_Reset_Hand_and_State()
        {
            PrepareGame();
            game.PlayHand(PlayerA.User.ConnectionId, Hand.Rock);
            game.PlayHand(PlayerB.User.ConnectionId, Hand.Paper);
            game.StartRound();
            Assert.Equal(GameState.Start, game.State);
            Assert.Equal(Hand.Default, PlayerB.PlayerHand);
            Assert.Equal(Hand.Default, PlayerB.PlayerHand);
        }
        [Fact]
        public void PlayerA_Should_Win()
        {
            PrepareGame();
            while (game.State != GameState.End)
            {
                game.PlayHand(PlayerA.User.ConnectionId, Hand.Rock);
                game.PlayHand(PlayerB.User.ConnectionId, Hand.Scissors);
                game.StartRound();
            }
            Assert.Equal(GameState.End, game.State);
            Assert.Equal(PlayerA.User.Id, game.WinnerId);
        }
        [Fact]
        public void PlayerA_Should_Win_If_PlayerB_Left()
        {
            PrepareGame();
            while (game.State != GameState.End)
            {
                game.PlayHand(PlayerA.User.ConnectionId, Hand.Rock);
                game.PlayHand(PlayerB.User.ConnectionId, Hand.Scissors);
                game.StartRound();
                game.LeftGame(PlayerB.User.ConnectionId);
            }
            Assert.Equal(GameState.End, game.State);
            Assert.Equal(PlayerA.User.Id, game.WinnerId);
        }
    }
}
