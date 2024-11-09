using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using CropChainBackend.Data;
using CropChainBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace CropChainBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult RegisterUser(User user)
        {
            //Check if user already exists
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest(new { message = "User already exists" });
            }

            //Else Hash password and register user
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();


            return Ok(new { message = "User Registered Successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            //Check if user present in DB
            var userDetails = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userDetails == null || !BCrypt.Net.BCrypt.Verify(user.Password, userDetails.Password))
            {
                return Unauthorized("Invalid Credentials");
            }

            //Else Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token) });
        }


        [Authorize]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            return Ok(_context.Users.ToList());
        }

    }
}