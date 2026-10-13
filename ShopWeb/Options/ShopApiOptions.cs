using System.ComponentModel.DataAnnotations;

namespace ShopWeb.Options;

public class ShopApiOptions
{
    public const string SectionName = "ShopApi";

    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;
}
