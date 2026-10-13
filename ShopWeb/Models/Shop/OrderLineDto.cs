namespace ShopWeb.Models.Shop;

// The tutorial's API sends itemName on each line; this API nests the whole item instead,
// so ItemName is read from it.
public record OrderLineDto(int ItemId, ItemDto? Item, int Quantity, decimal UnitPrice)
{
    public string ItemName => Item?.Name ?? $"Item {ItemId}";

    public decimal LineTotal => Quantity * UnitPrice;
}
