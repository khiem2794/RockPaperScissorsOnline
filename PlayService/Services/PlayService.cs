using Play.Data;
using Play.Models.PlayModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Services
{
    public interface IPlayService {
        void SaveGame(PlayData data);
        List<PlayData> GetUserPlayData(int userId);
    }
    public class PlayService : IPlayService
    {
        private readonly PlayDbContext _playDbContext;

        public PlayService(PlayDbContext playDbContext)
        {
            this._playDbContext = playDbContext;
        }

        public List<PlayData> GetUserPlayData(int userId)
        {
            return _playDbContext.Plays.Where(p => p.User1Id == userId || p.User2Id == userId).ToList();
        }

        public void SaveGame(PlayData data)
        {
            _playDbContext.Plays.Add(data);
            _playDbContext.SaveChangesAsync();
        }
    }
}
