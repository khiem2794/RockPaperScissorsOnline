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
            if (_users.Count == 0) _games.Clear();
            var user = new User(Context.ConnectionId, "player " + Context.ConnectionId);
            _ = _users.TryAdd(user.UserName, user);
            Clients.Caller.MessageClient(new Message(MessageType.UserInfo, new { User = user }));
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var name = "player " + Context.ConnectionId;
            _users.TryRemove(name, out _);
            _games.Values.Where(g => g.Players.Count(p => p.User.UserName == name) > 0).ToList().ForEach(async g =>
            {
                g.LeftGame(Context.ConnectionId);
                await UpdateGameToPlayers(g);
            });
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
            _users.TryGetValue("player " + Context.ConnectionId, out var user);
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
            var exist = _games.TryGetValue(gameId, out var game);
            if (exist)
            {
                game.LeftGame(Context.ConnectionId);
                await UpdateGameToPlayers(game);
            }
        }

        private async Task UpdateGameToPlayers(Game game)
        {
            await Clients.Clients(game.Players.Select(player => player.User.ConnectionId).ToList()).MessageClient(new Message(MessageType.GameUpdate, game.UpdateGame()));
            if (game.State == GameState.Compare)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(3000)).ContinueWith(_ => StartRound(game));
            }
            if (game.State == GameState.End)
            {
                _ = _games.TryRemove(game.GameId, out _);
            }
        }

        private async Task StartRound(Game game)
        {
            game.StartRound();
            await Clients.Clients(game.Players.Select(player => player.User.ConnectionId).ToList()).MessageClient(new Message(MessageType.GameUpdate, game.UpdateGame()));
        }
    }
}
