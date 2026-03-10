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
using BCrypt.Net;

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

            // BCrypt provjera passworda
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
            // hash passworda
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var entity = new User
            {
                Username = request.Username,
                Email = request.Email.Trim(),
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(entity);
            _context.SaveChanges();

            return new UserDto
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email
            };
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Username)
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