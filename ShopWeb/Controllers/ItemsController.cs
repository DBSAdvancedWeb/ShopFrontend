using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopWeb.Models.Shop;
using ShopWeb.Services;

namespace ShopWeb.Controllers;

[Authorize]
public class ItemsController(IShopApiClient shopApi) : Controller
{
    // GET: Items?search=mouse
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        IEnumerable<ItemDto> items = await shopApi.GetItemsAsync(ct);

        if (!string.IsNullOrWhiteSpace(search))
        {
            items = items.Where(i =>
                i.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (i.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var model = new ItemListViewModel
        {
            Items = items.OrderBy(i => i.Name).ToList(),
            Search = search
        };
        return View(model);
    }

    // GET: Items/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var item = await shopApi.GetItemAsync(id, ct);
        return item is null ? NotFound() : View(item);
    }

    // GET: Items/Create
    public IActionResult Create()
    {
        return View(new ItemDto(0, string.Empty, null, 0));
    }

    // POST: Items/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemDto item, CancellationToken ct)
    {
        item = item with { Id = 0 }; // the API assigns the id

        if (!ModelState.IsValid)
        {
            return View(item);
        }

        var created = await shopApi.CreateItemAsync(item, ct);
        TempData["Success"] = $"Item \"{created.Name}\" created.";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    // GET: Items/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await shopApi.GetItemAsync(id, ct);
        return item is null ? NotFound() : View(item);
    }

    // POST: Items/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemDto item, CancellationToken ct)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(item);
        }

        if (!await shopApi.UpdateItemAsync(item, ct))
        {
            return NotFound();
        }

        TempData["Success"] = $"Item \"{item.Name}\" updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Items/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await shopApi.GetItemAsync(id, ct);
        return item is null ? NotFound() : View(item);
    }

    // POST: Items/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        try
        {
            if (!await shopApi.DeleteItemAsync(id, ct))
            {
                return NotFound();
            }
        }
        catch (ShopApiException)
        {
            // The API restricts deleting items that are used on order lines
            TempData["Error"] = "This item couldn't be deleted. It may be part of an existing order.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Item deleted.";
        return RedirectToAction(nameof(Index));
    }
}
