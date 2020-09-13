using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profile.AuthConfig;

namespace Profile.Tests
{
    public class ProfileTestsStartup : Startup
    {
        public ProfileTestsStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureAuth(IApplicationBuilder app)
        {
            base.ConfigureAuth(app);
        }

        public override void ConfigureAuthService(IServiceCollection services, JwtTokenConfig jwtConfig)
        {
            base.ConfigureAuthService(services, ProfileIntegrationTests.jwtSampleConfigProfile);
        }
    }
}
