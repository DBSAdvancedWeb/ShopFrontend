namespace ShopWeb.Models.Shop;

public record OrderLineDto(int ItemId, string ItemName, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}
