using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Profile.Models;
using Profile.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Profile.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService profileService;

        public ProfileController(IProfileService profileService)
        {
            this.profileService = profileService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Index");
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> ProfileInfo()
        {
            var userId = Int32.Parse(User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var userInfo = await profileService.GetUserAuthInfo(accessToken);
            var gamesInfo = await profileService.GetUserGameInfo(accessToken);

            return Ok(new ProfileInfo
            {
                User = userInfo,
                Games = gamesInfo,
            });
        }
    }
}
