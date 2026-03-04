using gsa_api.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
        public UsersController(GsaContext _context) { dbc = _context; }


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
                if(!hashSHA256Equal(user.Password, dbUser.PasswordHash))
                {
                    return Results.Json(new { message = "Invalid email or password." }, statusCode: 401);
                }
                return Results.Ok(new
                {
                    message = "Login successful!",
                    data = new { userId = dbUser.Id, username = dbUser.Username, role = dbUser.Role, token = "..." }
                });
            } else
            {
                return Results.StatusCode(400);
            }
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
            var comparer = StringComparer.FromComparison(StringComparison.OrdinalIgnoreCase);
            return comparer.Compare(input, hashedStr) == 0;
        }
    }


}
