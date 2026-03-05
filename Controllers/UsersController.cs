using gsa_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace gsa_api.Controllers
{
    [Route("gsa-api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GsaContext dbc;
        private readonly TokenBlacklister tokenBlacklister;
        private readonly IConfiguration config;
        public UsersController(GsaContext _context, IConfiguration conf, TokenBlacklister blacklister)
        {
            dbc = _context;
            config = conf;
            tokenBlacklister = blacklister;
        }


        // POST api/<UsersController>
        [HttpPost("register")]
        public IResult Register(InputtedUser user)
        {
            if(user is not null)
            {
                if(!new EmailAddressAttribute().IsValid(user.Email))
                {
                    return Results.Json(new { message = "Email not valid." }, statusCode: 422);
                }
                if(dbc.Users.Any(u => u.Email == user.Email))
                {
                    return Results.Json(new { message = "Email has been used." }, statusCode: 422);
                }
                if(!user.Password.Any(Char.IsDigit) || !user.Password.Any(Char.IsLetter) || !user.Password.Any(c => !Char.IsLetter(c) && !Char.IsDigit(c)))
                {
                    return Results.Json(new { message = "Password must contains combination of letter, number and symbol." }, statusCode: 422);
                }
                if(user.Password.Length < 8)
                {
                    return Results.Json(new { message = "Password length must be greater or equal than 8 characters." }, statusCode: 422);
                }
                user.Password = hashSHA256(user.Password);
                dbc.Users.Add(user.toUser());
                dbc.SaveChanges();
                return Results.Json(new { message = "User registered successfully." });
                
            } else
            {
                return Results.StatusCode(400);
            }
        }
        [HttpPost("login")]

        public IResult Login(Credential user)
        {
            if(user is not null)
            {
                if (!new EmailAddressAttribute().IsValid(user.Email))
                {
                    return Results.Json(new { message = "Email not valid." }, statusCode: 422);
                }
                if (!dbc.Users.Any(u => u.Email == user.Email))
                {
                    return Results.Json(new { message = "Invalid email or password." }, statusCode: 401);
                }
                var dbUser = dbc.Users.First(u => u.Email == user.Email);
                Debug.WriteLine(dbUser.PasswordHash);
                Debug.WriteLine(hashSHA256(user.Password));
                Debug.WriteLine(hashSHA256Equal(user.Password, dbUser.PasswordHash));
                if(!hashSHA256Equal(user.Password, dbUser.PasswordHash))
                {
                    return Results.Json(new { message = "Invalid email or password." }, statusCode: 401);
                }
                return Results.Ok(new
                {
                    message = "Login successful!",
                    data = new { userId = dbUser.Id, username = dbUser.Username, role = dbUser.Role, token = GenerateToken(dbUser.Id.ToString(), dbUser.Email) }
                });
            } else
            {
                return Results.StatusCode(400);
            }
        }

        [HttpPost("logout")]
        public IResult Logout()
        {
            if(User is not null)
            {
                var tokId = User.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? "0";
                tokenBlacklister.BlacklistToken(tokId);
                return Results.Json(new {message = "Logout successful."});
            }
            return Results.Json(new {message = "Authorization token missing or invalid." }, statusCode: 401);
        }

        [HttpGet("me")]
        [Authorize]
        public IResult Me()
        {
            return Results.Ok();
        }

        public string GenerateToken(string userId, string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        } 

        public static string hashSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static bool hashSHA256Equal(string input, string hashedStr)
        {
            var hashedInput = hashSHA256(input);
            return StringComparer.OrdinalIgnoreCase.Compare(hashedInput, hashedStr) == 0;
        }
    }


}
