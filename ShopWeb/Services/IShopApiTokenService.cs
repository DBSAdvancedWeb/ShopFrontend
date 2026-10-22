using System.Security.Claims;

namespace ShopWeb.Services;

public interface IShopApiTokenService
{
    string CreateToken(ClaimsPrincipal user, TimeSpan? lifetime = null);
}
