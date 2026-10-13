using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopWeb.Models.Shop;
using ShopWeb.Services;

namespace ShopWeb.Controllers;

[Authorize]
public class OrdersController(IShopApiClient shopApi) : Controller
{
    public static readonly string[] Statuses = ["Pending", "Processing", "Shipped", "Completed", "Cancelled"];

    // GET: Orders?status=Shipped
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

    // GET: Orders/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var order = await shopApi.GetOrderAsync(id, ct);
        return order is null ? NotFound() : View(order);
    }

    // GET: Orders/Create
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var items = await shopApi.GetItemsAsync(ct);
        var model = new OrderCreateViewModel
        {
            Lines = items.Select(i => new OrderCreateLineViewModel
            {
                ItemId = i.Id,
                Name = i.Name,
                Price = i.Price,
                StockQuantity = i.StockQuantity
            }).ToList()
        };
        return View(model);
    }

    // POST: Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateViewModel model, CancellationToken ct)
    {
        // Re-read items so names, prices and stock come from the API, not the form
        var items = (await shopApi.GetItemsAsync(ct)).ToDictionary(i => i.Id);

        model.Lines = model.Lines.Where(l => items.ContainsKey(l.ItemId)).ToList();
        for (var i = 0; i < model.Lines.Count; i++)
        {
            var line = model.Lines[i];
            var item = items[line.ItemId];
            line.Name = item.Name;
            line.Price = item.Price;
            line.StockQuantity = item.StockQuantity;

            if (line.Quantity > item.StockQuantity)
            {
                ModelState.AddModelError($"Lines[{i}].Quantity", $"Only {item.StockQuantity} in stock.");
            }
        }

        if (!model.Lines.Any(l => l.Quantity > 0))
        {
            ModelState.AddModelError(string.Empty, "Choose a quantity for at least one item.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var order = new OrderDto(
            Id: 0,
            CustomerName: model.CustomerName,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            Items: model.Lines
                .Where(l => l.Quantity > 0)
                .Select(l => new OrderLineDto(l.ItemId, null, l.Quantity, l.Price))
                .ToList());

        var created = await shopApi.CreateOrderAsync(order, ct);
        TempData["Success"] = $"Order #{created.Id} placed.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    // GET: Orders/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var order = await shopApi.GetOrderAsync(id, ct);
        if (order is null)
        {
            return NotFound();
        }

        return View(new OrderEditViewModel
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            OrderDate = order.OrderDate,
            Status = order.Status
        });
    }

    // POST: Orders/Edit/5
    // Only the order header is editable - the API's PUT doesn't update lines.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrderEditViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!Statuses.Contains(model.Status))
        {
            ModelState.AddModelError(nameof(model.Status), "Choose a valid status.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Lines are sent empty: the API's PUT only updates the order itself
        var order = new OrderDto(model.Id, model.CustomerName, model.OrderDate, model.Status, []);
        if (!await shopApi.UpdateOrderAsync(order, ct))
        {
            return NotFound();
        }

        TempData["Success"] = $"Order #{id} updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Orders/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var order = await shopApi.GetOrderAsync(id, ct);
        return order is null ? NotFound() : View(order);
    }

    // POST: Orders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        if (!await shopApi.DeleteOrderAsync(id, ct))
        {
            return NotFound();
        }

        TempData["Success"] = $"Order #{id} deleted.";
        return RedirectToAction(nameof(Index));
    }
}
