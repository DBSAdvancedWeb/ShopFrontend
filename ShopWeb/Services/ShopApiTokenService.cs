using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ShopWeb.Options;

namespace ShopWeb.Services;

public class ShopApiTokenService(IOptions<JwtOptions> options) : IShopApiTokenService
{
    private readonly JsonWebTokenHandler _handler = new();

    public string CreateToken(ClaimsPrincipal user, TimeSpan? lifetime = null)
    {
        var jwt = options.Value;

        // Identity puts the user's id in the NameIdentifier claim of the cookie
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("The signed-in user has no id claim.");
        var name = user.Identity?.Name ?? userId;
        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? name;

        var now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(lifetime ?? TimeSpan.FromMinutes(jwt.TokenLifetimeMinutes)),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = userId,
                [JwtRegisteredClaimNames.Name] = name,
                [JwtRegisteredClaimNames.Email] = email,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return _handler.CreateToken(descriptor);
    }
}
