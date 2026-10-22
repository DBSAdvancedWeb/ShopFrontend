namespace ShopWeb.Models.Shop;

// Either the new order or the API's validation messages
public record CreateOrderResult(OrderDto? Order, IReadOnlyList<string> Errors)
{
    public bool Succeeded => Order is not null;
}
