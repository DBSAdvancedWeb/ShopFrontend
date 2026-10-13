namespace ShopWeb.Models.Shop;

public class ItemListViewModel
{
    public IReadOnlyList<ItemDto> Items { get; init; } = [];
    public string? Search { get; init; }
}
