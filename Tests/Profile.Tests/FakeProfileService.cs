using Profile.Models;
using Profile.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Profile.Tests
{
    public class FakeProfileService : IProfileService
    {
        public static ProfileInfo TestData = new ProfileInfo { User = new UserInfo { Id = 1, Email = "test", UserName = "test", RegisteredAt = DateTime.Now }, Games = new List<GameInfo>() };
        public Task<UserInfo> GetUserAuthInfo(string accessToken)
        {
            return Task.FromResult(TestData.User); ;
        }

        public Task<List<GameInfo>> GetUserGameInfo(string accessToken)
        {
            return Task.FromResult(TestData.Games);
        }
    }
}
