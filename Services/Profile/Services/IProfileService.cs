using Profile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Profile.Services
{
    public interface IProfileService
    {

        public Task<UserInfo> GetUserAuthInfo(string accessToken);
        public Task<List<GameInfo>> GetUserGameInfo(string accessToken);
    }
}
