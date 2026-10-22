namespace ShopWeb.Models.Shop;

public record OrderDto(
    int Id,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    decimal Total,
    IReadOnlyList<OrderLineDto> Items)
{
    public int ItemCount => Items.Sum(i => i.Quantity);
}
