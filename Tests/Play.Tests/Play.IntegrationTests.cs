using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Play.AuthConfig;
using Play.Enum;
using Play.Hubs;
using Play.Models.PlayModel;
using Play.Services;
using Play.Tests;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Play.FunctionalTests
{
    public class FakePlayService : IPlayService
    {
        public List<PlayData> GetUserPlayData(int userId)
        {
            return null;
        }

        public Task SaveGameAsync(PlayData data)
        {
            return Task.CompletedTask;
        }
    }
    public class HubIntegrationTests : IDisposable
    {
        public readonly static JwtTokenConfig jwtSampleConfig = new JwtTokenConfig { Audience = "https://sample.com", Issuer = "https://sample.com", Secret = "Z0O1234561234567" };
        private readonly TestServer server;
        private HubConnection playerA;
        private HubConnection playerB;
        private List<Message> playerAreceivedMsg = new List<Message>();
        private List<Message> playerBreceivedMsg = new List<Message>();
        public HubIntegrationTests()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<PlayTestsStartup>()
                .ConfigureTestServices(services =>
                {
                    services.AddScoped<IPlayService, FakePlayService>();
                });

            server = new TestServer(webHostBuilder);
            playerA = CreateClient(1, "playerA");
            playerB = CreateClient(2, "playerB");
            playerA.On<object>("MessageClient", msg =>
            {
                playerAreceivedMsg.Add(JsonConvert.DeserializeObject<Message>(msg.ToString()));
            });
            playerB.On<object>("MessageClient", msg =>
            {
                playerBreceivedMsg.Add(JsonConvert.DeserializeObject<Message>(msg.ToString()));
            });
        }
        private HubConnection CreateClient(int id, string name)
        {
            var claims = new Claim[] {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            };
            var jwtToken = new JwtSecurityToken(
                jwtSampleConfig.Issuer,
                jwtSampleConfig.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSampleConfig.Secret)), SecurityAlgorithms.HmacSha256Signature));
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl(server.BaseAddress.ToString() + "gamehub", o =>
                  {
                      o.AccessTokenProvider = () => Task.FromResult(accessToken);
                      o.HttpMessageHandlerFactory = _ => server.CreateHandler();
                  })
                .Build();
            return connection;
        }

        public async void Dispose()
        {
            await playerA.StopAsync();
            await playerB.StopAsync();
            playerAreceivedMsg.Clear();
            playerBreceivedMsg.Clear();
        }

        private async Task<string> PrepareGame()
        {
            await playerA.StartAsync();
            await playerB.StartAsync();
            await playerA.InvokeAsync("CreateGame");
            await Task.Delay(500);
            string gameId = playerAreceivedMsg[^1].Data.GameId;
            await playerB.InvokeAsync("JoinGame", gameId);
            return gameId;
        }

        [Fact]
        public async Task Connected_Should_Return_GameUpdate_ConnectionEstablished()
        {
            await playerA.StartAsync();
            await Task.Delay(100);
            Assert.Equal(MessageType.ConnectionEstablished, playerAreceivedMsg[^1].MessageType);
        }

        [Fact]
        public async Task CreateGame_Should_Return_GameUpdate_Waiting()
        {
            await playerA.StartAsync();
            await playerA.InvokeAsync("CreateGame");
            await Task.Delay(500);
            Assert.Equal(2, playerAreceivedMsg.Count);
            Assert.Equal(GameState.Waiting, playerAreceivedMsg[^1].Data.State);
        }

        [Fact]
        public async Task JoinGame_Should_Return_GameUpdate_Start()
        {
            await playerA.StartAsync();
            await playerB.StartAsync();
            await playerA.InvokeAsync("CreateGame");
            await playerB.InvokeAsync("JoinWaitingGame");
            await Task.Delay(500);
            Assert.Equal(3, playerAreceivedMsg.Count);
            Assert.Equal(2, playerBreceivedMsg.Count);
            Assert.Equal(GameState.Start, playerAreceivedMsg[^1].Data.State);
            Assert.Equal(GameState.Start, playerBreceivedMsg[^1].Data.State);
        }

        [Fact]
        public async Task One_Player_PlayHand_Should_Return_GameUpdate_Start()
        {
            string gameId = await PrepareGame();
            await playerA.InvokeAsync("PlayHand", gameId, Hand.Paper);
            await Task.Delay(500);
            Assert.Equal(GameState.Start, playerAreceivedMsg[^1].Data.State);
            Assert.Equal(GameState.Start, playerBreceivedMsg[^1].Data.State);
        }

        [Theory]
        [InlineData(Hand.Rock, Hand.Paper)]
        public async Task Two_Player_PlayHand_Should_Return_GameUpdate_Compare_Then_Start(Hand hA, Hand hB)
        {
            string gameId = await PrepareGame();
            await Task.Delay(500);
            int playerACurrentMsgCount = playerAreceivedMsg.Count;
            int playerBCurrentMsgCount = playerBreceivedMsg.Count;
            await playerA.InvokeAsync("PlayHand", gameId, hA);
            await playerB.InvokeAsync("PlayHand", gameId, hB);
            await Task.Delay(500);
            Assert.Equal(GameState.Compare, playerAreceivedMsg[playerACurrentMsgCount + 1].Data.State);
            Assert.Equal(GameState.Start, playerAreceivedMsg[playerACurrentMsgCount + 2].Data.State);
            Assert.Equal(GameState.Compare, playerBreceivedMsg[playerBCurrentMsgCount + 1].Data.State);
            Assert.Equal(GameState.Start, playerBreceivedMsg[playerBCurrentMsgCount + 2].Data.State);
        }

        [Theory]
        [InlineData(Hand.Rock, Hand.Scissors)]
        [InlineData(Hand.Scissors, Hand.Paper)]
        [InlineData(Hand.Paper, Hand.Rock)]
        public async Task PlayerA_Should_Win_Round(Hand hA, Hand hB)
        {
            string gameId = await PrepareGame();
            await Task.Delay(500);
            await playerA.InvokeAsync("PlayHand", gameId, hA);
            await playerB.InvokeAsync("PlayHand", gameId, hB);
            await Task.Delay(500);
            Assert.Equal(1, playerAreceivedMsg[^1].Data.PlayersState.Where(p => p.Name == "playerA").First().Point);
            Assert.Equal(0, playerAreceivedMsg[^1].Data.PlayersState.Where(p => p.Name == "playerB").First().Point);
        }

        [Theory]
        [InlineData(new[] { Hand.Scissors, Hand.Rock, Hand.Paper }, new[] { Hand.Paper, Hand.Scissors, Hand.Rock })]
        public async Task PlayerA_Should_Win_Game(Hand[] hA, Hand[] hB)
        {
            string gameId = await PrepareGame();
            await Task.Delay(500);
            int r = 1;
            while (r <= 3)
            {
                await playerA.InvokeAsync("PlayHand", gameId, hA[r - 1]);
                await playerB.InvokeAsync("PlayHand", gameId, hB[r - 1]);
                r++;
            }
            await Task.Delay(500);
            Assert.Equal(GameState.End, playerAreceivedMsg[^1].Data.State);
            Assert.Equal(GameState.End, playerBreceivedMsg[^1].Data.State);
            Assert.Equal(3, playerAreceivedMsg[^1].Data.PlayersState.Where(p => p.Name == "playerA").First().Point);
            Assert.Equal(0, playerAreceivedMsg[^1].Data.PlayersState.Where(p => p.Name == "playerB").First().Point);
        }

        [Fact]
        public async Task PlayerA_Should_Win_Game_When_B_Left()
        {
            string gameId = await PrepareGame();
            await Task.Delay(500);
            await playerA.InvokeAsync("PlayHand", gameId, Hand.Paper);
            await playerB.InvokeAsync("PlayHand", gameId, Hand.Rock);
            await playerB.InvokeAsync("LeftGame", gameId);
            await Task.Delay(500);
            Assert.Equal(GameState.End, playerAreceivedMsg[^1].Data.State);
        }
    }
}
