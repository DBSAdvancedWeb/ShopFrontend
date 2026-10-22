using ShopWeb.Models.Shop;

namespace ShopWeb.Services;

public interface IShopApiClient
{
    // Items
    Task<IReadOnlyList<ItemDto>> GetItemsAsync(CancellationToken ct = default);
    Task<ItemDto?> GetItemAsync(int id, CancellationToken ct = default);
    Task<ItemDto> CreateItemAsync(ItemDto item, CancellationToken ct = default);
    Task<bool> UpdateItemAsync(ItemDto item, CancellationToken ct = default);
    Task<bool> DeleteItemAsync(int id, CancellationToken ct = default);

    // Orders
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken ct = default);
    Task<OrderDto?> GetOrderAsync(int id, CancellationToken ct = default);
    Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
}
