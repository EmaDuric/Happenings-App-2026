using Happenings.Model.Exceptions;
﻿using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
                throw new BusinessRuleException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new BusinessRuleException("Password is required");

            var email = request.Email.Trim().ToLowerInvariant();

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
                throw new UnauthorizedException("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials");

            var token = GenerateToken(user);

            return new AuthResponse { Token = token };
        }

        public void ChangePassword(int userId, ChangePasswordRequest request)
        {
            var user = _context.Users.Find(userId)
                ?? throw new NotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new BusinessRuleException("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.SaveChanges();
        }

        public string? ForgotPassword(ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new BusinessRuleException("Email is required");

            var email = request.Email.Trim().ToLowerInvariant();
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            // Ne otkrivamo da li email postoji � samo ako postoji generisemo token.
            if (user == null)
                return null;

            // Kriptografski slucajan jednokratni token; u bazu ide samo njegov hash.
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            user.PasswordResetTokenHash = BCrypt.Net.BCrypt.HashPassword(token);
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            _context.SaveChanges();

            // U realnoj aplikaciji token bi se slao mailom; ovdje ga vracamo za demo.
            return token;
        }

        public void ResetPassword(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new BusinessRuleException("Email is required");
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new BusinessRuleException("Reset token is required");
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new BusinessRuleException("New password is required");

            var email = request.Email.Trim().ToLowerInvariant();
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            // Jednaka greska bez obzira na razlog � ne otkrivamo detalje.
            if (user == null
                || string.IsNullOrEmpty(user.PasswordResetTokenHash)
                || user.PasswordResetTokenExpiry == null
                || user.PasswordResetTokenExpiry < DateTime.UtcNow
                || !BCrypt.Net.BCrypt.Verify(request.Token, user.PasswordResetTokenHash))
            {
                throw new BusinessRuleException("Invalid or expired reset token");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            // Jednokratno � ponisti token nakon upotrebe.
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiry = null;
            _context.SaveChanges();
        }

        public UserDto Register(UserInsertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new BusinessRuleException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new BusinessRuleException("Password is required");

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new BusinessRuleException("Username is required");

            // Normalizuj email
            var email = request.Email.Trim().ToLowerInvariant();

            if (_context.Users.Any(x => x.Email == email))
                throw new ConflictException("User already exists");

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