using Microsoft.AspNetCore.SignalR;
using PlayService.Enum;
using PlayService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlayService.Hubs
{
    public class GameHub : Hub<IGameHub>
    {
        private static readonly ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();
        private static readonly ConcurrentDictionary<string, Game> _games = new ConcurrentDictionary<string, Game>();
        public override Task OnConnectedAsync()
        {
            var user = new User(Context.ConnectionId,"player " + Context.ConnectionId);
            _ = _users.TryAdd(user.UserName, user);
            Clients.Caller.MessageClient(new Message(MessageType.UserInfo, new { User = user }));
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _users.TryRemove("player " + Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }
        public async Task CreateGame()
        {
            _users.TryGetValue("player " + Context.ConnectionId, out var user);
            var game = new Game();
            var player = new Player(user);
            game.Players.Add(player);
            _games.TryAdd(game.GameId, game);
            game.Initialize();
            await UpdateGameToPlayers(game);
        }

        public async Task JoinGame(string gameId)
        {
            _users.TryGetValue("player " + Context.ConnectionId, out var user);
            _ = _games.TryGetValue(gameId, out Game _);
            if (_games.Count > 0)
            {
                Game game = _games.ToList()[0].Value;
                if (game?.Players.Count < Game.MaxPlayers)
                {
                    var player = new Player(user);
                    game.Players.Add(player);
                    _games.TryAdd(game.GameId, game);
                    if (game.Players.Count == Game.MaxPlayers) game.StartGame();
                    await UpdateGameToPlayers(game);
                }
            }
        }

        public async Task PlayHand(string gameId, Hand hand)
        {
            _games.TryGetValue(gameId, out var game);
            lock (game)
            {
                game.PlayHand(Context.ConnectionId, hand);
            }
            await UpdateGameToPlayers(game);
        }

        private async Task UpdateGameToPlayers(Game game)
        {
            await Clients.Clients(game.Players.Select(player => player.User.ConnectionId).ToList()).MessageClient(new Message(MessageType.GameUpdate, game.UpdateGame()));
        }
    }
}
