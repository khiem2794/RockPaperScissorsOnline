using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Profile.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.Services
{
    public class ProfileService : IProfileService
    {
        public HttpClient _httpClient { get; set; }
        private readonly IConfiguration _configuration;
        public ProfileService(HttpClient httpClient, IConfiguration configuration)
        {
            this._configuration = configuration;
            this._httpClient = httpClient;
        }

        public async Task<UserInfo> GetUserAuthInfo(string accessToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _configuration["ServicesApi:UserProfile"]);
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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _configuration["ServicesApi:GamesProfile"]);
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
