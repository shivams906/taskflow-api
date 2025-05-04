using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskFlowAPI.Data;
using TaskFlowAPI.Models;
using TaskFlowAPI.Settings;
using LoginRequest = TaskFlowAPI.Models.LoginRequest;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(IOptions<JwtSettings> jwtSettings, AppDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _jwtSettings = jwtSettings.Value;
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] LoginRequest request)
        {
            if (_db.Users.Any(u => u.Username == request.Username))
                return BadRequest("Username already exists");

            var user = new User { Username = request.Username };
            user.CreatedAtUtc = DateTime.UtcNow;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok("User registered successfully");
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result != PasswordVerificationResult.Success)
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpGet("secret")]
        [Authorize]
        public IActionResult GetSecret()
        {
            return Ok("This is a protected endpoint!");
        }



        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
