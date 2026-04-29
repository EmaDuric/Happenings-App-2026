using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Happenings.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly HappeningsContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(HappeningsContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public AuthResponse Login(LoginRequest request)
        {
            var email = request.Email.Trim();
            var password = request.Password.Trim();

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
                throw new Exception("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            var token = GenerateToken(user);

            return new AuthResponse
            {
                Token = token
            };
        }

        public UserDto Register(UserInsertRequest request)
        {
            // VALIDACIJA
            if (_context.Users.Any(x => x.Email == request.Email))
                throw new Exception("User already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email.Trim(),
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                IsOrganizer = request.IsOrganizer // 🔥 BITNO
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var savedUser = _context.Users.FirstOrDefault(x => x.Id == user.Id);

            if (savedUser == null)
                throw new Exception("User was not saved correctly");

            // ORGANIZER LOGIKA
            if (request.IsOrganizer)
            {
                var existingOrganizer = _context.Organizers
                    .FirstOrDefault(x => x.UserId == savedUser.Id);

                if (existingOrganizer == null)
                {
                    var organizer = new Organizer
                    {
                        UserId = savedUser.Id,
                        Name = savedUser.Username,
                        ContactEmail = savedUser.Email, // 🔥 BITNO zbog NOT NULL
                        PhoneNumber = "000000000" // može placeholder
                    };

                    _context.Organizers.Add(organizer);
                    _context.SaveChanges();
                }
            }

            return new UserDto
            {
                Id = savedUser.Id,
                Username = savedUser.Username,
                Email = savedUser.Email
            };
        }

        private string GenerateToken(User user)
        {
            var keyString = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT Key is missing");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyString)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),

                // 🔥 KLJUČNO ZA ROLE
                new Claim("isOrganizer", user.IsOrganizer.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}