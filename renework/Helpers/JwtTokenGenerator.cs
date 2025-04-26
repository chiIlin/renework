// Helpers/JwtTokenGenerator.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace renework.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly JwtSettings _settings;
        private readonly byte[] _key;

        public JwtTokenGenerator(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
            _key = Encoding.UTF8.GetBytes(_settings.Key);
        }

        public string GenerateToken(string userId, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,    userId),
                new Claim(ClaimTypes.NameIdentifier,      userId),
                new Claim(ClaimTypes.Role,                role),
                new Claim(JwtRegisteredClaimNames.Jti,    Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
