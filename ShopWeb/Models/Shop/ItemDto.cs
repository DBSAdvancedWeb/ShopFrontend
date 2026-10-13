using System.ComponentModel.DataAnnotations;

namespace ShopWeb.Models.Shop;

// StockQuantity and the validation attributes go beyond the tutorial:
// the item create/edit forms bind straight to this record.
public record ItemDto(
    int Id,
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(0, 100000)] decimal Price,
    [Range(0, int.MaxValue)][property: Display(Name = "Stock")] int StockQuantity = 0);
