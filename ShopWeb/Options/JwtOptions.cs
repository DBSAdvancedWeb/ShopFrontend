using System.ComponentModel.DataAnnotations;

namespace ShopWeb.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required]
    public string Issuer { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 60)]
    public int TokenLifetimeMinutes { get; set; } = 5;
}
