using System.ComponentModel.DataAnnotations;

namespace ShopWeb.Models.Shop;

// Form model for editing an order's header. The lines can't be edited (the API's PUT ignores them).
public class OrderEditViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Customer")]
    public string CustomerName { get; set; } = string.Empty;

    [Display(Name = "Order date")]
    public DateTime OrderDate { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
