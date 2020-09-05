using Microsoft.AspNetCore.Authentication.JwtBearer;
using Profile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.Services
{
    public class ProfileService : IProfileService
    {
        public HttpClient _httpClient { get; set; }
        public ProfileService(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<UserInfo> GetUserAuthInfo(string accessToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:6001/user/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<UserInfo>(responseStream);
            }
            return null;
        }

        public async Task<List<GameInfo>> GetUserGameInfo(string accessToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5001/game/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<List<GameInfo>>(responseStream);

            }
            return null;
        }
    }
}
