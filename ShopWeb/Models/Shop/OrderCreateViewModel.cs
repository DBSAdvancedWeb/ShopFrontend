using System.ComponentModel.DataAnnotations;

namespace ShopWeb.Models.Shop;

// Form model for placing a new order: one quantity box per available item.
public class OrderCreateViewModel
{
    [Required, MaxLength(100)]
    [Display(Name = "Customer name")]
    public string CustomerName { get; set; } = string.Empty;

    public List<OrderCreateLineViewModel> Lines { get; set; } = [];
}

public class OrderCreateLineViewModel
{
    public int ItemId { get; set; }

    // Display only - refreshed from the API on post so prices can't be tampered with
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    [Range(0, 1000)]
    public int Quantity { get; set; }
}
