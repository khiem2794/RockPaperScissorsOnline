using Auth.Infrastructure;
using Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private const string TokenKey = "token";
        private readonly IUserService _userService;
        private readonly IJwtAuthManager _jwtAuthManager;

        public UserController(IUserService userService, IJwtAuthManager _jwtAuthManager)
        {
            this._jwtAuthManager = _jwtAuthManager;
            this._userService = userService;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return Ok("Index");
        }

        [HttpGet("jwt")]
        public IActionResult Jwt()
        {
            var token = Request.Cookies[TokenKey];
            if (!String.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var (principal, accessToken) = _jwtAuthManager.DecodeJwtToken(token);
                    if (accessToken != null)
                    {
                        string refreshToken = _jwtAuthManager.GetRefreshTokenWithUserName(principal.Identity.Name);
                        if (!String.IsNullOrEmpty(refreshToken))
                        {
                            LoginResponse loginResponse = new LoginResponse
                            {
                                AccessToken = token,
                                RefreshToken = refreshToken,
                                Username = principal.Identity.Name
                            };
                            return Ok(loginResponse);
                        }
                    }
                }
                catch { }
            }
            ClearTokenCookie();
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await _userService.RegisterUser(request.Username, request.Password, request.Email);
            if (user == null)
                return BadRequest("user already exist");
            var claims = new[] {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            var jwtResult = _jwtAuthManager.GenerateTokens(user.UserName, claims, DateTime.Now);
            LoginResponse loginResult = new LoginResponse
            {
                Username = user.UserName,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString,
            };
            SetTokenCookie(jwtResult.AccessToken);
            return Ok(loginResult);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest();
            var user = await _userService.GetUser(request.Username, request.Password);
            if (user == null)
                return NotFound("user not found");

            var claims = new Claim[] {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            var jwtResult = _jwtAuthManager.GenerateTokens(user.UserName, claims, DateTime.Now);
            LoginResponse loginResult = new LoginResponse
            {
                Username = user.UserName,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString,
            };
            SetTokenCookie(jwtResult.AccessToken);
            return Ok(loginResult);
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            _jwtAuthManager.RemoveRefreshTokenByUserName(User.Identity.Name);
            ClearTokenCookie();
            return Ok();
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken.RefreshToken)) return Unauthorized();
                var userName = User.Identity.Name;
                var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                var jwtResult = _jwtAuthManager.Refresh(refreshToken.RefreshToken, accessToken, DateTime.Now);
                return Ok(new LoginResponse
                {
                    Username = userName,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString,
                });
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message);
            }
        }

        [Authorize]
        [HttpGet("[action]")]
        public IActionResult Profile()
        {
            var userInfo = _userService.GetUserInfo(User.Identity.Name);
            if (userInfo != null)
            {
                return Ok(
                    new UserInfoResponse
                    {
                        Id = userInfo.Id,
                        Username = userInfo.UserName,
                        Email = userInfo.Email,
                        RegisteredAt = userInfo.RegisteredAt,
                    }
                );
            }
            else
            {
                return NotFound();
            }
        }

        private void SetTokenCookie(string token)
        {
            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                MaxAge = TimeSpan.FromMinutes(10),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SameSite = SameSiteMode.Lax
            };
            Response.Cookies.Append(TokenKey, token, cookieOptions);
        }

        private void ClearTokenCookie()
        {
            Response.Cookies.Delete(TokenKey);
        }
    }

    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UserInfoResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
