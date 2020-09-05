using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Play.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController: ControllerBase
    {
        private readonly IPlayService _playService;
        public GameController(IPlayService playService)
        {
            this._playService = playService;
        }
        [HttpGet("/")]
        public IActionResult Index()
        {
            return Ok("Index");
        }
        [Authorize]
        [HttpPost("profile")]
        public IActionResult Profile()
        {
            var userId = Int32.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var games = _playService.GetUserPlayData(userId);
            return Ok(games);
        }
    }
}
