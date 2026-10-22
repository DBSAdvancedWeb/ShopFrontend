namespace ShopWeb.Models.Shop;

public class CreateOrderViewModel
{
    public List<CreateOrderLineInput> Lines { get; set; } = [];

    public decimal EstimatedTotal => Lines.Sum(l => l.Quantity * l.Price);
}
