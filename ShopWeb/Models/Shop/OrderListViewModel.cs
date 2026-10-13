namespace ShopWeb.Models.Shop;

public class OrderListViewModel
{
    public IReadOnlyList<OrderDto> Orders { get; init; } = [];
    public IReadOnlyList<string> Statuses { get; init; } = [];
    public string? SelectedStatus { get; init; }
    public decimal TotalValue => Orders.Sum(o => o.Total);
}
