using Microsoft.AspNetCore.SignalR;
using Moq;
using Play.Enum;
using Play.Hubs;
using Play.Models;
using Play.Models.PlayModel;
using Play.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Play.FunctionalTests
{
    public class HubFunctionalTests : IDisposable
    {
        private readonly Mock<IPlayService> mockPlayService;
        private readonly Mock<IHubCallerClients<IGameHub>> mockClients;
        private readonly Mock<IGameHub> mockClientProxy;
        private readonly Mock<HubCallerContext> mockHubCallerContext;
        private readonly GameHub hub;
        private readonly List<Message> messages;

        public HubFunctionalTests()
        {
            mockPlayService = new Mock<IPlayService>();
            mockClients = new Mock<IHubCallerClients<IGameHub>>();
            mockClientProxy = new Mock<IGameHub>();
            mockHubCallerContext = new Mock<HubCallerContext>();
            hub = new GameHub(mockPlayService.Object);
            messages = new List<Message>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
            mockClients.Setup(clients => clients.Caller).Returns(mockClientProxy.Object);
            mockClients.Setup(clients => clients.Clients(It.IsAny<List<string>>())).Returns(mockClientProxy.Object);
            mockClientProxy.Setup(m => m.MessageClient(Capture.In(messages)));
            hub.Clients = mockClients.Object;
            hub.Context = mockHubCallerContext.Object;
        }

        public async void Dispose()
        {
            SelectPlayerA();
            await hub.OnDisconnectedAsync(null);
            SelectPlayerB();
            await hub.OnDisconnectedAsync(null);
        }

        private void SelectPlayerA()
        {
            mockHubCallerContext.Setup(h => h.ConnectionId).Returns("PlayerA-connection");
            mockHubCallerContext.Setup(h => h.User.Identity.Name).Returns("PlayerA");
            mockHubCallerContext.Setup(h => h.User.Claims).Returns(new[]{
                new Claim(ClaimTypes.NameIdentifier, "1"),
            });
        }

        private void SelectPlayerB()
        {
            mockHubCallerContext.Setup(h => h.ConnectionId).Returns("PlayerB-connection");
            mockHubCallerContext.Setup(h => h.User.Identity.Name).Returns("PlayerB");
            mockHubCallerContext.Setup(h => h.User.Claims).Returns(new[]{
                new Claim(ClaimTypes.NameIdentifier, "2"),
            });
        }

        private async Task<string> PrepareGame()
        {
            SelectPlayerA();
            await hub.OnConnectedAsync();
            await hub.CreateGame();
            SelectPlayerB();
            await hub.OnConnectedAsync();
            await hub.JoinWaitingGame();
            var gameStartMsg = messages[^1];
            return gameStartMsg.Data.GameId;
        }

        [Fact]
        public async Task Player_Connected_Should_Receive_ConnectionEstablished()
        {
            SelectPlayerA();
            await hub.OnConnectedAsync();
            mockClientProxy.Verify(clientProxy => clientProxy.MessageClient(It.Is<Message>(m => m.MessageType == MessageType.ConnectionEstablished)), Times.Once);
        }

        [Fact]
        public async Task Player_CreateGame_Should_Receive_GameUpdate()
        {
            SelectPlayerA();
            await hub.OnConnectedAsync();
            await hub.CreateGame();
            mockClientProxy.Verify(clientProxy => clientProxy.MessageClient(It.Is<Message>(m => m.MessageType == MessageType.GameUpdate)), Times.Once);
        }

        [Fact]
        public async Task Player_JoinGame_Should_Receive_GameUpdate()
        {
            SelectPlayerA();
            await hub.OnConnectedAsync();
            await hub.CreateGame();
            SelectPlayerB();
            await hub.OnConnectedAsync();
            await hub.JoinWaitingGame();
            var gameStartMsg = messages[^1];
            Assert.Equal(4, messages.Count);
            Assert.Equal(GameState.Start, gameStartMsg.Data.State);
        }

        [Theory]
        [InlineData(Hand.Rock)]
        [InlineData(Hand.Paper)]
        [InlineData(Hand.Scissors)]
        public async Task Player_PlayHand_Should_Receive_GameUpdate_Start(Hand h)
        {
            var gameId = await PrepareGame();
            messages.Clear();
            SelectPlayerA();
            await hub.PlayHand(gameId, h);
            Assert.Single(messages);
            Assert.Equal(GameState.Start, messages[^1].Data.State);
        }

        [Theory]
        [InlineData(Hand.Rock, Hand.Paper)]
        [InlineData(Hand.Paper, Hand.Scissors)]
        [InlineData(Hand.Scissors, Hand.Rock)]
        public async Task PlayerAB_PlayHand_Should_Receive_GameUpdate_Compare_and_Start(Hand hA, Hand hB)
        {
            var gameId = await PrepareGame();
            messages.Clear();
            SelectPlayerA();
            await hub.PlayHand(gameId, hA);
            SelectPlayerB();
            await hub.PlayHand(gameId, hB);
            Assert.True(messages.Count > 2);
            Assert.Equal(GameState.Compare, messages[messages.Count - 2].Data.State);
            Assert.Equal(GameState.Start, messages[^1].Data.State);
        }

        [Theory]
        [InlineData(Hand.Rock, Hand.Paper)]
        [InlineData(Hand.Paper, Hand.Rock)]
        [InlineData(Hand.Scissors, Hand.Scissors)]
        public async Task GameEnd_Should_Receive_GameUpdate_End(Hand hA, Hand hB)
        {
            var gameId = await PrepareGame();
            messages.Clear();
            var round = 1;
            while (round <= Game.MaxRound)
            {
                SelectPlayerA();
                await hub.PlayHand(gameId, hA);
                SelectPlayerB();
                await hub.PlayHand(gameId, hB);
                round++;
            }
            Assert.Equal(GameState.End, messages[^1].Data.State);
        }

        [Theory]
        [InlineData(Hand.Rock, Hand.Paper)]
        [InlineData(Hand.Paper, Hand.Rock)]
        [InlineData(Hand.Scissors, Hand.Scissors)]
        public async Task LeftGame_Should_Receive_GameUpdate_End(Hand hA, Hand hB)
        {
            var gameId = await PrepareGame();
            messages.Clear();
            SelectPlayerA();
            await hub.PlayHand(gameId, hA);
            SelectPlayerB();
            await hub.LeftGame(gameId);
            Assert.Equal(GameState.End, messages[^1].Data.State);
        }

        [Theory]
        [InlineData(Hand.Rock, Hand.Paper)]
        [InlineData(Hand.Paper, Hand.Rock)]
        [InlineData(Hand.Scissors, Hand.Scissors)]
        public async Task GameEnd_Should_Save_Data(Hand hA, Hand hB)
        {
            var gameId = await PrepareGame();
            messages.Clear();
            var round = 1;
            while (round <= Game.MaxRound)
            {
                SelectPlayerA();
                await hub.PlayHand(gameId, hA);
                SelectPlayerB();
                await hub.PlayHand(gameId, hB);
                round++;
            }
            Assert.Equal(GameState.End, messages[^1].Data.State);
            mockPlayService.Verify(s => s.SaveGameAsync(It.IsAny<PlayData>()));
        }
    }
}
