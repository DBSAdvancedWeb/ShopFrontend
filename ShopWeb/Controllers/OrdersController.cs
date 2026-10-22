using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopWeb.Models.Shop;
using ShopWeb.Services;

namespace ShopWeb.Controllers;

[Authorize]
public class OrdersController(IShopApiClient shopApi) : Controller
{
    // GET /Orders?status=Pending
    public async Task<IActionResult> Index(string? status, CancellationToken ct)
    {
        var allOrders = await shopApi.GetOrdersAsync(ct);

        var orders = string.IsNullOrWhiteSpace(status)
            ? allOrders
            : allOrders.Where(o => string.Equals(o.Status, status, StringComparison.OrdinalIgnoreCase));

        var model = new OrderListViewModel
        {
            Orders = orders.OrderByDescending(o => o.OrderDate).ToList(),
            // Built from the data so the dropdown only offers statuses that exist
            Statuses = allOrders.Select(o => o.Status).Distinct().Order().ToList(),
            SelectedStatus = status
        };

        return View(model);
    }

    // GET /Orders/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var order = await shopApi.GetOrderAsync(id, ct);
        return order is null ? NotFound() : View(order);
    }

    // GET /Orders/Create or /Orders/Create?itemId=3 (pre-selects one of that item)
    public async Task<IActionResult> Create(int? itemId, CancellationToken ct)
    {
        var quantities = new Dictionary<int, int>();
        if (itemId is int id)
        {
            quantities[id] = 1;
        }

        return View(await BuildCreateModelAsync(quantities, ct));
    }

    // POST /Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel input, CancellationToken ct)
    {
        var quantities = input.Lines
            .Where(l => l.Quantity > 0)
            .GroupBy(l => l.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        if (quantities.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Choose a quantity for at least one item.");
        }

        if (ModelState.IsValid)
        {
            var request = new CreateOrderRequest(
                quantities.Select(q => new CreateOrderLineRequest(q.Key, q.Value)).ToList());

            var result = await shopApi.CreateOrderAsync(request, ct);

            if (result.Order is not null)
            {
                TempData["Success"] = $"Order {result.Order.Id} placed.";
                return RedirectToAction(nameof(Details), new { id = result.Order.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        // Redisplay with fresh names, prices and stock from the API
        return View(await BuildCreateModelAsync(quantities, ct));
    }

    private async Task<CreateOrderViewModel> BuildCreateModelAsync(
        IReadOnlyDictionary<int, int> quantities, CancellationToken ct)
    {
        var items = await shopApi.GetItemsAsync(ct);

        return new CreateOrderViewModel
        {
            Lines = items
                .OrderBy(i => i.Name)
                .Select(i => new CreateOrderLineInput
                {
                    ItemId = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    StockQuantity = i.StockQuantity,
                    Quantity = quantities.GetValueOrDefault(i.Id)
                })
                .ToList()
        };
    }
}
