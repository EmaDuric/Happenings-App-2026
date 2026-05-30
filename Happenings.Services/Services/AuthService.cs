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
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required");

            var email = request.Email.Trim().ToLowerInvariant();

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
                throw new Exception("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            var token = GenerateToken(user);

            return new AuthResponse { Token = token };
        }

        public void ChangePassword(int userId, ChangePasswordRequest request)
        {
            var user = _context.Users.Find(userId)
                ?? throw new Exception("User not found");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new Exception("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.SaveChanges();
        }

        public UserDto Register(UserInsertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required");

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new Exception("Username is required");

            // Normalizuj email
            var email = request.Email.Trim().ToLowerInvariant();

            if (_context.Users.Any(x => x.Email == email))
                throw new Exception("User already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                IsOrganizer = false, // klijent ne može sam sebi dodijeliti organizer ulogu
                IsAdmin = false      // klijent ne može sam sebi dodijeliti admin ulogu
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        private string GenerateToken(User user)
        {
            var keyString = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT Key is missing");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            string role;
            if (user.IsAdmin)
                role = "Admin";
            else if (user.IsOrganizer)
                role = "Organizer";
            else
                role = "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("isOrganizer", user.IsOrganizer.ToString()),
                new Claim(ClaimTypes.Role, role)
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