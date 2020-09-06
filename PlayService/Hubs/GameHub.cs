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
        private static readonly ConcurrentDictionary<string, Game> _games = new ConcurrentDictionary<string, Game>();
        private readonly IPlayService _playDataService;

        public GameHub(IPlayService playDataService)
        {
            this._playDataService = playDataService;
        }

        public override Task OnConnectedAsync()
        {
            if (_users.Count == 0) _games.Clear();
            var user = new User(Context.ConnectionId, Int32.Parse(Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value), Context.User.Identity.Name);
            _ = _users.TryAdd(user.UserName, user);
            Clients.Caller.MessageClient(new Message(MessageType.UserInfo, new { User = user }));
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _users.TryRemove(Context.User.Identity.Name, out _);
            _games.Values.Where(g => g.Players.Count(p => p.User.UserName == Context.User.Identity.Name) > 0).ToList().ForEach(async g =>
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
            });
            return base.OnDisconnectedAsync(exception);
        }
        public async Task CreateGame()
        {
            _users.TryGetValue(Context.User.Identity.Name, out var user);
            var game = new Game();
            var player = new Player(user);
            game.Players.Add(player);
            _games.TryAdd(game.GameId, game);
            game.Initialize();
            await UpdateGameToPlayers(game);
        }

        public async Task JoinGame(string gameId)
        {
            _users.TryGetValue(Context.User.Identity.Name, out var user);
            var exist = _games.TryGetValue(gameId, out Game game);
            if (exist && game.Players.Count < Game.MaxPlayers)
            {
                var player = new Player(user);
                game.Players.Add(player);
                _games.TryAdd(game.GameId, game);
                if (game.Players.Count == Game.MaxPlayers) game.StartRound();
                await UpdateGameToPlayers(game);
            }
        }

        public async Task JoinWaitingGame()
        {
            _users.TryGetValue(Context.User.Identity.Name, out var user);
            _games.Select(v => v.Value).Where(g => g.hasPlayer(Context.ConnectionId)).ToList().ForEach(async g => await this.LeftGame(g.GameId));
            var game = _games.FirstOrDefault(g => g.Value.Players.Count < Game.MaxPlayers).Value;
            if (game != null)
            {
                if (game.Players.Count < Game.MaxPlayers)
                {
                    var player = new Player(user);
                    game.Players.Add(player);
                    _games.TryAdd(game.GameId, game);
                    if (game.Players.Count == Game.MaxPlayers) game.StartRound();
                    await UpdateGameToPlayers(game);
                }
            }
        }

        public async Task PlayHand(string gameId, Hand hand)
        {
            var exist = _games.TryGetValue(gameId, out var game);
            if (exist)
            {
                lock (game)
                {
                    game.PlayHand(Context.ConnectionId, hand);
                }
                await UpdateGameToPlayers(game);
            }
        }

        public async Task LeftGame(string gameId)
        {
            bool exist = _games.TryGetValue(gameId, out var game);
            if (exist)
            {
                if (game.State == GameState.End) return;
                game.LeftGame(Context.ConnectionId);
                if (game.State == GameState.Waiting)
                {
                    _ = _games.TryRemove(game.GameId, out _);
                }
                else
                {
                    await UpdateGameToPlayers(game);
                }
            }
        }

        private async Task UpdateGameToPlayers(Game game)
        {
            await Clients.Clients(game.Players.Where(p => !p.LeftGame).Select(player => player.User.ConnectionId).ToList()).MessageClient(new Message(MessageType.GameUpdate, game.UpdateGame()));
            if (game.State == GameState.Compare)
            {
                await StartRound(game);
            }
            if (game.State == GameState.End)
            {
                PlayData data = new PlayData
                {
                    User1Id = game.Players[0].User.Id,
                    User2Id = game.Players[1].User.Id,
                    GameDate = DateTime.Now,
                    WinnerId = game.WinnerId,
                };
                await _playDataService.SaveGameAsync(data);
                _ = _games.TryRemove(game.GameId, out _);
            }
        }

        private async Task StartRound(Game game)
        {
            if (game.State != GameState.End)
            {
                game.StartRound();
                await Clients.Clients(game.Players.Where(p => !p.LeftGame).Select(player => player.User.ConnectionId).ToList()).MessageClient(new Message(MessageType.GameUpdate, game.UpdateGame()));
            }
        }
    }
}
