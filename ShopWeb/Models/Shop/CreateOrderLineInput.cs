using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopWeb.Models.Shop;

public class CreateOrderLineInput
{
    // Posted back by the form
    public int ItemId { get; set; }

    [Range(0, 1000)]
    public int Quantity { get; set; }

    // Display only: reloaded from the API, never trusted from the form
    [ValidateNever]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}
