using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Play.Enum;
using Play.Models;
using Play.Models.PlayModel;
using Play.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Play.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GameHub : Hub<IGameHub>
    {
        private static readonly ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();
        private static readonly ConcurrentDictionary<string, IGame> _games = new ConcurrentDictionary<string, IGame>();
        private readonly IPlayService _playDataService;

        public GameHub(IPlayService playDataService)
        {
            this._playDataService = playDataService;
        }

        public override Task OnConnectedAsync()
        {
            if (_users.TryGetValue(Context.User.Identity.Name, out _))
            {
                Context.Abort();
                return Task.CompletedTask;
            }
            else
            {
                var user = new User(Context.ConnectionId, Int32.Parse(Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value), Context.User.Identity.Name);
                _ = _users.TryAdd(user.UserName, user);
                Clients.Caller.MessageClient(new Message(MessageType.ConnectionEstablished, null));
                return base.OnConnectedAsync();
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_users.TryGetValue(Context.User.Identity.Name, out var user))
            {
                if (user.ConnectionId != Context.ConnectionId)
                {
                    Context.Abort();
                }
                else
                {
                    _users.TryRemove(Context.User.Identity.Name, out _);
                    var currentGames = _games.Values.Where(g => g.Players.Count(p => p.User.UserName == Context.User.Identity.Name) > 0).ToList();
                    foreach (var g in currentGames)
                    {
                        if (g.State == GameState.Waiting)
                        {
                            _ = _games.TryRemove(g.GameId, out _);
                        }
                        if (g.State != GameState.End)
                        {
                            g.LeftGame(Context.ConnectionId);
                            await UpdateGameToPlayers(g);
                        }
                    }
                    await base.OnDisconnectedAsync(exception);
                }
            }
        }
        public async Task CreateGame()
        {
            _users.TryGetValue(Context.User.Identity.Name, out var user);
            var game = new Game();
            var player = new Player(user);
            game.AddPlayer(player);
            _games.TryAdd(game.GameId, game);
            await UpdateGameToPlayers(game);
        }

        public async Task JoinGame(string gameId)
        {
            _users.TryGetValue(Context.User.Identity.Name, out var user);
            var exist = _games.TryGetValue(gameId, out IGame game);
            var player = new Player(user);
            if (exist && game.AddPlayer(player))
            {
                await UpdateGameToPlayers(game);
            }
        }

        public async Task JoinWaitingGame()
        {
            _users.TryGetValue(Context.User.Identity.Name, out var user);
            var currentGames = _games.Select(v => v.Value).Where(g => g.HasPlayer(Context.ConnectionId)).ToList();
            foreach (var g in currentGames)
            {
                if (g.State == GameState.Waiting)
                {
                    _ = _games.TryRemove(g.GameId, out _);
                }
                if (g.State != GameState.End)
                {
                    g.LeftGame(Context.ConnectionId);
                    await UpdateGameToPlayers(g);
                }
            }
            var game = _games.FirstOrDefault(g => g.Value.State == GameState.Waiting).Value;
            if (game != null)
            {
                var player = new Player(user);
                if (game.AddPlayer(player))
                {
                    await UpdateGameToPlayers(game);
                }
            }
        }

        public async Task PlayHand(string gameId, Hand hand)
        {
            var exist = _games.TryGetValue(gameId, out var game);
            if (exist && game.State == GameState.Start)
            {
                game.PlayHand(Context.ConnectionId, hand);
                await UpdateGameToPlayers(game);
                if (game.State == GameState.Compare)
                {
                    await StartRound(game);
                }
            }
        }

        public async Task LeftGame(string gameId)
        {
            bool exist = _games.TryGetValue(gameId, out var game);
            if (exist)
            {
                if (game.LeftGame(Context.ConnectionId))
                {
                    await UpdateGameToPlayers(game);
                }
                _ = _games.TryRemove(game.GameId, out _);
            }
        }

        private async Task UpdateGameToPlayers(IGame game)
        {
            var clients = Clients.Clients(game.Players.Where(p => !p.LeftGame).Select(player => player.User.ConnectionId).ToList());
            await clients.MessageClient(new Message(MessageType.GameUpdate, game.UpdateGame()));
            if (game.State == GameState.End)
            {
                await SaveGameData(game);
            }
        }

        private async Task StartRound(IGame game)
        {
            game.StartRound();
            var clients = Clients.Clients(game.Players.Where(p => !p.LeftGame).Select(player => player.User.ConnectionId).ToList());
            await clients.MessageClient(new Message(MessageType.GameUpdate, game.UpdateGame()));
        }

        private async Task SaveGameData(IGame game)
        {
            PlayData data = new PlayData
            {
                User1Id = game.Players[0].User.Id,
                User2Id = game.Players[1].User.Id,
                GameDate = DateTime.Now,
                WinnerId = game.WinnerId,
            };
            await _playDataService.SaveGameAsync(data);
        }
    }
}
