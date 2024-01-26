using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ramand.Models;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ramand.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IConfiguration _configuration;
        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("getAll")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetUser>>> GetAll()
        {
            var ConnectionStrings = _configuration.GetConnectionString("MyConnection");
            using var connection = new SqlConnection(ConnectionStrings);
            var result = await connection.QueryAsync<Users>("SELECT UserName FROM Users");
            return Ok(result);
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(Register register)
        {
            string token = CreateToken();
            var user = new Users
            {
                UserName = register.UserName,
                Password = register.Password,
                VerificationToken = token,
            };

            var ConnectionStrings = _configuration.GetConnectionString("MyConnection");
            using var db = new SqlConnection(_configuration.GetConnectionString("MyConnection"));
            await db.ExecuteAsync("INSERT INTO Users (UserName, Password, VerificationToken) VALUES (@UserName, @Password, @VerificationToken)", user);
            return Ok();
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(Login login)
        {
            var ConnectionStrings = _configuration.GetConnectionString("MyConnection");
            using var connection = new SqlConnection(ConnectionStrings);
            var user = await connection.QueryFirstOrDefaultAsync<Users>("SELECT * FROM Users WHERE UserName = @UserName", login);

            string token = CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken, user.Id);
            return Ok(token);
        }

        private string CreateToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        private string CreateJwtToken(Users user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = CreateToken(),
                Expires = DateTime.Now.AddDays(3),
                Created = DateTime.Now
            };

            return refreshToken;
        }
        private async void SetRefreshToken(RefreshToken newRefreshToken, int userId)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };

            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            using var connection = new SqlConnection(_configuration.GetConnectionString("MyConnection"));

            await connection.ExecuteAsync("UPDATE Users SET refreshToken = @Token, tokenCreated = @Created, tokenExpires = @Expires WHERE id = @Id", new { Token = newRefreshToken.Token, Created = newRefreshToken.Created, Expires = newRefreshToken.Expires, Id = userId });
        }
    }
}