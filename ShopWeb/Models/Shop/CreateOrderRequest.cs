namespace ShopWeb.Models.Shop;

public record CreateOrderRequest(IReadOnlyList<CreateOrderLineRequest> Lines);
