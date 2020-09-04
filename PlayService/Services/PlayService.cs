using PlayService.Data;
using PlayService.Models.PlayModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayService.Services
{
    public interface IPlayDataService {
        void SaveGame(PlayData data);
    }
    public class PlayDataService : IPlayDataService
    {
        private readonly PlayDbContext _playDbContext;

        public PlayDataService(PlayDbContext playDbContext)
        {
            this._playDbContext = playDbContext;
        }

        public void SaveGame(PlayData data)
        {
            _playDbContext.Plays.Add(data);
            _playDbContext.SaveChangesAsync();
        }
    }
}
