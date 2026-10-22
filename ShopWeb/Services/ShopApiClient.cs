using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ShopWeb.Models.Shop;

namespace ShopWeb.Services;

// Typed HttpClient for the Shop API. BaseAddress/headers are configured in Program.cs.
// Paths are relative (no leading slash) so they append to BaseUrl.
public class ShopApiClient(HttpClient http, ILogger<ShopApiClient> logger) : IShopApiClient
{
    private const string ItemsPath = "api/items";
    private const string OrdersPath = "api/orders";

    // Items

    public async Task<IReadOnlyList<ItemDto>> GetItemsAsync(CancellationToken ct = default)
        => GetListAsync<ItemDto>(ItemsPath, ct);

    public Task<ItemDto?> GetItemAsync(int id, CancellationToken ct = default)
        => GetOrNullAsync<ItemDto>($"{ItemsPath}/{id}", ct);

    public Task<ItemDto> CreateItemAsync(ItemDto item, CancellationToken ct = default)
        => PostAsync(ItemsPath, item, ct);

    public Task<bool> UpdateItemAsync(ItemDto item, CancellationToken ct = default)
        => PutAsync($"{ItemsPath}/{item.Id}", item, ct);

    public Task<bool> DeleteItemAsync(int id, CancellationToken ct = default)
        => DeleteAsync($"{ItemsPath}/{id}", ct);

    // Orders

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken ct = default)
        => await GetListAsync<OrderDto>(OrdersPath, ct);

    public Task<OrderDto?> GetOrderAsync(int id, CancellationToken ct = default)
        => GetOrNullAsync<OrderDto>($"{OrdersPath}/{id}", ct);

    public async Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        using var response = await SendAsync(() => http.PostAsJsonAsync(OrdersPath, request, ct), OrdersPath);

        // 400 = the API rejected the order (bad item, not enough stock): show why
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

            var errors = problem?.Errors.SelectMany(e => e.Value).ToList()
                ?? ["The shop rejected the order."];

            return new CreateOrderResult(null, errors);
        }

        // Anything else that fails (401, 500) throws and shows the unavailable page
        await EnsureSuccessAsync(response, OrdersPath, ct);
        var order = await ReadAsync<OrderDto>(response, OrdersPath, ct);
        return new CreateOrderResult(order, []);
    }

    // Helpers

    private async Task<List<T>> GetListAsync<T>(string path, CancellationToken ct)
    {
        using var response = await SendAsync(() => http.GetAsync(path, ct), path);
        await EnsureSuccessAsync(response, path, ct);
        return await ReadAsync<List<T>>(response, path, ct);
    }

    // Returns null on 404 so callers can return NotFound()
    private async Task<T?> GetOrNullAsync<T>(string path, CancellationToken ct) where T : class
    {
        using var response = await SendAsync(() => http.GetAsync(path, ct), path);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, path, ct);
        return await ReadAsync<T>(response, path, ct);
    }

    private async Task<T> PostAsync<T>(string path, T body, CancellationToken ct)
    {
        using var response = await SendAsync(() => http.PostAsJsonAsync(path, body, ct), path);
        await EnsureSuccessAsync(response, path, ct);
        return await ReadAsync<T>(response, path, ct);
    }

    // Returns false on 404 (the resource no longer exists)
    private async Task<bool> PutAsync<T>(string path, T body, CancellationToken ct)
    {
        using var response = await SendAsync(() => http.PutAsJsonAsync(path, body, ct), path);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, path, ct);
        return true;
    }

    // Returns false on 404 (the resource no longer exists)
    private async Task<bool> DeleteAsync(string path, CancellationToken ct)
    {
        using var response = await SendAsync(() => http.DeleteAsync(path, ct), path);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, path, ct);
        return true;
    }

    // Converts transport failures (API down, timeouts after retries) into ShopApiException
    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send, string path)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Shop API request to {Path} failed", path);
            throw new ShopApiException("The shop service is unavailable. Please try again later.", ex.StatusCode, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "Shop API request to {Path} timed out", path);
            throw new ShopApiException("The shop service took too long to respond.", HttpStatusCode.GatewayTimeout, ex);
        }
        catch (Polly.Timeout.TimeoutRejectedException ex)
        {
            logger.LogError(ex, "Shop API request to {Path} timed out", path);
            throw new ShopApiException("The shop service took too long to respond.", HttpStatusCode.GatewayTimeout, ex);
        }
        catch (Polly.CircuitBreaker.BrokenCircuitException ex)
        {
            logger.LogError(ex, "Shop API circuit is open, skipped request to {Path}", path);
            throw new ShopApiException("The shop service is unavailable. Please try again later.", HttpStatusCode.ServiceUnavailable, ex);
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string path, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        logger.LogWarning("Shop API {Method} {Path} returned {StatusCode}: {Body}",
            response.RequestMessage?.Method, path, (int)response.StatusCode, body);

        var message = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "The shop service rejected the request.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            _ => "The shop service returned an error."
        };
        throw new ShopApiException(message, response.StatusCode);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, string path, CancellationToken ct)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(ct);
        return result ?? throw new ShopApiException($"The shop service returned an empty response for {path}.");
    }
}
