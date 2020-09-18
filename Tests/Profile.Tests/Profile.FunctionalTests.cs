using Auth.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Profile.Services;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Profile.Tests
{
    public class ProfileIntegrationTests
    {
        public static readonly Profile.AuthConfig.JwtTokenConfig jwtSampleConfigProfile = new Profile.AuthConfig.JwtTokenConfig { Audience = "http://sample.com", Issuer = "http://sample.com", Secret = "123456TxHmxtYHCF" };
        private static readonly Auth.Infrastructure.JwtTokenConfig jwtSampleConfigAuth = new Auth.Infrastructure.JwtTokenConfig { Audience = "http://sample.com", Issuer = "http://sample.com", Secret = "123456TxHmxtYHCF", AccessTokenExpiration = 100, RefreshTokenExpiration = 150 };
        private JwtAuthManager _jwtAuthManager = new JwtAuthManager(jwtSampleConfigAuth);
        private readonly TestServer server;
        private readonly HttpClient _httpClient;
        private readonly string accessTokenSample;

        public ProfileIntegrationTests()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<ProfileTestsStartup>()
                .ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IProfileService));

                    services.Remove(descriptor);
                    services.AddScoped<IProfileService, FakeProfileService>();
                });

            server = new TestServer(webHostBuilder);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
            };
            accessTokenSample = _jwtAuthManager.GenerateTokens("Test", claims, DateTime.Now).AccessToken;
            _httpClient = server.CreateClient();
        }

        [Fact]
        public async Task Info_Should_Return_UnAuthorized()
        {
            var response = await _httpClient.GetAsync("/profile/info");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Info_Should_Return_Ok_Status()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessTokenSample);
            var response = await _httpClient.GetAsync("/profile/info");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Info_Should_Return_Test_Data_From_Fake()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessTokenSample);
            var response = await _httpClient.GetAsync("/profile/info");
            var responseJson = await response.Content.ReadAsStringAsync();
            var testDataJson = JsonSerializer.Serialize(FakeProfileService.TestData);
            Assert.Equal(testDataJson, responseJson);
        }
    }
}
