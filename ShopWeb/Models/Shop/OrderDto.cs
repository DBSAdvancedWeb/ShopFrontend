using System.Text.Json.Serialization;

namespace ShopWeb.Models.Shop;

public record OrderDto(
    int Id,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    // This API's JSON calls the order lines "lines"
    [property: JsonPropertyName("lines")] IReadOnlyList<OrderLineDto> Items)
{
    // This API has no total field, so it's worked out from the lines
    public decimal Total => Items.Sum(i => i.LineTotal);

    public int ItemCount => Items.Sum(i => i.Quantity);
}
