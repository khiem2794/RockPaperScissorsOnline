using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.AuthConfig;
using Play.FunctionalTests;

namespace Play.Tests
{
    public class PlayTestsStartup : Startup
    {
        public PlayTestsStartup(IConfiguration env) : base(env)
        {
        }

        public override void ConfigureAuthService(IServiceCollection services, JwtTokenConfig jwtConfig)
        {
            base.ConfigureAuthService(services, HubIntegrationTests.jwtSampleConfig);
        }

        public override void ConfigureAuth(IApplicationBuilder app)
        {
            base.ConfigureAuth(app);
        }
    }
}
