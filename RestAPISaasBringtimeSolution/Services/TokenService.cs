using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, ILogger<TokenService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<long> roleIds, long? foundationId, string? foundationSlug)
        {
            try
            {
                var jwtKey = _config["Jwt:Key"] ?? "BringtimeSolution_SuperSecretJwtSecurityKey_2026_SecureKey_MustBeLongEnough!";
                var jwtIssuer = _config["Jwt:Issuer"] ?? "BringtimeSolution";
                var jwtAudience = _config["Jwt:Audience"] ?? "BringtimeSolutionAudience";
                var expiryDays = double.TryParse(_config["Jwt:ExpiryInDays"], out var days) ? days : 1.0;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Peran (Roles)
                foreach (var role in roles)
                {
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                        claims.Add(new Claim("role", role));
                    }
                }

                // Custom Role IDs & Yayasan Claims
                var roleIdList = roleIds.ToList();
                var firstRoleId = roleIdList.FirstOrDefault();
                if (firstRoleId > 0)
                {
                    claims.Add(new Claim("roleId", firstRoleId.ToString()));
                }
                claims.Add(new Claim("roleIds", string.Join(",", roleIdList)));

                if (foundationId.HasValue && foundationId.Value > 0)
                {
                    claims.Add(new Claim("foundationId", foundationId.Value.ToString()));
                }

                if (!string.IsNullOrWhiteSpace(foundationSlug))
                {
                    claims.Add(new Claim("foundationSlug", foundationSlug));
                }

                // Expire tepat 1 hari (24 jam)
                var expiresAt = DateTime.UtcNow.AddDays(expiryDays);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expiresAt,
                    Issuer = jwtIssuer,
                    Audience = jwtAudience,
                    SigningCredentials = credentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TokenService.GenerateToken for user: {Email}", user.Email);
                throw;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return null;

                var jwtKey = _config["Jwt:Key"] ?? "BringtimeSolution_SuperSecretJwtSecurityKey_2026_SecureKey_MustBeLongEnough!";
                var jwtIssuer = _config["Jwt:Issuer"] ?? "BringtimeSolution";
                var jwtAudience = _config["Jwt:Audience"] ?? "BringtimeSolutionAudience";

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return null;
            }
        }
    }
}
