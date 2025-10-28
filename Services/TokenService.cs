using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Parcial2_Ecommerce.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(Parcial2_Ecommerce.Models.User user)
        {
            var claims = new[]
            {
                // Identificador único del usuario
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

                // Email y Rol (para autorización)
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),

                // Si es empresa, asocia el CompanyId (opcional)
                new Claim("companyId", user.CompanyId?.ToString() ?? string.Empty)
            };

            var keyString = _config["Jwt:Key"] ?? "";
            if (keyString.Length < 32)
                throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres. Revisa appsettings(.Development).json");

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}